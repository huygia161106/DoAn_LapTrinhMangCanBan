using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientA
{
    public partial class MainForm : Form
    {
        // 1. Khai báo các thực thể mạng toàn cục
        private TcpClient client;
        private SslStream sslStream;
        private StreamReader reader;
        private StreamWriter writer;

        // Biến dùng để Sort A-Z, Z-A
        private int sortColumn = -1; // Cột đang được chọn để sort
        private SortOrder sortOrder = SortOrder.None; // Chiều sort

        public MainForm()
        {
            InitializeComponent();
            SetupChartAppearance();

            this.listView1.ColumnClick += new ColumnClickEventHandler(listView1_ColumnClick);

            // Gắn sự kiện Load để tự động kết nối khi mở Form
            this.Load += MainForm_Load;
        }

        // 2. Tự động thiết lập đường hầm bảo mật khi Form hiện lên
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

        public void UpdateResourceChart(double cpuPercent, double ramPercent)
        {
            if (this.chart1.InvokeRequired)
            {
                this.chart1.Invoke(new Action(() => UpdateResourceChart(cpuPercent, ramPercent)));
                return;
            }

            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            chart1.Series["CPU"].Points.AddXY(currentTime, cpuPercent);
            chart1.Series["RAM"].Points.AddXY(currentTime, ramPercent);

            // Giữ lại 30 điểm dữ liệu gần nhất để biểu đồ không bị quá dày
            if (chart1.Series["CPU"].Points.Count > 30)
            {
                chart1.Series["CPU"].Points.RemoveAt(0);
                chart1.Series["RAM"].Points.RemoveAt(0);
            }

            // Cập nhật các thành phần UI khác
            progressBar1.Value = (int)Math.Min(100, Math.Max(0, cpuPercent));
            progressBar3.Value = (int)Math.Min(100, Math.Max(0, ramPercent));
            lblPercentCPU.Text = $"{Math.Round(cpuPercent, 1)}%";
            lblPercentRam.Text = $"{Math.Round(ramPercent, 1)}%";
        }

        // Hàm cập nhật danh sách ứng dụng lên ListView an toàn và giữ nguyên vị trí cuộn
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

            // Client B đang gửi lên đơn vị là KBps, ta thêm hậu tố cho đẹp
            lblNetDown.Text = $"{netDown} KB/s";
            lblNetUp.Text = $"{netUp} KB/s";
        }

        private async void timerFetchData_Tick(object sender, EventArgs e)
        {
            try
            {
                if (writer == null || reader == null) return;

                // Gửi lệnh lấy dữ liệu mới nhất (ID mặc định là 1)
                await writer.WriteLineAsync("GET_LATEST 1");

                string response = await reader.ReadLineAsync();

                if (response != null && response.StartsWith("LATEST_DATA"))
                {
                    // Cắt bỏ chữ LATEST_DATA ở đầu
                    string data = response.Substring("LATEST_DATA ".Length);

                    // Cắt chuỗi thành TỐI ĐA 7 phần (để bảo toàn các khoảng trắng trong AppList ở phần cuối cùng)
                    string[] parts = data.Split(new char[] { ' ' }, 7);

                    if (parts.Length >= 6) // Đảm bảo có ít nhất 6 thông số cơ bản
                    {
                        // 1. Gán dữ liệu cho Biểu đồ
                        double cpuPercent = Convert.ToDouble(parts[0]);
                        double ramPercent = Convert.ToDouble(parts[1]);
                        UpdateResourceChart(cpuPercent, ramPercent);

                        // 2. Gán dữ liệu cho Label Hệ thống (Tên máy, IP, Mạng)
                        string netDown = parts[2];
                        string netUp = parts[3];
                        string machineName = parts[4];
                        string ip = parts[5];
                        UpdateSystemInfo(machineName, ip, netDown, netUp);

                        // 3. Gán dữ liệu cho Bảng ứng dụng (Nếu có phần thứ 7)
                        if (parts.Length == 7)
                        {
                            UpdateAppList(parts[6]);
                        }
                        else
                        {
                            UpdateAppList("NONE");
                        }
                    }
                }
            }

            catch
            {
                timerFetchData.Stop();
                MessageBox.Show("Mất kết nối luồng giám sát với Server!");
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
    }
}