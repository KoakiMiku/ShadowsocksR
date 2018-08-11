﻿using ShadowsocksR.Controller;
using ShadowsocksR.Encryption;
using ShadowsocksR.Model;
using ShadowsocksR.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using ZXing.QrCode.Internal;

namespace ShadowsocksR.View
{
    public partial class ConfigForm : Form
    {
        private ShadowsocksController _controller;
        // this is a copy of configuration that we are working on
        private Configuration _modifiedConfiguration;
        private int _oldSelectedIndex = -1;
        private bool _ignoreLoad = false;
        private string _oldSelectedID = null;
        private string _SelectedID = null;

        public ConfigForm(ShadowsocksController controller, int focusIndex)
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;
            Icon = Icon.FromHandle(Resources.ssw128.GetHicon());
            _controller = controller;

            NumServerPort.Minimum = IPEndPoint.MinPort;
            NumServerPort.Maximum = IPEndPoint.MaxPort;
            NumUDPPort.Minimum = IPEndPoint.MinPort;
            NumUDPPort.Maximum = IPEndPoint.MaxPort;

            UpdateTexts();
            controller.ConfigChanged += controller_ConfigChanged;

            LoadCurrentConfiguration();
            if (_modifiedConfiguration.index >= 0 && _modifiedConfiguration.index < _modifiedConfiguration.configs.Count)
                _oldSelectedID = _modifiedConfiguration.configs[_modifiedConfiguration.index].id;
            if (focusIndex == -1)
            {
                int index = _modifiedConfiguration.index + 1;
                if (index < 0 || index > _modifiedConfiguration.configs.Count)
                    index = _modifiedConfiguration.configs.Count;

                focusIndex = index;
            }

            PictureQRcode.Visible = false;

            int dpi_mul = Util.Utils.GetDpiMul();
            //ServersListBox.Height = ServersListBox.Height * 4 / dpi_mul;
            ServersListBox.Width = ServersListBox.Width * dpi_mul / 4;
            //ServersListBox.Height = ServersListBox.Height * dpi_mul / 4;
            ServersListBox.Height = checkAdvSetting.Top + checkAdvSetting.Height;
            AddButton.Width = AddButton.Width * dpi_mul / 4;
            AddButton.Height = AddButton.Height * dpi_mul / 4;
            DeleteButton.Width = DeleteButton.Width * dpi_mul / 4;
            DeleteButton.Height = DeleteButton.Height * dpi_mul / 4;
            UpButton.Width = UpButton.Width * dpi_mul / 4;
            UpButton.Height = UpButton.Height * dpi_mul / 4;
            DownButton.Width = DownButton.Width * dpi_mul / 4;
            DownButton.Height = DownButton.Height * dpi_mul / 4;

            //OKButton.Width = OKButton.Width * dpi_mul / 4;
            OKButton.Height = OKButton.Height * dpi_mul / 4;
            //MyCancelButton.Width = MyCancelButton.Width * dpi_mul / 4;
            MyCancelButton.Height = MyCancelButton.Height * dpi_mul / 4;

            ShowWindow();

            if (focusIndex >= 0 && focusIndex < _modifiedConfiguration.configs.Count)
            {
                SetServerListSelectedIndex(focusIndex);
                LoadSelectedServer();
            }

            UpdateServersListBoxTopIndex();
        }

