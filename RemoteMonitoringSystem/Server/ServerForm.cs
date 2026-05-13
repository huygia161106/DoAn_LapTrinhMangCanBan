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
using System.Collections.Concurrent;
using Newtonsoft.Json; // Thư viện xử lý JSON chuyên nghiệp

namespace Server
{
    public partial class ServerForm : Form
    {
        private TcpListener listener;
        private DatabaseHelper db;

        // Quản lý các luồng gửi của Client B để có thể gửi lệnh "Kill" ngược xuống
        private static ConcurrentDictionary<string, StreamWriter> connectedAgents = new ConcurrentDictionary<string, StreamWriter>();

        public ServerForm()
        {
            InitializeComponent();
            db = new DatabaseHelper();
        }

        private async void ServerForm_Load_1(object sender, EventArgs e)
        {
            LogToScreen("=== HỆ THỐNG GIÁM SÁT TRUNG TÂM (JSON MODE) ===");
            await Task.Run(() => StartServerAsync());
        }

        private async Task StartServerAsync()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 8888);
                listener.Start();
                LogToScreen("Server đang lắng nghe tại Port 8888...");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex) { LogToScreen("[LỖI HỆ THỐNG] " + ex.Message); }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            string clientId = "Unknown";
            try
            {
                using (SslStream sslStream = new SslStream(client.GetStream(), false, ValidateClientCertificate))
                {
                    X509Certificate2 cert = new X509Certificate2("ServerCertECC.pfx", "NT106.Q23");
                    await sslStream.AuthenticateAsServerAsync(cert, true, SslProtocols.Tls12, false);

                    using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                    using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                    {
                        string requestJson;
                        while ((requestJson = await reader.ReadLineAsync()) != null)
                        {
                            // Giải mã gói tin JSON
                            dynamic data = JsonConvert.DeserializeObject(requestJson);
                            string type = data.Type;

                            switch (type)
                            {
                                // ==========================================
                                // NHÓM 1: LỆNH XÁC THỰC (TÀI KHOẢN ADMIN/USER)
                                // ==========================================
                                case "GET_SALT":
                                    string salt = db.GetUserSalt((string)data.Username);
                                    await writer.WriteLineAsync(salt != null ? $"SALT {salt}" : "NOT_FOUND");
                                    break;

                                case "REGISTER":
                                    bool isCreated = db.CreateUser((string)data.Username, (string)data.Password, (string)data.Salt);
                                    await writer.WriteLineAsync(isCreated ? "REGISTER_OK" : "REGISTER_FAIL");
                                    LogToScreen($"[HỆ THỐNG] Đăng ký tài khoản: {(string)data.Username}");
                                    break;

                                case "LOGIN":
                                    string role = db.ValidateUser((string)data.Username, (string)data.Password);

                                    if (role != null)
                                    {
                                        // Gửi chữ LOGIN_OK kèm theo quyền của người đó (VD: "LOGIN_OK Admin" hoặc "LOGIN_OK User")
                                        await writer.WriteLineAsync($"LOGIN_OK {role}");
                                        LogToScreen($"[HỆ THỐNG] Đăng nhập: {(string)data.Username} (Quyền: {role})");
                                    }
                                    else
                                    {
                                        await writer.WriteLineAsync("LOGIN_FAIL");
                                    }
                                    break;

                                // ==========================================
                                // NHÓM 2: GIÁM SÁT & ĐIỀU KHIỂN (CLIENT B)
                                // ==========================================
                                case "REGISTER_AGENT":
                                    // Cấp phát ID tự động cho Client B
                                    string newId = db.RegisterOrGetAgent((string)data.MachineName, (string)data.IP);
                                    await writer.WriteLineAsync($"AGENT_ID {newId}");
                                    LogToScreen($"[HỆ THỐNG] Cấp ID [{newId}] cho máy {data.MachineName}");
                                    break;

                                case "PUSH_RESOURCE":
                                    // Nhận dữ liệu tài nguyên và lưu luồng để Remote Kill
                                    string currentAgentId = (string)data.ClientId;
                                    connectedAgents[currentAgentId] = writer;
                                    db.AddResourceHistory(currentAgentId, (string)data.Cpu, (string)data.Ram, (string)data.Disk, (string)data.NetDown, (string)data.NetUp, (string)data.AppList);
                                    LogToScreen($"[TÀI NGUYÊN] Máy {currentAgentId} ({(string)data.MachineName} - {(string)data.IP}) | CPU: {(string)data.Cpu}% | RAM: {(string)data.Ram}% | Đĩa C: {(string)data.Disk}% | Mạng: ↓{(string)data.NetDown} KB/s ↑{(string)data.NetUp} KB/s");
                                    break;

                                // ==========================================
                                // NHÓM 3: PHỤC VỤ DASHBOARD MAINFORM
                                // ==========================================
                                case "GET_ALL_CLIENTS":
                                    // Đổ danh sách ra Tab Admin
                                    string clientListJson = db.GetAllClientsList();
                                    await writer.WriteLineAsync($"CLIENT_LIST {clientListJson}");
                                    break;

                                case "GET_LATEST":
                                    // Đổ dữ liệu biểu đồ
                                    string latest = db.GetLatestResource((string)data.TargetClientId);
                                    await writer.WriteLineAsync(latest != null ? $"LATEST_DATA {latest}" : "NO_DATA");
                                    break;

                                case "REMOTE_KILL":
                                    // Chuyển tiếp lệnh tắt tiến trình
                                    string target = (string)data.TargetClientId;
                                    if (connectedAgents.TryGetValue(target, out var agentWriter))
                                    {
                                        var killCmd = new { Type = "KILL_COMMAND", ProcessName = (string)data.ProcessName };
                                        await agentWriter.WriteLineAsync(JsonConvert.SerializeObject(killCmd));
                                        LogToScreen($"[LỆNH ĐIỀU KHIỂN] Tắt {(string)data.ProcessName} trên máy {target}");
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { LogToScreen($"[NGẮT KẾT NỐI] Client {clientId}: {ex.Message}"); }
            finally { if (clientId != "Unknown") connectedAgents.TryRemove(clientId, out _); client.Close(); }
        }

        private bool ValidateClientCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            if (cert == null) return false;
            X509Certificate2 cert2 = new X509Certificate2(cert);
            return cert2.Issuer.Contains("UIT_ECC_RootCA") && cert2.Subject.Contains("RemoteMonitorClient");
        }

        private void LogToScreen(string msg)
        {
            if (rtbLogs.InvokeRequired) { rtbLogs.Invoke(new Action(() => LogToScreen(msg))); return; }
            rtbLogs.AppendText($"{DateTime.Now:HH:mm:ss} - {msg}{Environment.NewLine}");
            rtbLogs.ScrollToCaret();
        }
    }
}