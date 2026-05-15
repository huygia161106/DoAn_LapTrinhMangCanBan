using Newtonsoft.Json; // Thư viện xử lý JSON chuyên nghiệp
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
namespace Server
{
    public partial class ServerForm : Form
    {
        private TcpListener listener;
        private DatabaseHelper db;

        // Quản lý các luồng gửi của Client B để có thể gửi lệnh "Kill" ngược xuống
        private static ConcurrentDictionary<string, AgentSession> connectedAgents= new ConcurrentDictionary<string, AgentSession>();

        private static ConcurrentDictionary<StreamWriter, string> connectedRoles = new ConcurrentDictionary<StreamWriter, string>();

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
                                        connectedRoles[writer] = role;
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
                                    {
                                        string shareCode = GenerateShareCode();

                                        AgentSession session = new AgentSession
                                        {
                                            ShareCode = shareCode,
                                            MachineName = (string)data.MachineName,
                                            IP = (string)data.IP,
                                            Writer = writer,
                                            LatestData = ""
                                        };

                                        connectedAgents[shareCode] = session;

                                        await writer.WriteLineAsync($"SHARE_CODE {shareCode}");

                                        LogToScreen($"[AGENT ONLINE] {(string)data.MachineName} | CODE: {shareCode}");

                                        break;
                                    }
                                case "PUSH_RESOURCE":
                                    {
                                        string shareCode = (string)data.ShareCode;

                                        if (connectedAgents.TryGetValue(shareCode, out var session))
                                        {
                                            session.LatestData = requestJson;

                                            LogToScreen(
                                                $"[RESOURCE] {session.MachineName} | CPU: {(string)data.Cpu}% | RAM: {(string)data.Ram}%"
                                            );
                                        }

                                        break;
                                    }
                                case "GET_LATEST_BY_CODE":
                                    {
                                        string shareCode = (string)data.ShareCode;

                                        if (connectedAgents.TryGetValue(shareCode, out var session))
                                        {
                                            await writer.WriteLineAsync(
                                                $"LATEST_DATA {session.LatestData}"
                                            );
                                        }
                                        else
                                        {
                                            await writer.WriteLineAsync("NO_DATA");
                                        }

                                        break;
                                    }
                                // ==========================================
                                // NHÓM 3: PHỤC VỤ DASHBOARD MAINFORM
                                // ==========================================
                                case "GET_ALL_CLIENTS":
                                    {
                                        if (!IsAdmin(writer))
                                        {
                                            await writer.WriteLineAsync("ACCESS_DENIED");
                                            break;
                                        }

                                        string clients = string.Join(";",
                                            connectedAgents.Select(c =>
                                                $"{c.Key}|{c.Value.MachineName}|{c.Value.IP}"
                                            ));

                                        await writer.WriteLineAsync(clients);

                                        break;
                                    }
                                case "GET_LATEST":
                                    // Đổ dữ liệu biểu đồ
                                    string latest = db.GetLatestResource((string)data.TargetClientId);
                                    await writer.WriteLineAsync(latest != null ? $"LATEST_DATA {latest}" : "NO_DATA");
                                    break;

                                case "REMOTE_KILL":
                                    {
                                        if (!IsAdmin(writer))
                                        {
                                            await writer.WriteLineAsync("ACCESS_DENIED");

                                            LogToScreen("[SECURITY] User cố dùng REMOTE_KILL");

                                            break;
                                        }

                                        string target = (string)data.TargetClientId;
                                        string processName = (string)data.ProcessName;

                                        if (connectedAgents.TryGetValue(target, out var session))
                                        {
                                            await session.Writer.WriteLineAsync(
                                                JsonConvert.SerializeObject(new
                                                {
                                                    Type = "KILL_PROCESS",
                                                    ProcessName = processName
                                                })
                                            );

                                            await writer.WriteLineAsync("KILL_SENT");
                                        }
                                        else
                                        {
                                            await writer.WriteLineAsync("CLIENT_NOT_FOUND");
                                        }

                                        break;
                                    }

                            }
                        }
                    }
                }
            }
            catch (Exception ex) { LogToScreen($"[NGẮT KẾT NỐI] Client {clientId}: {ex.Message}"); }
            finally { if (clientId != "Unknown") connectedAgents.TryRemove(clientId, out _); client.Close();}
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
        private string GenerateShareCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            Random rnd = new Random();

            return "RM-" +
                   new string(Enumerable.Repeat(chars, 8)
                   .Select(s => s[rnd.Next(s.Length)])
                   .ToArray());
        }
        private bool IsAdmin(StreamWriter writer)
        {
            if (connectedRoles.TryGetValue(writer, out string role))
            {
                return role == "Admin";
            }

            return false;
        }
    }
}