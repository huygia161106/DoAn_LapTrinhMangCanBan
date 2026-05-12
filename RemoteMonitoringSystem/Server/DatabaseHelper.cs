using System;
using System.Data.SQLite;
using System.IO;
using Newtonsoft.Json;

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
        public string ValidateUser(string username, string passwordHash)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Lấy thẳng cột Role ra thay vì dùng COUNT(1)
                    string query = "SELECT Role FROM Users WHERE Username = @user AND PasswordHash = @pass";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", passwordHash);

                        object result = cmd.ExecuteScalar();

                        // Nếu đúng tài khoản, trả về Role (Admin/User). Nếu sai, trả về null.
                        return result != null ? result.ToString() : null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[LỖI DB] " + ex.Message);
                    return null;
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

        // Hàm lấy thông số CPU, RAM, Disk, Network, Info và AppList mới nhất (JSON)
        public string GetLatestResource(string clientId)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                SELECT r.CpuPercent, r.RamPercent, r.DiskPercent, r.NetworkDown, r.NetworkUp, r.AppList, c.MachineName, c.IP 
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
                                // Đóng gói toàn bộ dữ liệu thành một đối tượng ẩn danh (Anonymous Object)
                                var resourceData = new
                                {
                                    Cpu = reader["CpuPercent"].ToString(),
                                    Ram = reader["RamPercent"].ToString(),
                                    Disk = reader["DiskPercent"].ToString(), 
                                    NetDown = reader["NetworkDown"].ToString(),
                                    NetUp = reader["NetworkUp"].ToString(),
                                    MachineName = reader["MachineName"].ToString(),
                                    IP = reader["IP"].ToString(),
                                    AppList = reader["AppList"] != DBNull.Value ? reader["AppList"].ToString() : "NONE"
                                };

                                // Chuyển đối tượng thành chuỗi chuẩn JSON và trả về cho ServerForm
                                return JsonConvert.SerializeObject(resourceData);
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

        //Hàm tự động cấp phát ID (Nếu máy mới thì tạo ID mới, máy cũ thì trả về ID cũ + Cập nhật IP)
        public string RegisterOrGetAgent(string machineName, string ip)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Lệnh INSERT ON CONFLICT (Đòi hỏi MachineName phải UNIQUE): 
                    // Nếu Tên máy chưa có -> Tạo mới (tự động nhảy ID). Nếu có rồi -> Cập nhật lại IP và LastActive.
                    string query = @"
                INSERT INTO Clients (MachineName, IP, OwnerUserId, LastActive) 
                VALUES (@name, @ip, 1, CURRENT_TIMESTAMP)
                ON CONFLICT(MachineName) DO UPDATE SET 
                IP = excluded.IP, LastActive = CURRENT_TIMESTAMP;
                
                SELECT ClientId FROM Clients WHERE MachineName = @name;";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", machineName);
                        cmd.Parameters.AddWithValue("@ip", ip);
                        return cmd.ExecuteScalar().ToString(); // Trả về ID (dạng chuỗi số 1, 2, 3...)
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[LỖI CẤP ID] " + ex.Message);
                    return "UNKNOWN";
                }
            }
        }

        // Hàm lấy danh sách tất cả Client cho Tab Admin
        public string GetAllClientsList()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ClientId, MachineName, IP, LastActive FROM Clients";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        var clientsList = new System.Collections.Generic.List<object>();
                        while (reader.Read())
                        {
                            clientsList.Add(new
                            {
                                ClientId = reader["ClientId"].ToString(),
                                MachineName = reader["MachineName"].ToString(),
                                IP = reader["IP"].ToString()
                            });
                        }
                        return JsonConvert.SerializeObject(clientsList);
                    }
                }
                catch { return "[]"; }
            }
        }
    }
}
