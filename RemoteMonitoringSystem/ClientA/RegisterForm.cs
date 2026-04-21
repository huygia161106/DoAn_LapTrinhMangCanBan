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
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        private string GenerateSalt(int length = 16)
        {
            byte[] saltBytes = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // Nhớ thêm chữ async vào đây
        private async void btnSubmitRegister_Click(object sender, EventArgs e)
        {
            string username = txtRegUsername.Text.Trim();
            string password = txtRegPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string salt = GenerateSalt();
            string passwordHash = ComputeSha256Hash(password + salt);

            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 8888))
                using (NetworkStream netStream = client.GetStream())
                using (SslStream sslStream = new SslStream(netStream, false, ValidateServerCertificate))
                {
                    X509Certificate2 clientCertificate = new X509Certificate2("ClientCertECC.pfx", "NT106.Q23");
                    X509CertificateCollection clientCerts = new X509CertificateCollection(new X509Certificate[] { clientCertificate });

                    await sslStream.AuthenticateAsClientAsync("RemoteMonitorServer", clientCerts, SslProtocols.Tls12, false);

                    using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                    using (StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true })
                    {
                        // Gửi lệnh REGISTER (writer.WriteLineAsync tự nối \n)
                        await writer.WriteLineAsync($"REGISTER {username} {passwordHash} {salt}");

                        // Nhận phản hồi
                        string response = await reader.ReadLineAsync();

                        if (response == "REGISTER_OK")
                        {
                            MessageBox.Show("Đăng ký tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Tên đăng nhập đã tồn tại hoặc có lỗi xảy ra!", "Đăng ký thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối đến Server: " + ex.Message, "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}