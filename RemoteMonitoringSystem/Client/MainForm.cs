using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Client
{
    public partial class MainForm : Form
    {
        // 1. Khai báo các thực thể mạng toàn cục
        private TcpClient client;
        private SslStream sslStream;
        private StreamReader reader;
        private StreamWriter writer;
        private string currentShareCode = "";
        private string currentUserRole; // Thêm biến lưu quyền
        private string currentUsername;
        // Biến dùng để Sort A-Z, Z-A
        private int sortColumn = -1; // Cột đang được chọn để sort
        private SortOrder sortOrder = SortOrder.None; // Chiều sort

        public MainForm(string username, string role)
        {
            InitializeComponent();
            SetupChartAppearance();
            currentUsername = username;

            this.listView1.ColumnClick += new ColumnClickEventHandler(listView1_ColumnClick);

            // Gắn sự kiện Load để tự động kết nối khi mở Form
            this.Load += MainForm_Load;

            // Nhận quyền và áp dụng thiết lập
            currentUserRole = role;
            lbUsername.Text = $"{currentUsername} ({currentUserRole})";
            ApplySecurityPolicies();
        }

        // 2. Tự động thiết lập đường hầm bảo mật khi Form hiện lên
        private void ApplySecurityPolicies()
        {
            if (currentUserRole != "Admin")
            {
                // Ẩn tab admin
                if (guna2TabControl1.TabPages.Count > 1)
                {
                    guna2TabControl1.TabPages.RemoveAt(1);
                }

                // User phải nhập Share Code
                txtShareCode.Visible = true;
                btnConnectCode.Visible = true;

                // User không được kill process
                if (menuEndTaskToolStripMenuItem != null)
                {
                    menuEndTaskToolStripMenuItem.Enabled = false;
                }

                this.Text = "Remote Monitor - USER MODE";
            }
            else
            {
                // Admin không cần nhập code
                txtShareCode.Visible = false;
                btnConnectCode.Visible = false;

                this.Text = "Remote Monitor - ADMIN MODE";
            }
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            bool connected = await ConnectToServerAsync();
            if (connected)
            {
                // Chỉ bắt đầu kéo dữ liệu khi đã thông luồng mTLS
                timerFetchData.Start();
            }
        }

        private async Task<bool> ConnectToServerAsync()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 8888);
                NetworkStream netStream = client.GetStream();

                // Thiết lập lớp giáp SslStream
                sslStream = new SslStream(netStream, false, ValidateServerCertificate);

                // Tải chứng chỉ ECC mTLS của Client A
                X509Certificate2 clientCertificate = new X509Certificate2("ClientCertECC.pfx", "NT106.Q23");
                X509CertificateCollection clientCerts = new X509CertificateCollection(new X509Certificate[] { clientCertificate });

                await sslStream.AuthenticateAsClientAsync("RemoteMonitorServer", clientCerts, SslProtocols.Tls12, false);

                // Khởi tạo các bộ đọc/ghi chuỗi
                reader = new StreamReader(sslStream, Encoding.UTF8);
                writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true };

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối Dashboard: {ex.Message}", "Lỗi mTLS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SetupChartAppearance()
        {
            chart1.Series["CPU"].Color = System.Drawing.Color.LimeGreen;
            chart1.Series["RAM"].Color = System.Drawing.Color.DodgerBlue;
            chart1.Series["CPU"].BorderWidth = 2;
            chart1.Series["RAM"].BorderWidth = 2;

            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
        }

        public void UpdateResourceChart(double cpuPercent, double ramPercent, double diskPercent)
        {
            if (this.chart1.InvokeRequired)
            {
                this.chart1.Invoke(new Action(() => UpdateResourceChart(cpuPercent, ramPercent, diskPercent)));
                return;
            }

            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            // Cập nhật biểu đồ đường cho CPU và RAM
            chart1.Series["CPU"].Points.AddXY(currentTime, cpuPercent);
            chart1.Series["RAM"].Points.AddXY(currentTime, ramPercent);

            // Giữ lại 30 điểm dữ liệu gần nhất để biểu đồ không bị quá dày
            if (chart1.Series["CPU"].Points.Count > 30)
            {
                chart1.Series["CPU"].Points.RemoveAt(0);
                chart1.Series["RAM"].Points.RemoveAt(0);
            }

            // --- Cập nhật các ProgressBar ---
            progressBar1.Value = (int)Math.Min(100, Math.Max(0, cpuPercent));
            progressBar3.Value = (int)Math.Min(100, Math.Max(0, ramPercent));

            // Cập nhật ProgressBar4 cho Ổ đĩa
            if (progressBar4 != null)
            {
                progressBar4.Value = (int)Math.Min(100, Math.Max(0, diskPercent));
            }

            // --- Cập nhật các Label % ---
            lblPercentCPU.Text = $"{Math.Round(cpuPercent, 1)}%";
            lblPercentRam.Text = $"{Math.Round(ramPercent, 1)}%";

            // Cập nhật Label cho Ổ đĩa
            if (lblPercentDisk != null)
            {
                lblPercentDisk.Text = $"{Math.Round(diskPercent, 1)}%";
            }
        }

        public void UpdateAppList(string appListData)
        {
            if (this.listView1.InvokeRequired)
            {
                this.listView1.Invoke(new Action(() => UpdateAppList(appListData)));
                return;
            }

            if (string.IsNullOrWhiteSpace(appListData) || appListData == "NONE")
            {
                listView1.Items.Clear();
                return;
            }

            // --- LƯU LẠI TIẾN TRÌNH ĐANG CHỌN (Tránh bị mất focus khi click) ---
            string selectedProcess = "";
            if (listView1.SelectedItems.Count > 0)
            {
                selectedProcess = listView1.SelectedItems[0].Text; // Ghi nhớ file .exe bạn đang click
            }

            // 1. LƯU LẠI VỊ TRÍ THANH CUỘN HIỆN TẠI
            int topItemIndex = 0;
            if (listView1.TopItem != null)
            {
                topItemIndex = listView1.TopItem.Index; // Lấy index của dòng đang hiển thị trên cùng
            }

            // 2. KHÓA VẼ GIAO DIỆN (Chống hiện tượng chớp nháy màn hình)
            listView1.BeginUpdate();

            listView1.Items.Clear();

            // Cắt chuỗi và thêm dữ liệu
            string[] apps = appListData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string app in apps)
            {
                string[] details = app.Split('|');
                if (details.Length >= 3)
                {
                    ListViewItem item = new ListViewItem(details[0]); // Process Name
                    item.SubItems.Add(details[1]);                    // Window Title
                    item.SubItems.Add(details[2]);                    // Memory

                    listView1.Items.Add(item);
                }
            }

            // Tự động sắp xếp lại danh sách sau khi có dữ liệu mới
            if (sortColumn != -1)
            {
                listView1.Sort();
            }

            // 3. PHỤC HỒI LẠI VỊ TRÍ THANH CUỘN
            if (listView1.Items.Count > topItemIndex)
            {
                // Ép ListView cuộn xuống đúng dòng hồi nãy
                listView1.TopItem = listView1.Items[topItemIndex];
            }

            // --- BƯỚC MỚI: PHỤC HỒI LẠI TIẾN TRÌNH ĐÃ CHỌN ---
            if (!string.IsNullOrEmpty(selectedProcess)) 
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Text == selectedProcess)
                    {
                        item.Selected = true;
                        // item.Focused = true; // Bỏ comment dòng này nếu bạn muốn có thêm khung viền nét đứt bao quanh
                        break; // Tìm thấy rồi thì thoát vòng lặp cho nhẹ máy
                    }
                }
            }

            // 4. MỞ LẠI GIAO DIỆN
            listView1.EndUpdate();
        }


        // Hàm hiển thị thông tin tĩnh lên các Label (An toàn đa luồng)
        public void UpdateSystemInfo(string machineName, string ip, string netDown, string netUp)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateSystemInfo(machineName, ip, netDown, netUp)));
                return;
            }

            lblMachineName.Text = machineName;
            lblip.Text = ip;
            lblNetDown.Text = $"{netDown} KB/s";
            lblNetUp.Text = $"{netUp} KB/s";
        }

        private async void timerFetchData_Tick(object sender, EventArgs e)
        {
            try
            {
                // 1. Kiểm tra mạng và xem đã chọn máy nào chưa
                if (writer == null || reader == null) return;
                if (string.IsNullOrEmpty(currentShareCode))
                    return;

                // 2. GỬI YÊU CẦU LẤY DỮ LIỆU
                var fetchRequest = new
                {
                    Type = "GET_LATEST_BY_CODE",
                    ShareCode = currentShareCode
                };
                await writer.WriteLineAsync(JsonConvert.SerializeObject(fetchRequest));

                // 3. NHẬN PHẢN HỒI TỪ SERVER
                string response = await reader.ReadLineAsync();

                if (response != null && response.StartsWith("LATEST_DATA"))
                {
                    // Lấy chuỗi JSON thực sự ở phía sau chữ "LATEST_DATA "
                    string payload = response.Substring("LATEST_DATA ".Length).Trim();

                    // --- ĐÂY LÀ ĐIỂM QUAN TRỌNG NHẤT: DÙNG JSON ĐỂ GIẢI MÃ ---
                    dynamic data = JsonConvert.DeserializeObject(payload);

                    if (data != null)
                    {
                        // A. Cập nhật Biểu đồ (Ép kiểu an toàn từ JSON)
                        double cpuPercent = Convert.ToDouble(data.Cpu);
                        double ramPercent = Convert.ToDouble(data.Ram);
                        double diskPercent = Convert.ToDouble(data.Disk);

                        UpdateResourceChart(cpuPercent, ramPercent, diskPercent);

                        // B. Cập nhật Label hệ thống
                        string machineName = (string)data.MachineName;
                        string ip = (string)data.IP;
                        string netDown = (string)data.NetDown;
                        string netUp = (string)data.NetUp;

                        UpdateSystemInfo(machineName, ip, netDown, netUp);

                        // C. Cập nhật Danh sách tiến trình
                        string appList = (string)data.AppList;
                        if (string.IsNullOrEmpty(appList) || appList == "NONE")
                        {
                            UpdateAppList("NONE");
                        }
                        else
                        {
                            UpdateAppList(appList);
                        }
                    }
                }
                else if (response == "NO_DATA")
                {
                    this.Invoke(new Action(() => {
                        lblip.Text = "Trạng thái: Máy trạm chưa có dữ liệu.";
                    }));
                }

                else if (response != null && response.StartsWith("CLIENT_LIST"))
                {
                    string listPayload = response.Substring("CLIENT_LIST ".Length).Trim();
                    dynamic clients = JsonConvert.DeserializeObject(listPayload);

                    this.Invoke(new Action(() => {
                        dgvClients.Rows.Clear();
                        foreach (var c in clients)
                        {
                            // Gắn vào bảng: Cột 0 (ID), Cột 1 (Tên máy), Cột 2 (IP)
                            dgvClients.Rows.Add((string)c.ClientId, (string)c.MachineName, (string)c.IP);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                timerFetchData.Stop();
                MessageBox.Show($"Mất kết nối với Server Dashboard: {ex.Message}",
                                "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null) return false;
            X509Certificate2 cert2 = new X509Certificate2(certificate);
            // Xác thực dựa trên Root CA của trường
            return cert2.Issuer.Contains("UIT_ECC_RootCA") && cert2.Subject.Contains("RemoteMonitorServer");
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            writer?.Close();
            client?.Close();
            Application.Exit();
        }

        private async void EndTask_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string pName = listView1.SelectedItems[0].Text;
                var cmd = new { Type = "REMOTE_KILL", TargetClientId = "1", ProcessName = pName };
                await writer.WriteLineAsync(JsonConvert.SerializeObject(cmd));
                MessageBox.Show($"Đã gửi lệnh tắt: {pName}");
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Kiểm tra xem có đang click vào chính cột cũ không
            if (e.Column == sortColumn)
            {
                // Đảo chiều sắp xếp (A-Z thành Z-A và ngược lại)
                sortOrder = (sortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Nếu click cột mới, mặc định xếp A-Z
                sortColumn = e.Column;
                sortOrder = SortOrder.Ascending;
            }

            // Gán bộ quy tắc sắp xếp mới và thực thi
            listView1.ListViewItemSorter = new ListViewItemComparer(sortColumn, sortOrder);
            listView1.Sort();
        }

        public class ListViewItemComparer : System.Collections.IComparer
        {
            private int col;
            private SortOrder order;

            public ListViewItemComparer(int column, SortOrder order)
            {
                col = column;
                this.order = order;
            }

            public int Compare(object x, object y)
            {
                int returnVal = -1;
                string strX = ((ListViewItem)x).SubItems[col].Text;
                string strY = ((ListViewItem)y).SubItems[col].Text;

                // Xử lý ĐẶC BIỆT cho cột Memory (Cột số 2) - Cần bóc tách số ra để so sánh
                if (col == 2)
                {
                    double numX = ExtractNumber(strX);
                    double numY = ExtractNumber(strY);
                    returnVal = numX.CompareTo(numY);
                }
                else
                {
                    // Các cột khác (Process Name, Window Title) thì so sánh chữ cái bình thường (A-Z)
                    returnVal = String.Compare(strX, strY);
                }

                // Nếu người dùng muốn xếp Z-A (Giảm dần), ta lật ngược kết quả
                if (order == SortOrder.Descending)
                    returnVal *= -1;

                return returnVal;
            }

            // Hàm hỗ trợ lọc bỏ chữ "MB", chỉ lấy con số để tính toán
            private double ExtractNumber(string input)
            {
                string numberOnly = System.Text.RegularExpressions.Regex.Replace(input, "[^0-9.]", "");
                if (double.TryParse(numberOnly, out double result))
                    return result;
                return 0;
            }
        }

        private async void EndTaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string pName = listView1.SelectedItems[0].Text;
                string windowTitle = listView1.SelectedItems[0].SubItems[1].Text;

                // Nếu chưa chọn máy nào thì báo lỗi
                if (string.IsNullOrEmpty(currentShareCode))
                {
                    MessageBox.Show("Vui lòng chọn một máy tính từ Tab Admin trước!", "Lỗi");
                    return;
                }

                var confirm = MessageBox.Show($"Bạn có chắc chắn muốn tắt ứng dụng: {windowTitle} ({pName}) trên máy [{currentShareCode}]?",
                                             "Xác nhận Remote Kill",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        var request = new
                        {
                            Type = "REMOTE_KILL",
                            TargetClientId = currentShareCode, // Gửi lệnh Kill tới ID đang theo dõi
                            ProcessName = pName
                        };

                        string jsonRequest = JsonConvert.SerializeObject(request);

                        if (writer != null)
                        {
                            await writer.WriteLineAsync(jsonRequest);
                            MessageBox.Show($"Đã gửi lệnh tắt {pName} thành công!", "Thông báo");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi gửi lệnh: " + ex.Message);
                    }
                }
            }
        }

        private void dgvClients_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string selectedId = dgvClients.Rows[e.RowIndex].Cells[0].Value?.ToString();
                if (!string.IsNullOrEmpty(selectedId))
                {
                    currentShareCode = selectedId; // Chuyển mục tiêu theo dõi

                    // Xóa rỗng biểu đồ và bảng ứng dụng cũ để chuẩn bị đón data máy mới
                    chart1.Series["CPU"].Points.Clear();
                    chart1.Series["RAM"].Points.Clear();
                    listView1.Items.Clear();

                    // Nhảy sang Tab Theo dõi
                    guna2TabControl1.SelectedIndex = 0;
                }
            }
        }

        private async void btnRefreshList_Click(object sender, EventArgs e)
        {
            if (writer != null)
            {
                var request = new { Type = "GET_ALL_CLIENTS" };
                await writer.WriteLineAsync(JsonConvert.SerializeObject(request));
            }
        }
        private void btnConnectCode_Click(object sender, EventArgs e)
        {
            string code = txtShareCode.Text.Trim();

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Vui lòng nhập Share Code!");
                return;
            }

            currentShareCode = code;

            chart1.Series["CPU"].Points.Clear();
            chart1.Series["RAM"].Points.Clear();

            listView1.Items.Clear();

            MessageBox.Show($"Đã kết nối tới máy: {code}");
        }
    }
}