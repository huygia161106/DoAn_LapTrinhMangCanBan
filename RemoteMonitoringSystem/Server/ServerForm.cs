using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Server
{
    public partial class ServerForm : Form
    {
        private TcpListener listener;
        private DatabaseHelper db;

        public ServerForm()
        {
            InitializeComponent();
            db = new DatabaseHelper();
        }

        // Sự kiện khi Form vừa load lên sẽ tự động chạy Server
        private async void ServerForm_Load_1(object sender, EventArgs e)
        {
            LogToScreen("=== SERVER ĐIỀU KHIỂN & GIÁM SÁT ===");
            await Task.Run(() => StartServerAsync());
        }

        private async Task StartServerAsync()
        {
            try
            {
                // Khởi tạo Socket lắng nghe ở Port 8888
                listener = new TcpListener(IPAddress.Any, 8888);
                listener.Start();
                LogToScreen("Server đang lắng nghe tại Port 8888...");

                // Vòng lặp bất đồng bộ để liên tục chờ Client
                while (true)
                {
                    // Dùng AcceptTcpClientAsync để không chặn luồng UI
                    TcpClient client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    LogToScreen($"[KẾT NỐI MỚI] Client từ: {client.Client.RemoteEndPoint}");

                    // Xử lý đọc dữ liệu từ Client (cũng dùng bất đồng bộ)
                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                LogToScreen("[LỖI MẠNG] " + ex.Message);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                // Tắt Nagle
                client.NoDelay = true;
                using (NetworkStream netStream = client.GetStream())
                using (SslStream sslStream = new SslStream(netStream, false, ValidateClientCertificate))
                {
                    X509Certificate2 serverCertificate = new X509Certificate2("ServerCertECC.pfx", "NT106.Q23");
                    await sslStream.AuthenticateAsServerAsync(serverCertificate, clientCertificateRequired: true, enabledSslProtocols: SslProtocols.Tls12, checkCertificateRevocation: false);

                    LogToScreen($"[TLS-ECC] Đã kết nối an toàn với {client.Client.RemoteEndPoint}");

                    // SỬ DỤNG STREAM READER ĐỂ ĐỌC DỮ LIỆU LIÊN TỤC KHÔNG BỊ RỚT GÓI
                    using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                    using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                    {
                        string receivedMessage;

                        while ((receivedMessage = await reader.ReadLineAsync()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(receivedMessage)) 
                                continue;

                            string[] parts = receivedMessage.Split(' ');

                            if (parts[0] == "GET_SALT" && parts.Length >= 2)
                            {
                                string salt = db.GetUserSalt(parts[1]);
                                await writer.WriteLineAsync(salt != null ? $"SALT {salt}" : "SALT_NOT_FOUND");
                            }
                            else if (parts[0] == "REGISTER" && parts.Length >= 4)
                            {
                                bool isCreated = db.CreateUser(parts[1], parts[2], parts[3]);
                                await writer.WriteLineAsync(isCreated ? "REGISTER_OK" : "REGISTER_FAIL");
                                LogToScreen($"[ĐĂNG KÝ] {parts[1]}");
                            }
                            else if (parts[0] == "LOGIN" && parts.Length >= 3)
                            {
                                bool isValid = db.ValidateUser(parts[1], parts[2]);
                                await writer.WriteLineAsync(isValid ? "LOGIN_OK" : "LOGIN_FAIL");
                                LogToScreen($"[ĐĂNG NHẬP] {parts[1]}");
                            }
                            else if (parts[0] == "PUSH_RESOURCE" && parts.Length >= 9)
                            {
                                string clientId = parts[1];
                                string cpu = parts[2];
                                string ram = parts[3];
                                string disk = parts[4];
                                string netUp = parts[5];
                                string netDown = parts[6];
                                string machineName = parts[7];
                                string ipAddress = parts[8];

                                string appList = parts.Length > 9 ? string.Join(" ", parts, 9, parts.Length - 9) : "";

                                LogToScreen($"[TÀI NGUYÊN] ID:{clientId} ({machineName} - IP: {ipAddress}) | CPU: {cpu}% | RAM: {ram}% | Ổ C: {disk}% | Net: ↓{netDown} KB/s ↑{netUp} KB/s"); db.AddResourceHistory(clientId, cpu, ram, parts[4], parts[5], parts[6], appList);
                                db.AddResourceHistory(clientId, cpu, ram, disk, netDown, netUp, appList);

                            }

                            else if (parts[0] == "GET_LATEST" && parts.Length >= 2)
                            {
                                string targetClientId = parts[1];

                                // Bạn cần viết thêm hàm GetLatestResource trong DatabaseHelper
                                // Hàm này sẽ dùng lệnh SQL: "SELECT CpuPercent, RamPercent FROM ResourceHistory WHERE ClientId = @cId ORDER BY Timestamp DESC LIMIT 1"
                                string latestData = db.GetLatestResource(targetClientId);

                                if (latestData != null)
                                {
                                    // latestData có dạng "45.5 60.2"
                                    await writer.WriteLineAsync($"LATEST_DATA {latestData}");
                                }
                                else
                                {
                                    await writer.WriteLineAsync("NO_DATA");
                                }
                            }
                        }
                    }
                }
            }

            catch (IOException)
            {
                // Bỏ qua lỗi ngắt kết nối đột ngột từ Client (ví dụ Client tắt ngang app)
                LogToScreen("[NGẮT KẾT NỐI] Một Client đã rời đi.");
            }
            catch (Exception ex)
            {
                LogToScreen("[LỖI mTLS CLIENT] " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        private bool ValidateClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null) return false;

            X509Certificate2 cert2 = new X509Certificate2(certificate);

            // Kiểm tra xem chứng chỉ có phải do Root CA ECC cấp không
            if (cert2.Issuer.Contains("UIT_ECC_RootCA") && cert2.Subject.Contains("RemoteMonitorClient"))
            {
                return true;
            }
            return false;
        }

        // Hàm hỗ trợ in chữ lên màn hình một cách an toàn
        private void LogToScreen(string message)
        {
            if (rtbLogs.InvokeRequired)
            {
                rtbLogs.Invoke(new Action(() => LogToScreen(message)));
                return;
            }
            rtbLogs.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            rtbLogs.ScrollToCaret(); 
        }

        // Tắt Server an toàn khi đóng Form
        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener?.Stop();
        }


    }
}