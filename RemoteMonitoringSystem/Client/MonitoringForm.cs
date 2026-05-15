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
using Newtonsoft.Json;

namespace Client
{
    public partial class MonitoringForm : Form
    {
        // ==========================================
        // CÁC BIẾN TOÀN CỤC
        // ==========================================
        private PerformanceCounter cpuCounter;
        private long prevBytesReceived = 0;
        private long prevBytesSent = 0;

        // 
        private string currentShareCode = "";
        private string machineName = Environment.MachineName;
        private string ipAddress = "127.0.0.1";
        private string currentUsername;

        // Giao tiếp mạng mTLS
        private TcpClient currentClient;
        private SslStream currentSslStream;
        private StreamWriter currentWriter;
        private StreamReader currentReader;

        // Danh sách các tiến trình lõi cấm bị Kill
        private readonly string[] protectedProcesses = {
            "svchost", "explorer", "csrss", "wininit", "smss", "services", "lsass", "system"
        };

        public MonitoringForm(string username)
        {
            InitializeComponent();
            InitializeCounters();
            ipAddress = GetLocalIPAddress();
            currentUsername = username;
        }

        // ==========================================
        // 1. KHỞI TẠO BỘ ĐẾM HIỆU NĂNG
        // ==========================================
        private void InitializeCounters()
        {
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); // Lấy đà cho CPU
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo bộ đếm: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // 2. MỞ ĐƯỜNG HẦM mTLS & KẾT NỐI SERVER
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
                lblStatus.Text =$"Trạng thái: Đang chia sẻ máy tính"; // Hiển thị ID để dễ kiểm tra
                lblStatus.ForeColor = System.Drawing.Color.Green;
                timerPush.Start(); // Bắt đầu gửi dữ liệu mỗi 2 giây
            }
            else
            {
                btnStart.Enabled = true;
            }
        }

        private async Task<bool> ConnectToServerAsync()
        {
            try
            {
                currentClient = new TcpClient();
                currentClient.NoDelay = true;

                var connectTask = currentClient.ConnectAsync("127.0.0.1", 8888);
                if (await Task.WhenAny(connectTask, Task.Delay(2000)).ConfigureAwait(false) != connectTask)
                    throw new TimeoutException("Máy chủ không phản hồi.");

                await connectTask.ConfigureAwait(false);

                // Bắt tay mTLS với chứng chỉ ECC
                NetworkStream netStream = currentClient.GetStream();
                currentSslStream = new SslStream(netStream, false, ValidateServerCertificate);

                var clientCertificate = new X509Certificate2("ClientCertECC.pfx", "NT106.Q23");
                var clientCerts = new X509CertificateCollection(new X509Certificate[] { clientCertificate });

                await currentSslStream.AuthenticateAsClientAsync("RemoteMonitorServer", clientCerts, SslProtocols.Tls12, false).ConfigureAwait(false);

                // Khởi tạo kênh Đọc - Ghi
                currentWriter = new StreamWriter(currentSslStream, Encoding.UTF8) { AutoFlush = true };
                currentReader = new StreamReader(currentSslStream, Encoding.UTF8);

                // =========================================================
                // TỰ ĐỘNG XIN CẤP PHÁT ID TỪ SERVER
                // =========================================================

                // 1. Gửi thông tin của mình lên Server
                var regData = new { Type = "REGISTER_AGENT", MachineName = machineName, IP = ipAddress, Username = currentUsername };
                await currentWriter.WriteLineAsync(JsonConvert.SerializeObject(regData));

                // 2. Chờ Server duyệt và gửi mã số ID về
                string response = await currentReader.ReadLineAsync();

                if (response != null && response.StartsWith("REGISTER_OK|"))
                {
                    currentShareCode = response.Substring("REGISTER_OK|".Length).Trim();

                    this.Invoke((Action)(() =>
                    {
                        lblShareCode.Text = currentShareCode;

                        lblStatus.Text =
                            $"Đang chia sẻ máy tính | CODE: {currentShareCode}";

                        lblStatus.ForeColor = System.Drawing.Color.Green;
                    }));
                }
                else
                {
                    throw new Exception("Server không cấp Share Code.");
                }
                // =========================================================

                // Kích hoạt luồng chạy ngầm để lắng nghe lệnh Remote Kill từ Server
                _ = Task.Run(() => ListenForCommands());

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
        // 3. LUỒNG LẮNG NGHE LỆNH TỪ XA (REMOTE KILL)
        // ==========================================
        private async Task ListenForCommands()
        {
            try
            {
                string cmdJson;
                while ((cmdJson = await currentReader.ReadLineAsync()) != null)
                {
                    dynamic cmd = JsonConvert.DeserializeObject(cmdJson);

                    if (cmd.Type == "KILL_PROCESS")

                    {
                        // Lấy tên process và loại bỏ đuôi .exe
                        string pName = ((string)cmd.ProcessName).Replace(".exe", "").ToLower();

                        // KIỂM TRA BẢO MẬT: Chặn lệnh nếu nhắm vào tiến trình hệ thống
                        bool isProtected = false;
                        foreach (string proc in protectedProcesses)
                        {
                            if (pName == proc) { isProtected = true; break; }
                        }
                        if (isProtected) continue; // Bỏ qua lệnh Kill này để bảo vệ máy

                        // Thực thi tiêu diệt tiến trình
                        Process[] processes = Process.GetProcessesByName(pName);
                        foreach (var process in processes)
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch { /* Bỏ qua các lỗi Access Denied để không crash Client B */ }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() => {
                    btnStop_Click(null, null);
                    lblStatus.Text = "Luồng lệnh bị ngắt: " + ex.Message;
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                }));
            }
        }

        // ==========================================
        // 4. THU THẬP & ĐẨY DỮ LIỆU TÀI NGUYÊN (JSON)
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

                // --- Thu thập CPU ---
                float cpuVal = await Task.Run(() => cpuCounter.NextValue()).ConfigureAwait(false);
                string cpu = Math.Round(cpuVal, 1).ToString();

                // --- Thu thập RAM ---
                Microsoft.VisualBasic.Devices.ComputerInfo ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
                double totalRamMB = ci.TotalPhysicalMemory / (1024.0 * 1024.0);
                double availableRamMB = ci.AvailablePhysicalMemory / (1024.0 * 1024.0);
                double usedRamMB = totalRamMB - availableRamMB;
                string ramUsage = Math.Round((usedRamMB / totalRamMB) * 100, 1).ToString();

                // --- Thu thập Ổ đĩa ---
                DriveInfo drive = new DriveInfo("C");
                double diskFreePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;
                string disk = Math.Round(100 - diskFreePercent, 1).ToString();

                // --- Thu thập Mạng ---
                long currentReceived = 0, currentSent = 0;
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        currentReceived += ni.GetIPv4Statistics().BytesReceived;
                        currentSent += ni.GetIPv4Statistics().BytesSent;
                    }
                }
                double downloadSpeedKBps = (prevBytesReceived != 0) ? (currentReceived - prevBytesReceived) / 2.0 / 1024.0 : 0;
                double uploadSpeedKBps = (prevBytesSent != 0) ? (currentSent - prevBytesSent) / 2.0 / 1024.0 : 0;

                prevBytesReceived = currentReceived;
                prevBytesSent = currentSent;

                // --- Thu thập Danh sách ứng dụng ---
                StringBuilder appBuilder = new StringBuilder();
                foreach (Process p in Process.GetProcesses())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(p.MainWindowTitle))
                        {
                            long memoryMB = p.WorkingSet64 / (1024 * 1024);
                            appBuilder.Append($"{p.ProcessName}.exe|{p.MainWindowTitle}|{memoryMB} MB;");
                        }
                    }
                    catch { }
                }
                string appList = appBuilder.ToString().TrimEnd(';');
                if (string.IsNullOrEmpty(appList)) appList = "NONE";

                // --- ĐÓNG GÓI JSON & GỬI ---
                var resourceData = new
                {
                    Type = "PUSH_RESOURCE",
                    ShareCode = currentShareCode, // Lúc này đã tự động lấy tên máy (kiểu string)
                    Cpu = cpu,
                    Ram = ramUsage,
                    Disk = disk,
                    NetDown = Math.Round(downloadSpeedKBps, 1).ToString(),
                    NetUp = Math.Round(uploadSpeedKBps, 1).ToString(),
                    MachineName = machineName,
                    IP = ipAddress,
                    AppList = appList
                };

                await currentWriter.WriteLineAsync(JsonConvert.SerializeObject(resourceData));
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
        // 5. CÁC HÀM TIỆN ÍCH KHÁC
        // ==========================================
        private void btnStop_Click(object sender, EventArgs e)
        {
            timerPush.Stop();
            currentWriter?.Close();
            currentReader?.Close();
            currentSslStream?.Close();
            currentClient?.Close();

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            lblStatus.Text = "Trạng thái: Đã dừng.";
            lblStatus.ForeColor = System.Drawing.Color.Red;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null) return false;
            X509Certificate2 cert2 = new X509Certificate2(certificate);
            return cert2.Issuer.Contains("UIT_ECC_RootCA") && cert2.Subject.Contains("RemoteMonitorServer");
        }

        private string GetLocalIPAddress()
        {
            string localIP = "127.0.0.1";
            try
            {
                // Duyệt qua tất cả các card mạng trên máy tính
                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // Chỉ lấy các card mạng đang hoạt động (Up) và không phải là mạng nội bộ ảo (Loopback)
                    if (item.OperationalStatus == OperationalStatus.Up &&
                        item.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var props = item.GetIPProperties();

                        // ĐIỂM MẤU CHỐT: Chỉ lấy card mạng có Default Gateway (có lối ra Router/Internet)
                        if (props.GatewayAddresses.Count > 0)
                        {
                            foreach (UnicastIPAddressInformation ip in props.UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == AddressFamily.InterNetwork) // Lấy IPv4
                                {
                                    return ip.Address.ToString(); // Trả về IP thật của Wi-Fi hoặc dây LAN
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return localIP;
        }
    }
}