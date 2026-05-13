using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Username và Password!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isLoginSuccess = false;
            string userRole = "User"; // Mặc định là User


            try
            {
                using (TcpClient client = new TcpClient("192.168.31.198", 8888))
                using (NetworkStream netStream = client.GetStream())
                using (SslStream sslStream = new SslStream(netStream, false, ValidateServerCertificate))
                {
                    // 1. Tải chứng chỉ mTLS
                    X509Certificate2 clientCertificate = new X509Certificate2("ClientCertECC.pfx", "NT106.Q23");
                    X509CertificateCollection clientCerts = new X509CertificateCollection(new X509Certificate[] { clientCertificate });

                    // Bắt tay bảo mật
                    await sslStream.AuthenticateAsClientAsync("RemoteMonitorServer", clientCerts, SslProtocols.Tls12, false);

                    // 2. Dùng StreamWriter/StreamReader để giao tiếp mượt mà
                    using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                    using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                    {
                        // 3. XIN SALT TỪ SERVER (Sử dụng định dạng JSON)
                        var getSaltRequest = new
                        {
                            Type = "GET_SALT",
                            Username = username
                        };

                        await writer.WriteLineAsync(JsonConvert.SerializeObject(getSaltRequest));

                        // Chờ Server trả về Salt (Server hiện đang trả về: "SALT <chuỗi_salt>" hoặc "NOT_FOUND")
                        string saltResponse = await reader.ReadLineAsync();

                        if (string.IsNullOrEmpty(saltResponse) || saltResponse == "NOT_FOUND")
                        {
                            MessageBox.Show("Tài khoản không tồn tại!", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // --- Cắt bỏ 5 ký tự đầu tiên ("SALT ") để lấy đúng chuỗi Salt gốc ---
                        string actualSalt = saltResponse.Substring(5).Trim();

                        // 4. BĂM MẬT KHẨU CÙNG VỚI SALT GỐC VÀ ĐĂNG NHẬP
                        string passwordHash = ComputeSha256Hash(password + actualSalt);

                        // Đóng gói lệnh LOGIN thành JSON
                        var loginRequest = new
                        {
                            Type = "LOGIN",
                            Username = username,
                            Password = passwordHash
                        };

                        await writer.WriteLineAsync(JsonConvert.SerializeObject(loginRequest));

                        // Chờ Server trả lời ("LOGIN_OK" hoặc "LOGIN_FAIL")
                        string loginResponse = await reader.ReadLineAsync();


                        if (loginResponse != null && loginResponse.StartsWith("LOGIN_OK"))
                        {
                            isLoginSuccess = true;

                            // Bóc tách để lấy quyền (Admin hoặc User) nằm sau dấu khoảng trắng
                            string[] parts = loginResponse.Split(' ');
                            if (parts.Length > 1)
                            {
                                userRole = parts[1];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối đến Server! Lỗi: " + ex.Message, "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 5. Xử lý chuyển giao diện
            if (isLoginSuccess)
            {
                MainForm mainForm = new MainForm(userRole);
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Mật khẩu không chính xác!", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Hàm Callback xác thực Server (Bắt buộc phải có khi dùng mTLS)
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            RegisterForm regForm = new RegisterForm();
            regForm.ShowDialog();
        }
    }
}