using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientA
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // 1. Kiểm tra rỗng
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Username và Password!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Băm mật khẩu bằng thuật toán SHA-256 trước khi gửi qua mạng
            string passwordHash = ComputeSha256Hash(password);

            // 3. TODO (Tuần 2-3): Sử dụng TcpClient gửi chuỗi này tới Server
            string loginMessage = $"LOGIN {username} {passwordHash}";

            // Console.WriteLine(loginMessage); // Dùng để debug xem chuỗi gửi đi có chuẩn không

            // Tạm thời mô phỏng Server trả về LOGIN_OK để làm tiếp UI
            bool isLoginSuccess = false;


            try
            {
                // Kết nối tới Server ở địa chỉ Localhost (127.0.0.1), Port 5000
                using (TcpClient client = new TcpClient("127.0.0.1", 5000))
                using (NetworkStream stream = client.GetStream())
                {
                    // 1. Gửi gói tin LOGIN đi
                    byte[] sendData = Encoding.UTF8.GetBytes(loginMessage);
                    stream.Write(sendData, 0, sendData.Length);

                    // 2. Chờ Server trả lời
                    byte[] receiveBuffer = new byte[1024];
                    int bytesRead = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
                    string response = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);

                    // 3. Xử lý kết quả
                    if (response == "LOGIN_OK")
                    {
                        isLoginSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối đến Server! Lỗi: " + ex.Message, "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (isLoginSuccess)
            {
                // Mở MainForm và chuyển quyền điều khiển
                MainForm mainForm = new MainForm();
                mainForm.Show();
                this.Hide(); // Ẩn form đăng nhập đi
            }
            else
            {
                MessageBox.Show("Tài khoản hoặc mật khẩu không chính xác!", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Hàm băm chuỗi đầu vào sử dụng thuật toán SHA-256
        /// </summary>
        private string ComputeSha256Hash(string rawData)
        {
            // Tạo đối tượng SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash trả về mảng byte
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Chuyển mảng byte thành chuỗi hex string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Xử lý sự kiện khi đóng LoginForm thì tắt luôn toàn bộ chương trình
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }
    }
}
