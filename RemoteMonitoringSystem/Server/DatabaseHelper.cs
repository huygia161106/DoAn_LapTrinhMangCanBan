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

        /// <summary>
        /// Hàm tạo tài khoản mới (Register)
        /// </summary>
        public bool CreateUser(string username, string passwordHash)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Câu lệnh Insert, mặc định Role là 'User'
                    string query = "INSERT INTO Users (Username, PasswordHash) VALUES (@user, @pass)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", passwordHash);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0; // Trả về true nếu Insert thành công
                    }
                }
                catch (SQLiteException ex)
                {
                    // Lỗi 19 trong SQLite thường là do vi phạm UNIQUE constraint (Trùng Username)
                    if (ex.ErrorCode == 19)
                    {
                        Console.WriteLine($"[CẢNH BÁO DB] Tên đăng nhập '{username}' đã tồn tại.");
                    }
                    else
                    {
                        Console.WriteLine("[LỖI DB] " + ex.Message);
                    }
                    return false; // Đăng ký thất bại
                }
            }
        }
    }
}