        private void UpdateTexts()
        {
            Text = I18N.GetString("Servers Setting");

            AddButton.Text = I18N.GetString("Add");
            DeleteButton.Text = I18N.GetString("Delete");
            ModifyButton.Text = I18N.GetString("Modify");
            UpButton.Text = I18N.GetString("Up");
            DownButton.Text = I18N.GetString("Down");

            IPLabel.Text = I18N.GetString("Server IP");
            ServerPortLabel.Text = I18N.GetString("Server Port");
            labelUDPPort.Text = I18N.GetString("UDP Port");
            PasswordLabel.Text = I18N.GetString("Password");
            EncryptionLabel.Text = I18N.GetString("Encryption");
            TCPProtocolLabel.Text = I18N.GetString(TCPProtocolLabel.Text);
            labelObfs.Text = I18N.GetString(labelObfs.Text);
            labelRemarks.Text = I18N.GetString("Remarks");
            labelGroup.Text = I18N.GetString("Group");

            checkAdvSetting.Text = I18N.GetString(checkAdvSetting.Text);
            TCPoverUDPLabel.Text = I18N.GetString(TCPoverUDPLabel.Text);
            UDPoverTCPLabel.Text = I18N.GetString(UDPoverTCPLabel.Text);
            labelProtocolParam.Text = I18N.GetString(labelProtocolParam.Text);
            labelObfsParam.Text = I18N.GetString(labelObfsParam.Text);
            ObfsUDPLabel.Text = I18N.GetString(ObfsUDPLabel.Text);
            LabelNote.Text = I18N.GetString(LabelNote.Text);
            CheckTCPoverUDP.Text = I18N.GetString(CheckTCPoverUDP.Text);
            CheckUDPoverUDP.Text = I18N.GetString(CheckUDPoverUDP.Text);
            CheckObfsUDP.Text = I18N.GetString(CheckObfsUDP.Text);
            checkSSRLink.Text = I18N.GetString(checkSSRLink.Text);

            for (int i = 0; i < TCPProtocolComboBox.Items.Count; ++i)
            {
                TCPProtocolComboBox.Items[i] = I18N.GetString(TCPProtocolComboBox.Items[i].ToString());
            }

            ServerGroupBox.Text = I18N.GetString("Server");

            OKButton.Text = I18N.GetString("OK");
            MyCancelButton.Text = I18N.GetString("Cancel");
        }

        private void controller_ConfigChanged(object sender, EventArgs e)
        {
            LoadCurrentConfiguration();
        }

        private void ShowWindow()
        {
            Opacity = 1;
            Show();
            IPTextBox.Focus();
        }

