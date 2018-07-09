﻿using ShadowsocksR.Controller;
using ShadowsocksR.Model;
using ShadowsocksR.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShadowsocksR.View
{
    public partial class SettingsForm : Form
    {
        private ShadowsocksController controller;
        // this is a copy of configuration that we are working on
        private Configuration _modifiedConfiguration;

        public SettingsForm(ShadowsocksController controller)
        {
            Font = SystemFonts.MessageBoxFont;
            InitializeComponent();

            Icon = Icon.FromHandle(Resources.ssw128.GetHicon());
            this.controller = controller;

            UpdateTexts();
            controller.ConfigChanged += controller_ConfigChanged;

            int dpi_mul = Util.Utils.GetDpiMul();

            NumProxyPort.Width = NumProxyPort.Width * dpi_mul / 4;
            TextAuthUser.Width = TextAuthUser.Width * dpi_mul / 4;
            TextAuthPass.Width = TextAuthPass.Width * dpi_mul / 4;

            DNSText.Width = DNSText.Width * dpi_mul / 4;
            NumReconnect.Width = NumReconnect.Width * dpi_mul / 4;
            NumTimeout.Width = NumTimeout.Width * dpi_mul / 4;
            NumTTL.Width = NumTTL.Width * dpi_mul / 4;

            LoadCurrentConfiguration();
        }
        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            controller.ConfigChanged -= controller_ConfigChanged;
        }

        private void UpdateTexts()
        {
            Text = I18N.GetString("Global Settings");

            ListenGroup.Text = I18N.GetString(ListenGroup.Text);
            checkShareOverLan.Text = I18N.GetString(checkShareOverLan.Text);
            ProxyPortLabel.Text = I18N.GetString("Proxy Port");
            ReconnectLabel.Text = I18N.GetString("Reconnect Times");
            TTLLabel.Text = I18N.GetString("TTL");
            labelTimeout.Text = I18N.GetString("Connect Timeout");

            checkAutoStartup.Text = I18N.GetString(checkAutoStartup.Text);

            LabelAuthUser.Text = I18N.GetString("Username");
            LabelAuthPass.Text = I18N.GetString("Password");

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
        }

        private int SaveOldSelectedServer()
        {
            try
            {
                int localPort = int.Parse(NumProxyPort.Text);
                Configuration.CheckPort(localPort);
                int ret = 0;
                _modifiedConfiguration.shareOverLan = checkShareOverLan.Checked;
                _modifiedConfiguration.localPort = localPort;
                _modifiedConfiguration.reconnectTimes = NumReconnect.Text.Length == 0 ? 0 : int.Parse(NumReconnect.Text);

                if (checkAutoStartup.Checked != AutoStartup.Check() && !AutoStartup.Set(checkAutoStartup.Checked))
                {
                    MessageBox.Show(I18N.GetString("Failed to update registry"));
                }
                _modifiedConfiguration.TTL = Convert.ToInt32(NumTTL.Value);
                _modifiedConfiguration.connectTimeout = Convert.ToInt32(NumTimeout.Value);
                _modifiedConfiguration.authUser = TextAuthUser.Text;
                _modifiedConfiguration.authPass = TextAuthPass.Text;
                _modifiedConfiguration.dnsServer = DNSText.Text;

                return ret;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return -1; // ERROR
        }

        private void LoadSelectedServer()
        {
            checkShareOverLan.Checked = _modifiedConfiguration.shareOverLan;
            NumProxyPort.Value = _modifiedConfiguration.localPort;
            NumReconnect.Value = _modifiedConfiguration.reconnectTimes;

            checkAutoStartup.Checked = AutoStartup.Check();
            NumTTL.Value = _modifiedConfiguration.TTL;
            NumTimeout.Value = _modifiedConfiguration.connectTimeout;
            DNSText.Text = _modifiedConfiguration.dnsServer;

            TextAuthUser.Text = _modifiedConfiguration.authUser;
            TextAuthPass.Text = _modifiedConfiguration.authPass;
        }

        private void LoadCurrentConfiguration()
        {
            _modifiedConfiguration = controller.GetConfiguration();
            LoadSelectedServer();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (SaveOldSelectedServer() == -1)
            {
                return;
            }
            controller.SaveServersConfig(_modifiedConfiguration);
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
