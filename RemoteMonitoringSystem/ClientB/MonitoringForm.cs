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
        // Khai báo bộ đếm hiệu năng
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;

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
                // Khởi tạo đo RAM trống (Megabytes)
                ramCounter = new PerformanceCounter("Memory", "Available MBytes");

                // Gọi NextValue() lần đầu để hệ thống lấy đà (lần đầu luôn trả về 0)
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

            // Chờ kết nối và xác thực mTLS
            bool isConnected = await ConnectToServerAsync();

            if (isConnected)
            {
                btnStop.Enabled = true;
                lblStatus.Text = "Trạng thái: Đang gửi dữ liệu (OK)";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                // Mở đường hầm xong mới cho Timer bắt đầu đếm nhịp 2 giây/lần
                timerPush.Start();
            }
            else
            {
                // Nếu kết nối thất bại, bật lại nút Start cho người dùng thử lại
                btnStart.Enabled = true;
            }
        }

        // ==========================================
        // 3. HÀM KẾT NỐI VÀ XÁC THỰC mTLS (CHẠY 1 LẦN)
        // ==========================================
        private async Task<bool> ConnectToServerAsync()
        {
            try
            {
                currentClient = new TcpClient();
                currentClient.NoDelay = true;

                var connectTask = currentClient.ConnectAsync("127.0.0.1", 5000);
                if (await Task.WhenAny(connectTask, Task.Delay(2000)).ConfigureAwait(false) != connectTask)
                    throw new TimeoutException("Máy chủ không phản hồi.");

                await connectTask.ConfigureAwait(false);

                NetworkStream netStream = currentClient.GetStream();
                currentSslStream = new SslStream(netStream, false, ValidateServerCertificate);

                // Tải chứng chỉ ECC của Client
                var clientCertificate = new X509Certificate2("ClientCertECC.pfx", "NT106.Q23");
                var clientCerts = new X509CertificateCollection(new X509Certificate[] { clientCertificate });

                // Bắt tay TLS với chuẩn an toàn
                await currentSslStream.AuthenticateAsClientAsync("RemoteMonitorServer", clientCerts, SslProtocols.Tls12, false).ConfigureAwait(false);
                
                // Khởi tạo StreamWriter với tính năng AutoFlush(Tự động xả ngay lập tức)
                currentWriter = new StreamWriter(currentSslStream, Encoding.UTF8) { AutoFlush = true };

                return true; // Kết nối thành công
            }
            catch (Exception ex)
            {
                // Phải dùng Invoke vì đang ở thread khác sau ConfigureAwait(false)
                this.Invoke((Action)(() => {
                    lblStatus.Text = "Lỗi kết nối: " + ex.Message;
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                }));
                return false;
            }
        }

        // ==========================================
        // 4. XỬ LÝ NÚT STOP (ĐÓNG ĐƯỜNG HẦM)
        // ==========================================
        private void btnStop_Click(object sender, EventArgs e)
        {
            // 1. Dừng bộ đếm thời gian
            timerPush.Stop();

            // 2. Đóng Writer và đường hầm mTLS
            currentWriter?.Close();
            currentSslStream?.Close();
            currentClient?.Close();

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            lblStatus.Text = "Trạng thái: Đã dừng.";
            lblStatus.ForeColor = System.Drawing.Color.Red;
        }

        // ==========================================
        // 5. TIMER CHẠY NGẦM GỬI DỮ LIỆU (MỖI 2 GIÂY)
        // ==========================================
        private bool _isSending = false; // Flag chống chồng tick
        private async void timerPush_Tick(object sender, EventArgs e)
        {
            if (_isSending) return; // Bỏ qua nếu tick trước chưa xong
            _isSending = true;

            try
            {
                if (currentSslStream == null || currentClient == null || !currentClient.Connected)
                {
                    btnStop_Click(null, null);
                    return;
                }

                // Lấy CPU trên thread pool để không block timer
                float cpuVal = await Task.Run(() => cpuCounter.NextValue()).ConfigureAwait(false);

                string cpu = Math.Round(cpuVal, 1).ToString();
                string ramFree = ramCounter.NextValue().ToString();

                DriveInfo drive = new DriveInfo("C");
                double diskFreePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;
                string disk = Math.Round(100 - diskFreePercent, 1).ToString();

                // Tính toán lưu lượng mạng (Kbps) dựa trên sự chênh lệch bytes đã nhận và gửi
                long currentReceived = 0;
                long currentSent = 0;

                // Quét toàn bộ các card mạng đang hoạt động
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

                // Tính toán nếu không phải là lần chạy đầu tiên
                if (prevBytesReceived != 0 && prevBytesSent != 0)
                {
                    // Lấy chênh lệch chia cho 2 (vì Timer chạy 2s/lần), rồi chia cho 1024 để ra KiloBytes
                    downloadSpeedKBps = (currentReceived - prevBytesReceived) / 2.0 / 1024.0;
                    uploadSpeedKBps = (currentSent - prevBytesSent) / 2.0 / 1024.0;
                }

                prevBytesReceived = currentReceived;
                prevBytesSent = currentSent;

                string netDown = Math.Round(downloadSpeedKBps, 1).ToString();
                string netUp = Math.Round(uploadSpeedKBps, 1).ToString();

                string appList = "chrome.exe|Google Chrome";

                string pushMessage = $"PUSH_RESOURCE {currentClientId} {cpu} {ramFree} {disk} " +
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
                _isSending = false; // Luôn reset flag dù thành công hay thất bại
            }
        }

        // ==========================================
        // 6. XÁC THỰC CHỨNG CHỈ CỦA SERVER
        // ==========================================
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null) return false;

            X509Certificate2 cert2 = new X509Certificate2(certificate);

            // Kiểm tra bảo mật kép: Đúng CA gốc và đúng tên Server
            if (cert2.Issuer.Contains("UIT_ECC_RootCA") && cert2.Subject.Contains("RemoteMonitorServer"))
            {
                return true;
            }
            return false;
        }

        //=========================================
        // 7. HÀM LẤY ĐỊA CHỈ IP CỦA MÁY LOCAL
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