        private int SaveOldSelectedServer()
        {
            try
            {
                if (_oldSelectedIndex == -1 || _oldSelectedIndex >= _modifiedConfiguration.configs.Count)
                {
                    return 0; // no changes
                }
                Server server = new Server
                {
                    server = IPTextBox.Text.Trim(),
                    server_port = string.IsNullOrWhiteSpace(NumServerPort.Text) ? 0 : Convert.ToInt32(NumServerPort.Value),
                    server_udp_port = string.IsNullOrWhiteSpace(NumUDPPort.Text) ? 0 : Convert.ToInt32(NumUDPPort.Value),
                    password = PasswordTextBox.Text,
                    method = EncryptionSelect.Text,
                    protocol = TCPProtocolComboBox.Text,
                    protocolparam = TextProtocolParam.Text,
                    obfs = ObfsComboBox.Text,
                    obfsparam = TextObfsParam.Text,
                    remarks = RemarksTextBox.Text,
                    group = TextGroup.Text.Trim(),
                    udp_over_tcp = CheckUDPoverUDP.Checked,
                    //obfs_udp = CheckObfsUDP.Checked,
                    id = _SelectedID
                };
                Configuration.CheckServer(server);
                int ret = 0;
                if (_modifiedConfiguration.configs[_oldSelectedIndex].server != server.server
                    || _modifiedConfiguration.configs[_oldSelectedIndex].server_port != server.server_port
                    || _modifiedConfiguration.configs[_oldSelectedIndex].remarks_base64 != server.remarks_base64
                    || _modifiedConfiguration.configs[_oldSelectedIndex].group != server.group)
                {
                    ret = 1; // display changed
                }
                Server oldServer = _modifiedConfiguration.configs[_oldSelectedIndex];
                if (oldServer.isMatchServer(server))
                {
                    server.setObfsData(oldServer.getObfsData());
                    server.setProtocolData(oldServer.getProtocolData());
                    server.enable = oldServer.enable;
                }
                _modifiedConfiguration.configs[_oldSelectedIndex] = server;

                return ret;
            }
            catch (FormatException)
            {
                MessageBox.Show(I18N.GetString("Illegal port number format"), "ShadowsocksR",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return -1; // ERROR
        }

        private void GenQR(string ssconfig)
        {
            int dpi_mul = Util.Utils.GetDpiMul();
            int width = 350 * dpi_mul / 4;
            if (TextLink.Focused)
            {
                string qrText = ssconfig;
                QRCode code = Encoder.encode(qrText, ErrorCorrectionLevel.M);
                ByteMatrix m = code.Matrix;
                int blockSize = Math.Max(width / (m.Width + 2), 1);
                Bitmap drawArea = new Bitmap(((m.Width + 2) * blockSize), ((m.Height + 2) * blockSize));
                using (Graphics g = Graphics.FromImage(drawArea))
                {
                    g.Clear(Color.White);
                    using (Brush b = new SolidBrush(Color.Black))
                    {
                        for (int row = 0; row < m.Width; row++)
                        {
                            for (int col = 0; col < m.Height; col++)
                            {
                                if (m[row, col] != 0)
                                {
                                    g.FillRectangle(b, blockSize * (row + 1), blockSize * (col + 1),
                                        blockSize, blockSize);
                                }
                            }
                        }
                    }
                    Bitmap ngnl = Resources.ngnl;
                    int div = 13, div_l = 5, div_r = 8;
                    int l = (m.Width * div_l + div - 1) / div * blockSize, r = (m.Width * div_r + div - 1) / div * blockSize;
                    g.DrawImage(ngnl, new Rectangle(l + blockSize, l + blockSize, r - l, r - l));
                }
                PictureQRcode.Image = drawArea;
                PictureQRcode.Visible = true;
            }
            else
            {
                PictureQRcode.Visible = false;
            }
        }

        private void LoadSelectedServer()
        {
            if (ServersListBox.SelectedIndex >= 0 && ServersListBox.SelectedIndex < _modifiedConfiguration.configs.Count)
            {
                Server server = _modifiedConfiguration.configs[ServersListBox.SelectedIndex];

                IPTextBox.Text = server.server;
                NumServerPort.Value = server.server_port;
                NumUDPPort.Value = server.server_udp_port;
                PasswordTextBox.Text = server.password;
                EncryptionSelect.Text = server.method ?? "none";
                TCPProtocolComboBox.Text = server.protocol ?? "origin";
                ObfsComboBox.Text = server.obfs ?? "plain";
                TextProtocolParam.Text = server.protocolparam;
                TextObfsParam.Text = server.obfsparam;
                RemarksTextBox.Text = server.remarks;
                TextGroup.Text = server.group;
                CheckUDPoverUDP.Checked = server.udp_over_tcp;
                //CheckObfsUDP.Checked = server.obfs_udp;
                _SelectedID = server.id;

                if (TCPProtocolComboBox.Text == "origin" && ObfsComboBox.Text == "plain" && !CheckUDPoverUDP.Checked)
                {
                    checkAdvSetting.Checked = false;
                }

                if (checkSSRLink.Checked)
                {
                    TextLink.Text = server.GetSSRLinkForServer();
                }
                else
                {
                    TextLink.Text = server.GetSSLinkForServer();
                }

                if (CheckTCPoverUDP.Checked || CheckUDPoverUDP.Checked || server.server_udp_port != 0)
                {
                    checkAdvSetting.Checked = true;
                }

                Update_SSR_controls_Visable();
                UpdateObfsTextbox();
                TextLink.SelectAll();
                GenQR(TextLink.Text);
            }
        }

        private void LoadDefaultServer()
        {
            if (_modifiedConfiguration.configs.Count == 0)
            {
                Server server = Configuration.GetDefaultServer();

                IPTextBox.Text = server.server;
                NumServerPort.Value = server.server_port;
                NumUDPPort.Value = server.server_udp_port;
                PasswordTextBox.Text = server.password;
                EncryptionSelect.Text = server.method;
                TCPProtocolComboBox.Text = server.protocol;
                ObfsComboBox.Text = server.obfs;
                TextProtocolParam.Text = server.protocolparam;
                TextObfsParam.Text = server.obfsparam;
                RemarksTextBox.Text = server.remarks;
                TextGroup.Text = server.group;
                CheckUDPoverUDP.Checked = server.udp_over_tcp;
                //CheckObfsUDP.Checked = server.obfs_udp;
                _SelectedID = server.id;

                if (TCPProtocolComboBox.Text == "origin" && ObfsComboBox.Text == "plain" && !CheckUDPoverUDP.Checked)
                {
                    checkAdvSetting.Checked = false;
                }

                if (checkSSRLink.Checked)
                {
                    TextLink.Text = server.GetSSRLinkForServer();
                }
                else
                {
                    TextLink.Text = server.GetSSLinkForServer();
                }

                if (CheckTCPoverUDP.Checked || CheckUDPoverUDP.Checked || server.server_udp_port != 0)
                {
                    checkAdvSetting.Checked = true;
                }

                Update_SSR_controls_Visable();
                UpdateObfsTextbox();
                TextLink.SelectAll();
                GenQR(TextLink.Text);
            }
        }

        private void LoadConfiguration(Configuration configuration)
        {
            if (ServersListBox.Items.Count != _modifiedConfiguration.configs.Count)
            {
                ServersListBox.Items.Clear();
                foreach (Server server in _modifiedConfiguration.configs)
                {
                    if (!string.IsNullOrEmpty(server.group))
                    {
                        ServersListBox.Items.Add(server.group + " - " + server.HiddenName());
                    }
                    else
                    {
                        ServersListBox.Items.Add(I18N.GetString("(empty group)") + " - " + server.HiddenName());
                    }
                }
            }
            else
            {
                for (int i = 0; i < _modifiedConfiguration.configs.Count; ++i)
                {
                    if (!string.IsNullOrEmpty(_modifiedConfiguration.configs[i].group))
                    {
                        ServersListBox.Items[i] = _modifiedConfiguration.configs[i].group + " - " + _modifiedConfiguration.configs[i].HiddenName();
                    }
                    else
                    {
                        ServersListBox.Items[i] = I18N.GetString("(empty group)") + " - " + _modifiedConfiguration.configs[i].HiddenName();
                    }
                }
            }
        }

        public void SetServerListSelectedIndex(int index)
        {
            ServersListBox.ClearSelected();
            if (index < ServersListBox.Items.Count)
                ServersListBox.SelectedIndex = index;
            else
                _oldSelectedIndex = ServersListBox.SelectedIndex;
        }

        private void LoadCurrentConfiguration()
        {
            _modifiedConfiguration = _controller.GetConfiguration();
            LoadConfiguration(_modifiedConfiguration);
            SetServerListSelectedIndex(_modifiedConfiguration.index);
            LoadSelectedServer();
            if (ServersListBox.Items.Count == 0)
            {
                LoadDefaultServer();
                ModifyButton.Enabled = false;
                DeleteButton.Enabled = false;
                UpButton.Enabled = false;
                DownButton.Enabled = false;
            }
        }

        private void ServersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_oldSelectedIndex == ServersListBox.SelectedIndex || ServersListBox.SelectedIndex == -1)
            {
                // we are moving back to oldSelectedIndex or doing a force move
                return;
            }
            if (!_ignoreLoad) LoadSelectedServer();
            _oldSelectedIndex = ServersListBox.SelectedIndex;
        }

