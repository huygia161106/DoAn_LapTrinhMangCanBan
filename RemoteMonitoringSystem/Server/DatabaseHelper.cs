using System;
using System.Data.SQLite;
using System.IO;

namespace Server
{
    public class DatabaseHelper
    {
        // Đường dẫn tới file database (đặt cùng thư mục file .exe của Server)
        private string connectionString = "Data Source=RemoteMonitor.db;Version=3;";

        public DatabaseHelper()
        {
            // Kiểm tra xem file DB có tồn tại không
            if (!File.Exists("RemoteMonitor.db"))
            {
                Console.WriteLine("[CẢNH BÁO] Không tìm thấy file RemoteMonitor.db!");
            }
        }

        /// <summary>
        /// Hàm kiểm tra đăng nhập (Validate Login)
        /// </summary>
        public bool ValidateUser(string username, string passwordHash)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(1) FROM Users WHERE Username = @user AND PasswordHash = @pass";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", passwordHash);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0; // Trả về true nếu có 1 dòng khớp
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[LỖI DB] " + ex.Message);
                    return false;
                }
            }
        }
    }
}