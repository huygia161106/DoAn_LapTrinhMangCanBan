namespace ClientA
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.label1 = new System.Windows.Forms.Label();
            this.lbUsername = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.TheoDoi = new System.Windows.Forms.TabPage();
            this.lblNetDown = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.button1 = new System.Windows.Forms.Button();
            this.lblNetUp = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblip = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblMachineName = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lblPercentDisk = new System.Windows.Forms.Label();
            this.lblPercentRam = new System.Windows.Forms.Label();
            this.lblPercentCPU = new System.Windows.Forms.Label();
            this.progressBar4 = new System.Windows.Forms.ProgressBar();
            this.progressBar3 = new System.Windows.Forms.ProgressBar();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label18 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.Admin = new System.Windows.Forms.TabPage();
            this.label23 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOwner = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.timerFetchData = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.TheoDoi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.Admin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.label1.Location = new System.Drawing.Point(19, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(366, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "Hệ Thống Theo Dõi Từ Xa   - ";
            // 
            // lbUsername
            // 
            this.lbUsername.AutoSize = true;
            this.lbUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lbUsername.Location = new System.Drawing.Point(418, 9);
            this.lbUsername.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbUsername.Name = "lbUsername";
            this.lbUsername.Size = new System.Drawing.Size(121, 31);
            this.lbUsername.TabIndex = 1;
            this.lbUsername.Text = "Tên user";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.TheoDoi);
            this.tabControl1.Controls.Add(this.Admin);
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.tabControl1.Location = new System.Drawing.Point(13, 56);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1445, 719);
            this.tabControl1.TabIndex = 2;
            // 
            // TheoDoi
            // 
            this.TheoDoi.Controls.Add(this.lblNetDown);
            this.TheoDoi.Controls.Add(this.label26);
            this.TheoDoi.Controls.Add(this.label25);
            this.TheoDoi.Controls.Add(this.label24);
            this.TheoDoi.Controls.Add(this.chart1);
            this.TheoDoi.Controls.Add(this.button1);
            this.TheoDoi.Controls.Add(this.lblNetUp);
            this.TheoDoi.Controls.Add(this.label9);
            this.TheoDoi.Controls.Add(this.label8);
            this.TheoDoi.Controls.Add(this.lblip);
            this.TheoDoi.Controls.Add(this.label6);
            this.TheoDoi.Controls.Add(this.label5);
            this.TheoDoi.Controls.Add(this.lblMachineName);
            this.TheoDoi.Controls.Add(this.label3);
            this.TheoDoi.Controls.Add(this.label2);
            this.TheoDoi.Controls.Add(this.splitContainer1);
            this.TheoDoi.Controls.Add(this.comboBox1);
            this.TheoDoi.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.TheoDoi.Location = new System.Drawing.Point(4, 34);
            this.TheoDoi.Margin = new System.Windows.Forms.Padding(4);
            this.TheoDoi.Name = "TheoDoi";
            this.TheoDoi.Padding = new System.Windows.Forms.Padding(4);
            this.TheoDoi.Size = new System.Drawing.Size(1437, 681);
            this.TheoDoi.TabIndex = 0;
            this.TheoDoi.Text = "Theo dõi";
            this.TheoDoi.UseVisualStyleBackColor = true;
            // 
            // lblNetDown
            // 
            this.lblNetDown.AutoSize = true;
            this.lblNetDown.Location = new System.Drawing.Point(1016, 69);
            this.lblNetDown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNetDown.Name = "lblNetDown";
            this.lblNetDown.Size = new System.Drawing.Size(0, 25);
            this.lblNetDown.TabIndex = 16;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(945, 69);
            this.label26.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(93, 25);
            this.label26.TabIndex = 15;
            this.label26.Text = "Recieved";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(748, 69);
            this.label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(59, 25);
            this.label25.TabIndex = 14;
            this.label25.Text = "Send";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(8, 506);
            this.label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(205, 25);
            this.label24.TabIndex = 13;
            this.label24.Text = "Biểu đồ theo thời gian:";
            // 
            // chart1
            // 
            chartArea1.AxisX.IsMarginVisible = false;
            chartArea1.AxisX.LabelStyle.Format = "HH:mm:ss";
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartArea1.AxisX.Title = "Thời gian";
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartArea1.AxisY.Maximum = 100D;
            chartArea1.AxisY.Minimum = 0D;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(235, 516);
            this.chart1.Margin = new System.Windows.Forms.Padding(4);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Bright;
            series1.BorderWidth = 3;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series1.Legend = "Legend1";
            series1.Name = "CPU";
            series2.BorderWidth = 3;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series2.Legend = "Legend1";
            series2.Name = "RAM";
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(952, 164);
            this.chart1.TabIndex = 12;
            this.chart1.Text = "chart1";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(168, 7);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(179, 38);
            this.button1.TabIndex = 11;
            this.button1.Text = "Xem Lịch Sử";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // lblNetUp
            // 
            this.lblNetUp.AutoSize = true;
            this.lblNetUp.Location = new System.Drawing.Point(819, 69);
            this.lblNetUp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNetUp.Name = "lblNetUp";
            this.lblNetUp.Size = new System.Drawing.Size(0, 25);
            this.lblNetUp.TabIndex = 10;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(669, 69);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(68, 25);
            this.label9.TabIndex = 9;
            this.label9.Text = "Mạng:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(643, 69);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 25);
            this.label8.TabIndex = 8;
            this.label8.Text = "|";
            // 
            // lblip
            // 
            this.lblip.AutoSize = true;
            this.lblip.Location = new System.Drawing.Point(499, 69);
            this.lblip.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblip.Name = "lblip";
            this.lblip.Size = new System.Drawing.Size(0, 28);
            this.lblip.TabIndex = 7;
            this.lblip.UseCompatibleTextRendering = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(453, 69);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 25);
            this.label6.TabIndex = 6;
            this.label6.Text = "IP:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(427, 69);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 25);
            this.label5.TabIndex = 5;
            this.label5.Text = "|";
            // 
            // lblMachineName
            // 
            this.lblMachineName.AutoSize = true;
            this.lblMachineName.Location = new System.Drawing.Point(257, 69);
            this.lblMachineName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMachineName.Name = "lblMachineName";
            this.lblMachineName.Size = new System.Drawing.Size(0, 25);
            this.lblMachineName.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(137, 69);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 25);
            this.label3.TabIndex = 3;
            this.label3.Text = "Tên máy:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 69);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Thông tin:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(0, 101);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lblPercentDisk);
            this.splitContainer1.Panel1.Controls.Add(this.lblPercentRam);
            this.splitContainer1.Panel1.Controls.Add(this.lblPercentCPU);
            this.splitContainer1.Panel1.Controls.Add(this.progressBar4);
            this.splitContainer1.Panel1.Controls.Add(this.progressBar3);
            this.splitContainer1.Panel1.Controls.Add(this.progressBar1);
            this.splitContainer1.Panel1.Controls.Add(this.label14);
            this.splitContainer1.Panel1.Controls.Add(this.label13);
            this.splitContainer1.Panel1.Controls.Add(this.label12);
            this.splitContainer1.Panel1.Controls.Add(this.label11);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView1);
            this.splitContainer1.Panel2.Controls.Add(this.label18);
            this.splitContainer1.Size = new System.Drawing.Size(1435, 369);
            this.splitContainer1.SplitterDistance = 169;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // lblPercentDisk
            // 
            this.lblPercentDisk.AutoSize = true;
            this.lblPercentDisk.Location = new System.Drawing.Point(499, 119);
            this.lblPercentDisk.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPercentDisk.Name = "lblPercentDisk";
            this.lblPercentDisk.Size = new System.Drawing.Size(0, 25);
            this.lblPercentDisk.TabIndex = 9;
            // 
            // lblPercentRam
            // 
            this.lblPercentRam.AutoSize = true;
            this.lblPercentRam.Location = new System.Drawing.Point(499, 84);
            this.lblPercentRam.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPercentRam.Name = "lblPercentRam";
            this.lblPercentRam.Size = new System.Drawing.Size(0, 25);
            this.lblPercentRam.TabIndex = 8;
            // 
            // lblPercentCPU
            // 
            this.lblPercentCPU.AutoSize = true;
            this.lblPercentCPU.Location = new System.Drawing.Point(499, 48);
            this.lblPercentCPU.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPercentCPU.Name = "lblPercentCPU";
            this.lblPercentCPU.Size = new System.Drawing.Size(0, 25);
            this.lblPercentCPU.TabIndex = 7;
            // 
            // progressBar4
            // 
            this.progressBar4.Location = new System.Drawing.Point(77, 119);
            this.progressBar4.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar4.Name = "progressBar4";
            this.progressBar4.Size = new System.Drawing.Size(393, 28);
            this.progressBar4.TabIndex = 6;
            // 
            // progressBar3
            // 
            this.progressBar3.Location = new System.Drawing.Point(77, 84);
            this.progressBar3.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar3.Name = "progressBar3";
            this.progressBar3.Size = new System.Drawing.Size(393, 28);
            this.progressBar3.TabIndex = 5;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(77, 48);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(393, 28);
            this.progressBar1.TabIndex = 4;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 119);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(65, 25);
            this.label14.TabIndex = 3;
            this.label14.Text = "DISK:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 84);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(62, 25);
            this.label13.TabIndex = 2;
            this.label13.Text = "RAM:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 48);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(60, 25);
            this.label12.TabIndex = 1;
            this.label12.Text = "CPU:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 11);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(197, 25);
            this.label11.TabIndex = 0;
            this.label11.Text = "Tài nguyên hệ thống:";
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(8, 29);
            this.listView1.Margin = new System.Windows.Forms.Padding(4);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1392, 162);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Process Name";
            this.columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Window Title";
            this.columnHeader2.Width = 419;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Memory";
            this.columnHeader3.Width = 300;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(8, 0);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(295, 25);
            this.label18.TabIndex = 0;
            this.label18.Text = "Danh sách ứng dụng đang chạy:";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(8, 7);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(125, 33);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.Text = "Client";
            // 
            // Admin
            // 
            this.Admin.Controls.Add(this.label23);
            this.Admin.Controls.Add(this.dataGridView1);
            this.Admin.Controls.Add(this.button2);
            this.Admin.Controls.Add(this.textBox3);
            this.Admin.Controls.Add(this.textBox2);
            this.Admin.Controls.Add(this.textBox1);
            this.Admin.Controls.Add(this.label22);
            this.Admin.Controls.Add(this.label21);
            this.Admin.Controls.Add(this.label20);
            this.Admin.Controls.Add(this.label19);
            this.Admin.Location = new System.Drawing.Point(4, 34);
            this.Admin.Margin = new System.Windows.Forms.Padding(4);
            this.Admin.Name = "Admin";
            this.Admin.Padding = new System.Windows.Forms.Padding(4);
            this.Admin.Size = new System.Drawing.Size(1437, 681);
            this.Admin.TabIndex = 1;
            this.Admin.Text = "Admin";
            this.Admin.UseVisualStyleBackColor = true;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(36, 322);
            this.label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(303, 25);
            this.label23.TabIndex = 9;
            this.label23.Text = "--- Danh Sách Client Đã Thêm ---";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colID,
            this.colName,
            this.colOwner});
            this.dataGridView1.Location = new System.Drawing.Point(41, 367);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(1284, 213);
            this.dataGridView1.TabIndex = 8;
            // 
            // colID
            // 
            this.colID.HeaderText = "Client ID";
            this.colID.MinimumWidth = 6;
            this.colID.Name = "colID";
            this.colID.ReadOnly = true;
            // 
            // colName
            // 
            this.colName.HeaderText = "Tên Máy";
            this.colName.MinimumWidth = 6;
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colOwner
            // 
            this.colOwner.HeaderText = "Người Sở Hữu";
            this.colOwner.MinimumWidth = 6;
            this.colOwner.Name = "colOwner";
            this.colOwner.ReadOnly = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(41, 229);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(137, 48);
            this.button2.TabIndex = 7;
            this.button2.Text = "Thêm Client";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(217, 159);
            this.textBox3.Margin = new System.Windows.Forms.Padding(4);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(297, 30);
            this.textBox3.TabIndex = 6;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(217, 118);
            this.textBox2.Margin = new System.Windows.Forms.Padding(4);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(297, 30);
            this.textBox2.TabIndex = 5;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(217, 76);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(297, 30);
            this.textBox1.TabIndex = 4;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(36, 166);
            this.label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(76, 25);
            this.label22.TabIndex = 3;
            this.label22.Text = "Owner:";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(36, 126);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(150, 25);
            this.label21.TabIndex = 2;
            this.label21.Text = "Machine Name:";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(36, 84);
            this.label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(87, 25);
            this.label20.TabIndex = 1;
            this.label20.Text = "ClientID:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(36, 31);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(206, 25);
            this.label19.TabIndex = 0;
            this.label19.Text = "--- Thêm Client mới ---";
            // 
            // timerFetchData
            // 
            this.timerFetchData.Enabled = true;
            this.timerFetchData.Interval = 2000;
            this.timerFetchData.Tick += new System.EventHandler(this.timerFetchData_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1643, 800);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.lbUsername);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.tabControl1.ResumeLayout(false);
            this.TheoDoi.ResumeLayout(false);
            this.TheoDoi.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.Admin.ResumeLayout(false);
            this.Admin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbUsername;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage TheoDoi;
        private System.Windows.Forms.TabPage Admin;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblMachineName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblNetUp;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblip;
        private System.Windows.Forms.ProgressBar progressBar4;
        private System.Windows.Forms.ProgressBar progressBar3;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblPercentDisk;
        private System.Windows.Forms.Label lblPercentRam;
        private System.Windows.Forms.Label lblPercentCPU;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOwner;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label lblNetDown;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Timer timerFetchData;
    }
}