        private void UpdateServersListBoxTopIndex(int style = 0)
        {
            int visibleItems = ServersListBox.ClientSize.Height / ServersListBox.ItemHeight;
            int index;
            if (style == 0)
            {
                index = ServersListBox.SelectedIndex;
            }
            else
            {
                var items = ServersListBox.SelectedIndices;
                index = (style == 1 ? items[0] : items[items.Count - 1]);
            }
            int topIndex = Math.Max(index - visibleItems / 2, 0);
            ServersListBox.TopIndex = topIndex;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            Server server = new Server
            {
                server = IPTextBox.Text.Trim(),
                server_port = Convert.ToInt32(NumServerPort.Value),
                server_udp_port = Convert.ToInt32(NumUDPPort.Value),
                password = PasswordTextBox.Text,
                method = EncryptionSelect.Text,
                protocol = TCPProtocolComboBox.Text,
                protocolparam = TextProtocolParam.Text,
                obfs = ObfsComboBox.Text,
                obfsparam = TextObfsParam.Text,
                remarks = RemarksTextBox.Text,
                group = TextGroup.Text.Trim(),
                udp_over_tcp = CheckUDPoverUDP.Checked,
                //obfs_udp = CheckObfsUDP.Checked,
                id = _SelectedID
            };
            Configuration.CheckServer(server);

            if (server == null) return;
            _modifiedConfiguration.configs.Insert(_oldSelectedIndex < 0 ? 0 : _oldSelectedIndex + 1, server);
            LoadConfiguration(_modifiedConfiguration);
            _SelectedID = server.id;
            ServersListBox.SelectedIndex = _oldSelectedIndex + 1;
            _oldSelectedIndex = ServersListBox.SelectedIndex;

            if (ServersListBox.Items.Count != 0)
            {
                ModifyButton.Enabled = true;
                DeleteButton.Enabled = true;
                UpButton.Enabled = true;
                DownButton.Enabled = true;
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            _oldSelectedIndex = ServersListBox.SelectedIndex;
            var items = ServersListBox.SelectedIndices;
            if (items.Count > 0)
            {
                int[] array = new int[items.Count];
                int i = 0;
                foreach (int index in items)
                {
                    array[i++] = index;
                }
                Array.Sort(array);
                for (--i; i >= 0; --i)
                {
                    int index = array[i];
                    if (index >= 0 && index < _modifiedConfiguration.configs.Count)
                    {
                        _modifiedConfiguration.configs.RemoveAt(index);
                    }
                }
            }
            if (_oldSelectedIndex >= _modifiedConfiguration.configs.Count)
            {
                _oldSelectedIndex = _modifiedConfiguration.configs.Count - 1;
            }
            if (_oldSelectedIndex < 0)
            {
                _oldSelectedIndex = 0;
            }
            ServersListBox.SelectedIndex = _oldSelectedIndex;
            LoadConfiguration(_modifiedConfiguration);
            SetServerListSelectedIndex(_oldSelectedIndex);
            LoadSelectedServer();
            UpdateServersListBoxTopIndex();

            if (ServersListBox.Items.Count == 0)
            {
                PictureQRcode.Visible = false;
                ModifyButton.Enabled = false;
                DeleteButton.Enabled = false;
                UpButton.Enabled = false;
                DownButton.Enabled = false;
            }
        }

        private void ModifyButton_Click(object sender, EventArgs e)
        {
            if (SaveOldSelectedServer() == -1)
            {
                return;
            }
            LoadConfiguration(_modifiedConfiguration);
            LoadSelectedServer();

            if (ServersListBox.Items.Count != 0)
            {
                ModifyButton.Enabled = true;
                DeleteButton.Enabled = true;
                UpButton.Enabled = true;
                DownButton.Enabled = true;
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (SaveOldSelectedServer() == -1)
            {
                return;
            }
            if (_oldSelectedID != null)
            {
                for (int i = 0; i < _modifiedConfiguration.configs.Count; ++i)
                {
                    if (_modifiedConfiguration.configs[i].id == _oldSelectedID)
                    {
                        _modifiedConfiguration.index = i;
                        break;
                    }
                }
            }
            _controller.SaveServersConfig(_modifiedConfiguration);
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ConfigForm_Shown(object sender, EventArgs e)
        {
            IPTextBox.Focus();
        }

        private void ConfigForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _controller.ConfigChanged -= controller_ConfigChanged;
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            _oldSelectedIndex = ServersListBox.SelectedIndex;
            int index = _oldSelectedIndex;
            SaveOldSelectedServer();
            var items = ServersListBox.SelectedIndices;
            if (items.Count == 1)
            {
                if (index > 0 && index < _modifiedConfiguration.configs.Count)
                {
                    _modifiedConfiguration.configs.Reverse(index - 1, 2);
                    ServersListBox.ClearSelected();
                    ServersListBox.SelectedIndex = _oldSelectedIndex = index - 1;
                    LoadConfiguration(_modifiedConfiguration);
                    ServersListBox.ClearSelected();
                    ServersListBox.SelectedIndex = _oldSelectedIndex = index - 1;
                    LoadSelectedServer();
                }
            }
            else
            {
                List<int> all_items = new List<int>();
                foreach (int item in items)
                {
                    if (item == 0)
                        return;
                    all_items.Add(item);
                }
                foreach (int item in all_items)
                {
                    _modifiedConfiguration.configs.Reverse(item - 1, 2);
                }
                _ignoreLoad = true;
                ServersListBox.SelectedIndex = _oldSelectedIndex = index - 1;
                LoadConfiguration(_modifiedConfiguration);
                ServersListBox.ClearSelected();
                foreach (int item in all_items)
                {
                    if (item != index)
                        ServersListBox.SelectedIndex = _oldSelectedIndex = item - 1;
                }
                ServersListBox.SelectedIndex = _oldSelectedIndex = index - 1;
                _ignoreLoad = false;
                LoadSelectedServer();
            }
            UpdateServersListBoxTopIndex(1);
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            _oldSelectedIndex = ServersListBox.SelectedIndex;
            int index = _oldSelectedIndex;
            SaveOldSelectedServer();
            var items = ServersListBox.SelectedIndices;
            if (items.Count == 1)
            {
                if (_oldSelectedIndex >= 0 && _oldSelectedIndex < _modifiedConfiguration.configs.Count - 1)
                {
                    _modifiedConfiguration.configs.Reverse(index, 2);
                    ServersListBox.ClearSelected();
                    ServersListBox.SelectedIndex = _oldSelectedIndex = index + 1;
                    LoadConfiguration(_modifiedConfiguration);
                    ServersListBox.ClearSelected();
                    ServersListBox.SelectedIndex = _oldSelectedIndex = index + 1;
                    LoadSelectedServer();
                }
            }
            else
            {
                List<int> rev_items = new List<int>();
                int max_index = ServersListBox.Items.Count - 1;
                foreach (int item in items)
                {
                    if (item == max_index)
                        return;
                    rev_items.Insert(0, item);
                }
                foreach (int item in rev_items)
                {
                    _modifiedConfiguration.configs.Reverse(item, 2);
                }
                _ignoreLoad = true;
                ServersListBox.SelectedIndex = _oldSelectedIndex = index + 1;
                LoadConfiguration(_modifiedConfiguration);
                ServersListBox.ClearSelected();
                foreach (int item in rev_items)
                {
                    if (item != index)
                        ServersListBox.SelectedIndex = _oldSelectedIndex = item + 1;
                }
                ServersListBox.SelectedIndex = _oldSelectedIndex = index + 1;
                _ignoreLoad = false;
                LoadSelectedServer();
            }
            UpdateServersListBoxTopIndex(2);
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            int change = SaveOldSelectedServer();
            if (change == 1)
            {
                LoadConfiguration(_modifiedConfiguration);
            }
            LoadSelectedServer();
            ((TextBox)sender).SelectAll();
        }

        private void TextBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ((TextBox)sender).SelectAll();
            }
        }

