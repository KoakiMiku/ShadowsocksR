namespace ShadowsocksR.View
{
    partial class SettingsForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.MyCancelButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.ReconnectLabel = new System.Windows.Forms.Label();
            this.NumReconnect = new System.Windows.Forms.NumericUpDown();
            this.TTLLabel = new System.Windows.Forms.Label();
            this.NumTTL = new System.Windows.Forms.NumericUpDown();
            this.labelTimeout = new System.Windows.Forms.Label();
            this.NumTimeout = new System.Windows.Forms.NumericUpDown();
            this.checkAutoStartup = new System.Windows.Forms.CheckBox();
            this.ListenGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.TextAuthPass = new System.Windows.Forms.TextBox();
            this.LabelAuthPass = new System.Windows.Forms.Label();
            this.TextAuthUser = new System.Windows.Forms.TextBox();
            this.LabelAuthUser = new System.Windows.Forms.Label();
            this.checkShareOverLan = new System.Windows.Forms.CheckBox();
            this.NumProxyPort = new System.Windows.Forms.NumericUpDown();
            this.ProxyPortLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumReconnect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumTTL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumTimeout)).BeginInit();
            this.ListenGroup.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumProxyPort)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.ListenGroup, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 13);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(560, 335);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.MyCancelButton, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.OKButton, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(316, 138);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(200, 25);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // MyCancelButton
            // 
            this.MyCancelButton.AutoSize = true;
            this.MyCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.MyCancelButton.Location = new System.Drawing.Point(103, 3);
            this.MyCancelButton.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            this.MyCancelButton.Name = "MyCancelButton";
            this.MyCancelButton.Size = new System.Drawing.Size(97, 22);
            this.MyCancelButton.TabIndex = 22;
            this.MyCancelButton.Text = "Cancel";
            this.MyCancelButton.UseVisualStyleBackColor = true;
            this.MyCancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.AutoSize = true;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(3, 3);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(94, 22);
            this.OKButton.TabIndex = 21;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.tableLayoutPanel5);
            this.groupBox1.Location = new System.Drawing.Point(316, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(192, 129);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.AutoSize = true;
            this.tableLayoutPanel5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.Controls.Add(this.ReconnectLabel, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.NumReconnect, 1, 1);
            this.tableLayoutPanel5.Controls.Add(this.TTLLabel, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.NumTTL, 1, 3);
            this.tableLayoutPanel5.Controls.Add(this.labelTimeout, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.NumTimeout, 1, 2);
            this.tableLayoutPanel5.Controls.Add(this.checkAutoStartup, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel5.RowCount = 4;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(186, 109);
            this.tableLayoutPanel5.TabIndex = 3;
            // 
            // ReconnectLabel
            // 
            this.ReconnectLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.ReconnectLabel.AutoSize = true;
            this.ReconnectLabel.Location = new System.Drawing.Point(6, 32);
            this.ReconnectLabel.Name = "ReconnectLabel";
            this.ReconnectLabel.Size = new System.Drawing.Size(59, 12);
            this.ReconnectLabel.TabIndex = 3;
            this.ReconnectLabel.Text = "Reconnect";
            // 
            // NumReconnect
            // 
            this.NumReconnect.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.NumReconnect.Location = new System.Drawing.Point(71, 28);
            this.NumReconnect.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.NumReconnect.Name = "NumReconnect";
            this.NumReconnect.Size = new System.Drawing.Size(109, 21);
            this.NumReconnect.TabIndex = 18;
            // 
            // TTLLabel
            // 
            this.TTLLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.TTLLabel.AutoSize = true;
            this.TTLLabel.Location = new System.Drawing.Point(42, 86);
            this.TTLLabel.Name = "TTLLabel";
            this.TTLLabel.Size = new System.Drawing.Size(23, 12);
            this.TTLLabel.TabIndex = 3;
            this.TTLLabel.Text = "TTL";
            // 
            // NumTTL
            // 
            this.NumTTL.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.NumTTL.Location = new System.Drawing.Point(71, 82);
            this.NumTTL.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.NumTTL.Name = "NumTTL";
            this.NumTTL.Size = new System.Drawing.Size(109, 21);
            this.NumTTL.TabIndex = 20;
            // 
            // labelTimeout
            // 
            this.labelTimeout.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelTimeout.AutoSize = true;
            this.labelTimeout.Location = new System.Drawing.Point(12, 59);
            this.labelTimeout.Name = "labelTimeout";
            this.labelTimeout.Size = new System.Drawing.Size(53, 12);
            this.labelTimeout.TabIndex = 3;
            this.labelTimeout.Text = " Timeout";
            // 
            // NumTimeout
            // 
            this.NumTimeout.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.NumTimeout.Location = new System.Drawing.Point(71, 55);
            this.NumTimeout.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.NumTimeout.Name = "NumTimeout";
            this.NumTimeout.Size = new System.Drawing.Size(109, 21);
            this.NumTimeout.TabIndex = 19;
            // 
            // checkAutoStartup
            // 
            this.checkAutoStartup.AutoSize = true;
            this.checkAutoStartup.Location = new System.Drawing.Point(71, 6);
            this.checkAutoStartup.Name = "checkAutoStartup";
            this.checkAutoStartup.Size = new System.Drawing.Size(102, 16);
            this.checkAutoStartup.TabIndex = 12;
            this.checkAutoStartup.Text = "Start on Boot";
            this.checkAutoStartup.UseVisualStyleBackColor = true;
            // 
            // ListenGroup
            // 
            this.ListenGroup.AutoSize = true;
            this.ListenGroup.Controls.Add(this.tableLayoutPanel4);
            this.ListenGroup.Location = new System.Drawing.Point(3, 3);
            this.ListenGroup.Name = "ListenGroup";
            this.ListenGroup.Size = new System.Drawing.Size(307, 123);
            this.ListenGroup.TabIndex = 1;
            this.ListenGroup.TabStop = false;
            this.ListenGroup.Text = "Local proxy";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.TextAuthPass, 1, 3);
            this.tableLayoutPanel4.Controls.Add(this.LabelAuthPass, 0, 3);
            this.tableLayoutPanel4.Controls.Add(this.TextAuthUser, 1, 2);
            this.tableLayoutPanel4.Controls.Add(this.LabelAuthUser, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.checkShareOverLan, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.NumProxyPort, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.ProxyPortLabel, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 4;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(301, 103);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // TextAuthPass
            // 
            this.TextAuthPass.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.TextAuthPass.Location = new System.Drawing.Point(62, 79);
            this.TextAuthPass.Name = "TextAuthPass";
            this.TextAuthPass.Size = new System.Drawing.Size(236, 21);
            this.TextAuthPass.TabIndex = 11;
            // 
            // LabelAuthPass
            // 
            this.LabelAuthPass.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.LabelAuthPass.AutoSize = true;
            this.LabelAuthPass.Location = new System.Drawing.Point(3, 83);
            this.LabelAuthPass.Name = "LabelAuthPass";
            this.LabelAuthPass.Size = new System.Drawing.Size(53, 12);
            this.LabelAuthPass.TabIndex = 8;
            this.LabelAuthPass.Text = "Password";
            // 
            // TextAuthUser
            // 
            this.TextAuthUser.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.TextAuthUser.Location = new System.Drawing.Point(62, 52);
            this.TextAuthUser.Name = "TextAuthUser";
            this.TextAuthUser.Size = new System.Drawing.Size(236, 21);
            this.TextAuthUser.TabIndex = 10;
            // 
            // LabelAuthUser
            // 
            this.LabelAuthUser.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.LabelAuthUser.AutoSize = true;
            this.LabelAuthUser.Location = new System.Drawing.Point(3, 56);
            this.LabelAuthUser.Name = "LabelAuthUser";
            this.LabelAuthUser.Size = new System.Drawing.Size(53, 12);
            this.LabelAuthUser.TabIndex = 5;
            this.LabelAuthUser.Text = "Username";
            // 
            // checkShareOverLan
            // 
            this.checkShareOverLan.AutoSize = true;
            this.checkShareOverLan.Location = new System.Drawing.Point(62, 3);
            this.checkShareOverLan.Name = "checkShareOverLan";
            this.checkShareOverLan.Size = new System.Drawing.Size(156, 16);
            this.checkShareOverLan.TabIndex = 8;
            this.checkShareOverLan.Text = "Allow Clients from LAN";
            this.checkShareOverLan.UseVisualStyleBackColor = true;
            // 
            // NumProxyPort
            // 
            this.NumProxyPort.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.NumProxyPort.Location = new System.Drawing.Point(62, 25);
            this.NumProxyPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NumProxyPort.Name = "NumProxyPort";
            this.NumProxyPort.Size = new System.Drawing.Size(236, 21);
            this.NumProxyPort.TabIndex = 9;
            // 
            // ProxyPortLabel
            // 
            this.ProxyPortLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.ProxyPortLabel.AutoSize = true;
            this.ProxyPortLabel.Location = new System.Drawing.Point(27, 29);
            this.ProxyPortLabel.Name = "ProxyPortLabel";
            this.ProxyPortLabel.Size = new System.Drawing.Size(29, 12);
            this.ProxyPortLabel.TabIndex = 3;
            this.ProxyPortLabel.Text = "Port";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Padding = new System.Windows.Forms.Padding(12, 13, 12, 13);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SettingsForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingsForm_FormClosed);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumReconnect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumTTL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumTimeout)).EndInit();
            this.ListenGroup.ResumeLayout(false);
            this.ListenGroup.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumProxyPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button MyCancelButton;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.NumericUpDown NumProxyPort;
        private System.Windows.Forms.Label ProxyPortLabel;
        private System.Windows.Forms.CheckBox checkAutoStartup;
        private System.Windows.Forms.CheckBox checkShareOverLan;
        private System.Windows.Forms.GroupBox ListenGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TextBox TextAuthPass;
        private System.Windows.Forms.Label LabelAuthPass;
        private System.Windows.Forms.TextBox TextAuthUser;
        private System.Windows.Forms.Label LabelAuthUser;
        private System.Windows.Forms.Label labelTimeout;
        private System.Windows.Forms.NumericUpDown NumTimeout;
        private System.Windows.Forms.Label ReconnectLabel;
        private System.Windows.Forms.NumericUpDown NumReconnect;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label TTLLabel;
        private System.Windows.Forms.NumericUpDown NumTTL;
    }
}