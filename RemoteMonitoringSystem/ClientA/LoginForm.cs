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

            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 8888))
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
                        // XIN SALT TỪ SERVER (Dùng WriteLineAsync để tự động thêm \n)
                        await writer.WriteLineAsync($"GET_SALT {username}");
                        
                        // Chờ Server trả về Salt
                        string saltResponse = await reader.ReadLineAsync();

                        if (saltResponse == null || !saltResponse.StartsWith("SALT "))
                        {
                            MessageBox.Show("Tài khoản không tồn tại!", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        string salt = saltResponse.Substring(5);

                        // BĂM MẬT KHẨU CÙNG VỚI SALT VÀ ĐĂNG NHẬP
                        string passwordHash = ComputeSha256Hash(password + salt);
                        
                        // Gửi lệnh LOGIN
                        await writer.WriteLineAsync($"LOGIN {username} {passwordHash}");

                        // Chờ Server trả lời
                        string loginResponse = await reader.ReadLineAsync();

                        if (loginResponse == "LOGIN_OK")
                        {
                            isLoginSuccess = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối đến Server! Lỗi: " + ex.Message, "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Xử lý chuyển giao diện
            if (isLoginSuccess)
            {
                MainForm mainForm = new MainForm();
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