        private void PasswordLabel_CheckedChanged(object sender, EventArgs e)
        {
            if (PasswordLabel.Checked)
            {
                PasswordTextBox.UseSystemPasswordChar = false;
            }
            else
            {
                PasswordTextBox.UseSystemPasswordChar = true;
            }
        }

        private void UpdateProtocolTextbox()
        {
            try
            {
                Obfs.ObfsBase obfs = (Obfs.ObfsBase)Obfs.ObfsFactory.GetObfs(TCPProtocolComboBox.Text);
                int[] properties = obfs.GetObfs()[TCPProtocolComboBox.Text];
                if (properties[2] > 0)
                {
                    TextProtocolParam.Enabled = true;
                }
                else
                {
                    TextProtocolParam.Enabled = false;
                }
            }
            catch
            {
                TextProtocolParam.Enabled = false;
            }
        }

        private void UpdateObfsTextbox()
        {
            try
            {
                Obfs.ObfsBase obfs = (Obfs.ObfsBase)Obfs.ObfsFactory.GetObfs(ObfsComboBox.Text);
                int[] properties = obfs.GetObfs()[ObfsComboBox.Text];
                if (properties[2] > 0)
                {
                    TextObfsParam.Enabled = true;
                }
                else
                {
                    TextObfsParam.Enabled = false;
                }
            }
            catch
            {
                TextObfsParam.Enabled = false;
            }
        }

