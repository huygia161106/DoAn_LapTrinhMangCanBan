using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            await StartServerAsync();
        }

        private async Task StartServerAsync()
        {
            try
            {
                // Khởi tạo Socket lắng nghe ở Port 5000
                listener = new TcpListener(IPAddress.Any, 5000);
                listener.Start();
                LogToScreen("Server đang lắng nghe tại Port 5000...");

                // Vòng lặp bất đồng bộ để liên tục chờ Client
                while (true)
                {
                    // Dùng AcceptTcpClientAsync để không chặn luồng UI
                    TcpClient client = await listener.AcceptTcpClientAsync();
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
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    // Đọc gói tin từ Client A
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        LogToScreen($"[NHẬN TỪ CLIENT] {receivedMessage}");

                        // Cắt chuỗi để phân tích lệnh (Cú pháp: LOGIN username passwordHash)
                        string[] parts = receivedMessage.Split(' ');

                        if (parts[0] == "LOGIN" && parts.Length >= 3)
                        {
                            string username = parts[1];
                            string passHash = parts[2];

                            // Gọi class DatabaseHelper để kiểm tra trong SQLite
                            bool isValid = db.ValidateUser(username, passHash);

                            // Trả lời lại Client
                            string response = isValid ? "LOGIN_OK" : "LOGIN_FAIL";
                            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

                            LogToScreen($"[PHẢN HỒI CLIENT] {response}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogToScreen("[LỖI CLIENT] " + ex.Message);
            }
            finally
            {
                client.Close();
            }
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
            rtbLogs.ScrollToCaret(); // Tự động cuộn xuống dòng mới nhất
        }

        // Tắt Server an toàn khi đóng Form
        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener?.Stop();
        }


    }
}