namespace ShadowsocksR.View
{
    partial class ServerLogForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            this.ServerDataGrid = new ShadowsocksR.View.ServerLogForm.DoubleBufferListView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Group = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TotalConnect = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Connecting = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AvgLatency = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AvgDownSpeed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaxDownSpeed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AvgUpSpeed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaxUpSpeed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Download = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Upload = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DownloadRaw = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ErrorPercent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConnectError = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConnectTimeout = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConnectEmpty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Continuous = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.ServerDataGrid)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ServerDataGrid
            // 
            this.ServerDataGrid.AllowUserToAddRows = false;
            this.ServerDataGrid.AllowUserToDeleteRows = false;
            this.ServerDataGrid.AllowUserToResizeRows = false;
            this.ServerDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ServerDataGrid.ColumnHeadersHeight = 50;
            this.ServerDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.Group,
            this.Server,
            this.TotalConnect,
            this.Connecting,
            this.AvgLatency,
            this.AvgDownSpeed,
            this.MaxDownSpeed,
            this.AvgUpSpeed,
            this.MaxUpSpeed,
            this.Download,
            this.Upload,
            this.DownloadRaw,
            this.ErrorPercent,
            this.ConnectError,
            this.ConnectTimeout,
            this.ConnectEmpty,
            this.Continuous});
            this.ServerDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ServerDataGrid.Location = new System.Drawing.Point(0, 0);
            this.ServerDataGrid.Margin = new System.Windows.Forms.Padding(0);
            this.ServerDataGrid.MinimumSize = new System.Drawing.Size(1, 1);
            this.ServerDataGrid.MultiSelect = false;
            this.ServerDataGrid.Name = "ServerDataGrid";
            this.ServerDataGrid.ReadOnly = true;
            this.ServerDataGrid.RowHeadersVisible = false;
            this.ServerDataGrid.RowHeadersWidth = 50;
            this.ServerDataGrid.RowTemplate.Height = 23;
            this.ServerDataGrid.Size = new System.Drawing.Size(784, 461);
            this.ServerDataGrid.TabIndex = 0;
            this.ServerDataGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ServerDataGrid_CellClick);
            // 
            // ID
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.ID.DefaultCellStyle = dataGridViewCellStyle1;
            this.ID.HeaderText = "ID";
            this.ID.MinimumWidth = 40;
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Width = 40;
            // 
            // Group
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.Group.DefaultCellStyle = dataGridViewCellStyle2;
            this.Group.HeaderText = "Group";
            this.Group.MinimumWidth = 100;
            this.Group.Name = "Group";
            this.Group.ReadOnly = true;
            // 
            // Server
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.Server.DefaultCellStyle = dataGridViewCellStyle3;
            this.Server.HeaderText = "Server";
            this.Server.MinimumWidth = 200;
            this.Server.Name = "Server";
            this.Server.ReadOnly = true;
            this.Server.Width = 200;
            // 
            // TotalConnect
            // 
            this.TotalConnect.HeaderText = "Total Connect";
            this.TotalConnect.MinimumWidth = 60;
            this.TotalConnect.Name = "TotalConnect";
            this.TotalConnect.ReadOnly = true;
            this.TotalConnect.Visible = false;
            this.TotalConnect.Width = 60;
            // 
            // Connecting
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Connecting.DefaultCellStyle = dataGridViewCellStyle4;
            this.Connecting.HeaderText = "Connecting";
            this.Connecting.MinimumWidth = 60;
            this.Connecting.Name = "Connecting";
            this.Connecting.ReadOnly = true;
            this.Connecting.Width = 60;
            // 
            // AvgLatency
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.AvgLatency.DefaultCellStyle = dataGridViewCellStyle5;
            this.AvgLatency.HeaderText = "Latency";
            this.AvgLatency.MinimumWidth = 60;
            this.AvgLatency.Name = "AvgLatency";
            this.AvgLatency.ReadOnly = true;
            this.AvgLatency.Width = 60;
            // 
            // AvgDownSpeed
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.AvgDownSpeed.DefaultCellStyle = dataGridViewCellStyle6;
            this.AvgDownSpeed.HeaderText = "Avg DSpeed";
            this.AvgDownSpeed.MinimumWidth = 60;
            this.AvgDownSpeed.Name = "AvgDownSpeed";
            this.AvgDownSpeed.ReadOnly = true;
            this.AvgDownSpeed.Width = 60;
            // 
            // MaxDownSpeed
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.MaxDownSpeed.DefaultCellStyle = dataGridViewCellStyle7;
            this.MaxDownSpeed.HeaderText = "Max DSpeed";
            this.MaxDownSpeed.MinimumWidth = 60;
            this.MaxDownSpeed.Name = "MaxDownSpeed";
            this.MaxDownSpeed.ReadOnly = true;
            this.MaxDownSpeed.Visible = false;
            this.MaxDownSpeed.Width = 60;
            // 
            // AvgUpSpeed
            // 
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.AvgUpSpeed.DefaultCellStyle = dataGridViewCellStyle8;
            this.AvgUpSpeed.HeaderText = "Avg UpSpeed";
            this.AvgUpSpeed.MinimumWidth = 60;
            this.AvgUpSpeed.Name = "AvgUpSpeed";
            this.AvgUpSpeed.ReadOnly = true;
            this.AvgUpSpeed.Width = 60;
            // 
            // MaxUpSpeed
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.MaxUpSpeed.DefaultCellStyle = dataGridViewCellStyle9;
            this.MaxUpSpeed.HeaderText = "Max UpSpeed";
            this.MaxUpSpeed.MinimumWidth = 60;
            this.MaxUpSpeed.Name = "MaxUpSpeed";
            this.MaxUpSpeed.ReadOnly = true;
            this.MaxUpSpeed.Visible = false;
            this.MaxUpSpeed.Width = 60;
            // 
            // Download
            // 
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Download.DefaultCellStyle = dataGridViewCellStyle10;
            this.Download.HeaderText = "Dload";
            this.Download.MinimumWidth = 100;
            this.Download.Name = "Download";
            this.Download.ReadOnly = true;
            // 
            // Upload
            // 
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Upload.DefaultCellStyle = dataGridViewCellStyle11;
            this.Upload.HeaderText = "Upload";
            this.Upload.MinimumWidth = 100;
            this.Upload.Name = "Upload";
            this.Upload.ReadOnly = true;
            // 
            // DownloadRaw
            // 
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.DownloadRaw.DefaultCellStyle = dataGridViewCellStyle12;
            this.DownloadRaw.HeaderText = "DloadRaw";
            this.DownloadRaw.MinimumWidth = 60;
            this.DownloadRaw.Name = "DownloadRaw";
            this.DownloadRaw.ReadOnly = true;
            this.DownloadRaw.Visible = false;
            this.DownloadRaw.Width = 60;
            // 
            // ErrorPercent
            // 
            this.ErrorPercent.HeaderText = "Error Percent";
            this.ErrorPercent.MinimumWidth = 60;
            this.ErrorPercent.Name = "ErrorPercent";
            this.ErrorPercent.ReadOnly = true;
            this.ErrorPercent.Visible = false;
            this.ErrorPercent.Width = 60;
            // 
            // ConnectError
            // 
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ConnectError.DefaultCellStyle = dataGridViewCellStyle13;
            this.ConnectError.HeaderText = "Error";
            this.ConnectError.MinimumWidth = 60;
            this.ConnectError.Name = "ConnectError";
            this.ConnectError.ReadOnly = true;
            this.ConnectError.Width = 60;
            // 
            // ConnectTimeout
            // 
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ConnectTimeout.DefaultCellStyle = dataGridViewCellStyle14;
            this.ConnectTimeout.HeaderText = "Timeout";
            this.ConnectTimeout.MinimumWidth = 60;
            this.ConnectTimeout.Name = "ConnectTimeout";
            this.ConnectTimeout.ReadOnly = true;
            this.ConnectTimeout.Width = 60;
            // 
            // ConnectEmpty
            // 
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ConnectEmpty.DefaultCellStyle = dataGridViewCellStyle15;
            this.ConnectEmpty.HeaderText = "Empty Response";
            this.ConnectEmpty.MinimumWidth = 60;
            this.ConnectEmpty.Name = "ConnectEmpty";
            this.ConnectEmpty.ReadOnly = true;
            this.ConnectEmpty.Visible = false;
            this.ConnectEmpty.Width = 60;
            // 
            // Continuous
            // 
            this.Continuous.HeaderText = "Continuous";
            this.Continuous.MinimumWidth = 60;
            this.Continuous.Name = "Continuous";
            this.Continuous.ReadOnly = true;
            this.Continuous.Visible = false;
            this.Continuous.Width = 60;
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.ServerDataGrid, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(784, 461);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // ServerLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ServerLogForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ServerLog";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ServerLogForm_FormClosed);
            this.Move += new System.EventHandler(this.ServerLogForm_Move);
            ((System.ComponentModel.ISupportInitialize)(this.ServerDataGrid)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DoubleBufferListView ServerDataGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Group;
        private System.Windows.Forms.DataGridViewTextBoxColumn Server;
        private System.Windows.Forms.DataGridViewTextBoxColumn TotalConnect;
        private System.Windows.Forms.DataGridViewTextBoxColumn Connecting;
        private System.Windows.Forms.DataGridViewTextBoxColumn AvgLatency;
        private System.Windows.Forms.DataGridViewTextBoxColumn AvgDownSpeed;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaxDownSpeed;
        private System.Windows.Forms.DataGridViewTextBoxColumn AvgUpSpeed;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaxUpSpeed;
        private System.Windows.Forms.DataGridViewTextBoxColumn Download;
        private System.Windows.Forms.DataGridViewTextBoxColumn Upload;
        private System.Windows.Forms.DataGridViewTextBoxColumn DownloadRaw;
        private System.Windows.Forms.DataGridViewTextBoxColumn ErrorPercent;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnectError;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnectTimeout;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnectEmpty;
        private System.Windows.Forms.DataGridViewTextBoxColumn Continuous;
    }
}