        private void ProtocolCombo_TextChanged(object sender, EventArgs e)
        {
            UpdateProtocolTextbox();
        }

        private void ObfsCombo_TextChanged(object sender, EventArgs e)
        {
            UpdateObfsTextbox();
        }

        private void checkSSRLink_CheckedChanged(object sender, EventArgs e)
        {
            int change = SaveOldSelectedServer();
            if (change == 1)
            {
                LoadConfiguration(_modifiedConfiguration);
            }
            LoadSelectedServer();
        }

        private void checkAdvSetting_CheckedChanged(object sender, EventArgs e)
        {
            Update_SSR_controls_Visable();
        }

        private void Update_SSR_controls_Visable()
        {
            SuspendLayout();
            if (checkAdvSetting.Checked)
            {
                labelUDPPort.Visible = true;
                NumUDPPort.Visible = true;
                //TCPoverUDPLabel.Visible = true;
                //CheckTCPoverUDP.Visible = true;
            }
            else
            {
                labelUDPPort.Visible = false;
                NumUDPPort.Visible = false;
                //TCPoverUDPLabel.Visible = false;
                //CheckTCPoverUDP.Visible = false;
            }
            if (checkAdvSetting.Checked)
            {
                UDPoverTCPLabel.Visible = true;
                CheckUDPoverUDP.Visible = true;
            }
            else
            {
                UDPoverTCPLabel.Visible = false;
                CheckUDPoverUDP.Visible = false;
            }
            ResumeLayout();
        }

        private void IPLabel_CheckedChanged(object sender, EventArgs e)
        {
            if (IPLabel.Checked)
            {
                IPTextBox.UseSystemPasswordChar = false;
            }
            else
            {
                IPTextBox.UseSystemPasswordChar = true;
            }
        }
    }
}
