using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;

namespace ClientB
{
    public partial class MonitoringForm : Form
    {
        // Khai báo bộ đếm hiệu năng (Đã xóa ramCounter vì không cần dùng nữa)
        private PerformanceCounter cpuCounter;

        // Biến lưu trữ giá trị mạng để tính toán tốc độ (Kbps)
        private long prevBytesReceived = 0;
        private long prevBytesSent = 0;

        // Hardcode thông tin định danh của Client B
        private int currentClientId = 1;
        private string machineName = Environment.MachineName;
        private string ipAddress = "127.0.0.1"; // Tạm fix cứng IP LAN để test

        private StreamWriter currentWriter;

        // Biến toàn cục duy trì đường hầm mạng mTLS
        private TcpClient currentClient;
        private SslStream currentSslStream;

        public MonitoringForm()
        {
            InitializeComponent();
            InitializeCounters();
            ipAddress = GetLocalIPAddress();
        }

        // ==========================================
        // 1. KHỞI TẠO BỘ ĐẾM PHẦN CỨNG
        // ==========================================
        private void InitializeCounters()
        {
            try
            {
                // Khởi tạo đo phần trăm CPU
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                // Lấy đà cho CPU
                cpuCounter.NextValue();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể khởi tạo bộ đếm phần cứng: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // 2. XỬ LÝ NÚT START (MỞ ĐƯỜNG HẦM mTLS)
        // ==========================================
        private async void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            lblStatus.Text = "Trạng thái: Đang thiết lập kênh mTLS...";
            lblStatus.ForeColor = System.Drawing.Color.DarkOrange;

            bool isConnected = await ConnectToServerAsync();

            if (isConnected)
            {
                btnStop.Enabled = true;
                lblStatus.Text = "Trạng thái: Đang gửi dữ liệu (OK)";
                lblStatus.ForeColor = System.Drawing.Color.Green;
                timerPush.Start();
            }
            else
            {
                btnStart.Enabled = true;
            }
        }

        // ==========================================
        // 3. HÀM KẾT NỐI VÀ XÁC THỰC mTLS
        // ==========================================
        private async Task<bool> ConnectToServerAsync()
        {
            try
            {
                currentClient = new TcpClient();
                currentClient.NoDelay = true;

                var connectTask = currentClient.ConnectAsync("127.0.0.1", 8888); // Lưu ý: Port bên Server là 5000 nhé (code cũ của bạn đang để 8888)
                if (await Task.WhenAny(connectTask, Task.Delay(2000)).ConfigureAwait(false) != connectTask)
                    throw new TimeoutException("Máy chủ không phản hồi.");

                await connectTask.ConfigureAwait(false);

                NetworkStream netStream = currentClient.GetStream();
                currentSslStream = new SslStream(netStream, false, ValidateServerCertificate);

                var clientCertificate = new X509Certificate2("ClientCertECC.pfx", "NT106.Q23");
                var clientCerts = new X509CertificateCollection(new X509Certificate[] { clientCertificate });

                await currentSslStream.AuthenticateAsClientAsync("RemoteMonitorServer", clientCerts, SslProtocols.Tls12, false).ConfigureAwait(false);

                currentWriter = new StreamWriter(currentSslStream, Encoding.UTF8) { AutoFlush = true };

                return true;
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() => {
                    lblStatus.Text = "Lỗi kết nối: " + ex.Message;
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                }));
                return false;
            }
        }

        // ==========================================
        // 4. XỬ LÝ NÚT STOP
        // ==========================================
        private void btnStop_Click(object sender, EventArgs e)
        {
            timerPush.Stop();

            currentWriter?.Close();
            currentSslStream?.Close();
            currentClient?.Close();

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            lblStatus.Text = "Trạng thái: Đã dừng.";
            lblStatus.ForeColor = System.Drawing.Color.Red;
        }

