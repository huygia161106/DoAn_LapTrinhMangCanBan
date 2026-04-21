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
            if (!File.Exists("RemoteMonitor.db"))
            {
                Console.WriteLine("[CẢNH BÁO] Không tìm thấy file RemoteMonitor.db!");
            }
        }

        // Hàm kiểm tra đăng nhập (Validate Login)
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

        // Lấy Salt của một User bất kỳ
        public string GetUserSalt(string username)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Salt FROM Users WHERE Username = @user";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : null;
                }
            }
        }

        // Hàm tạo tài khoản mới (Register)
        public bool CreateUser(string username, string passwordHash, string salt)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Users (Username, PasswordHash, Salt) VALUES (@user, @pass, @salt)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", passwordHash);
                        cmd.Parameters.AddWithValue("@salt", salt);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch { return false; }
            }
        }

        // Hàm lưu lịch sử tài nguyên
        public void AddResourceHistory(string clientId, string cpu, string ram, string disk, string netDown, string netUp, string appList)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"INSERT INTO ResourceHistory (ClientId, CpuPercent, RamPercent, DiskPercent, NetworkDown, NetworkUp, AppList) 
                                     VALUES (@cId, @cpu, @ram, @disk, @netD, @netU, @apps)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cId", clientId);
                        cmd.Parameters.AddWithValue("@cpu", cpu);
                        cmd.Parameters.AddWithValue("@ram", ram);
                        cmd.Parameters.AddWithValue("@disk", disk);
                        cmd.Parameters.AddWithValue("@netD", netDown);
                        cmd.Parameters.AddWithValue("@netU", netUp);
                        cmd.Parameters.AddWithValue("@apps", appList);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[LỖI GHI DB] " + ex.Message);
                }
            }
        }

        // Hàm lấy thông số CPU, RAM, Network, Info và AppList mới nhất
        public string GetLatestResource(string clientId)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Dùng JOIN để kết nối bảng ResourceHistory (chứa mạng, phần cứng) và bảng Clients (chứa Tên máy, IP)
                    string query = @"
                        SELECT r.CpuPercent, r.RamPercent, r.NetworkDown, r.NetworkUp, r.AppList, c.MachineName, c.IP 
                        FROM ResourceHistory r 
                        JOIN Clients c ON r.ClientId = c.ClientId 
                        WHERE r.ClientId = @cId 
                        ORDER BY r.Timestamp DESC LIMIT 1";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cId", clientId);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string cpu = reader["CpuPercent"].ToString();
                                string ram = reader["RamPercent"].ToString();
                                string netDown = reader["NetworkDown"].ToString();
                                string netUp = reader["NetworkUp"].ToString();
                                string machineName = reader["MachineName"].ToString();
                                string ip = reader["IP"].ToString();

                                string appList = reader["AppList"] != DBNull.Value ? reader["AppList"].ToString() : "NONE";

                                // Trả về chuỗi gộp 7 thông số cách nhau bởi khoảng trắng
                                return $"{cpu} {ram} {netDown} {netUp} {machineName} {ip} {appList}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[LỖI ĐỌC LỊCH SỬ DB] " + ex.Message);
                }
                return null;
            }
        }
    }
}