        // ==========================================
        // 5. TIMER CHẠY NGẦM GỬI DỮ LIỆU
        // ==========================================
        private bool _isSending = false;
        private async void timerPush_Tick(object sender, EventArgs e)
        {
            if (_isSending) return;
            _isSending = true;

            try
            {
                if (currentSslStream == null || currentClient == null || !currentClient.Connected)
                {
                    btnStop_Click(null, null);
                    return;
                }

                // --- 1. LẤY CPU ---
                float cpuVal = await Task.Run(() => cpuCounter.NextValue()).ConfigureAwait(false);
                string cpu = Math.Round(cpuVal, 1).ToString();

                // --- 2. LẤY % RAM ĐÃ SỬ DỤNG (FIX CHUẨN) ---
                Microsoft.VisualBasic.Devices.ComputerInfo ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
                double totalRamMB = ci.TotalPhysicalMemory / (1024.0 * 1024.0);
                double availableRamMB = ci.AvailablePhysicalMemory / (1024.0 * 1024.0);
                double usedRamMB = totalRamMB - availableRamMB;
                double ramUsagePercent = (usedRamMB / totalRamMB) * 100;
                string ramUsage = Math.Round(ramUsagePercent, 1).ToString();

                // --- 3. LẤY % ĐĨA CỨNG ---
                DriveInfo drive = new DriveInfo("C");
                double diskFreePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;
                string disk = Math.Round(100 - diskFreePercent, 1).ToString();

                // --- 4. LẤY MẠNG ---
                long currentReceived = 0;
                long currentSent = 0;

                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        currentReceived += ni.GetIPv4Statistics().BytesReceived;
                        currentSent += ni.GetIPv4Statistics().BytesSent;
                    }
                }

                double downloadSpeedKBps = 0;
                double uploadSpeedKBps = 0;

                if (prevBytesReceived != 0 && prevBytesSent != 0)
                {
                    downloadSpeedKBps = (currentReceived - prevBytesReceived) / 2.0 / 1024.0;
                    uploadSpeedKBps = (currentSent - prevBytesSent) / 2.0 / 1024.0;
                }

                prevBytesReceived = currentReceived;
                prevBytesSent = currentSent;

                string netDown = Math.Round(downloadSpeedKBps, 1).ToString();
                string netUp = Math.Round(uploadSpeedKBps, 1).ToString();

                // --- 4.5. LẤY DANH SÁCH ỨNG DỤNG ĐANG CHẠY ---
                // Dùng StringBuilder để nối chuỗi cho mượt
                StringBuilder appBuilder = new StringBuilder();
                Process[] processes = Process.GetProcesses(); // Lấy toàn bộ process trên máy

                foreach (Process p in processes)
                {
                    try
                    {
                        // Chỉ lấy những phần mềm có giao diện cửa sổ (MainWindowTitle) để lọc bỏ các Service chạy ngầm
                        if (!string.IsNullOrEmpty(p.MainWindowTitle))
                        {
                            // Lấy lượng RAM mà phần mềm đang chiếm (đổi từ Byte sang MB)
                            long memoryMB = p.WorkingSet64 / (1024 * 1024);

                            // Nối vào chuỗi theo định dạng: Tên_Process.exe|Tiêu_đề_cửa_sổ|RAM_MB;
                            appBuilder.Append($"{p.ProcessName}.exe|{p.MainWindowTitle}|{memoryMB} MB;");
                        }
                    }
                    catch
                    {
                        // Bỏ qua các process hệ thống bị từ chối quyền truy cập (Access Denied)
                    }
                }

                string appList = appBuilder.ToString();
                // Bỏ dấu ';' ở cuối cùng nếu có
                if (appList.EndsWith(";")) appList = appList.TrimEnd(';');
                if (string.IsNullOrEmpty(appList)) appList = "NONE";
                // --- 5. ĐÓNG GÓI VÀ GỬI ---
                string pushMessage = $"PUSH_RESOURCE {currentClientId} {cpu} {ramUsage} {disk} " +
                                     $"{netDown} {netUp} {machineName} {ipAddress} {appList}";

                await currentWriter.WriteLineAsync(pushMessage).ConfigureAwait(false);
            }
            catch
            {
                this.Invoke((Action)(() => {
                    btnStop_Click(null, null);
                    lblStatus.Text = "Trạng thái: Máy chủ đã ngắt kết nối.";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                }));
            }
            finally
            {
                _isSending = false;
            }
        }

        // ==========================================
        // 6. XÁC THỰC CHỨNG CHỈ CỦA SERVER
        // ==========================================
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null) return false;
            X509Certificate2 cert2 = new X509Certificate2(certificate);
            if (cert2.Issuer.Contains("UIT_ECC_RootCA") && cert2.Subject.Contains("RemoteMonitorServer"))
            {
                return true;
            }
            return false;
        }

        //=========================================
        // 7. HÀM LẤY ĐỊA CHỈ IP LOCAL
        //=========================================
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
    }
}