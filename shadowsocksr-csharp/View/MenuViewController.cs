﻿using ShadowsocksR.Controller;
using ShadowsocksR.Model;
using ShadowsocksR.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace ShadowsocksR.View
{
    public class MenuViewController
    {
        // yes this is just a menu view controller
        // when config form is closed, it moves away from RAM
        // and it should just do anything related to the config form

        private ShadowsocksController controller;
        private UpdateNode updateNodeChecker;
        private UpdateSubscribeManager updateSubscribeManager;
        private UpdateChinaIP updateChinaIPChecker;
        private UpdateChinaIPManager updateChinaIPManager;
        private NotifyIcon _notifyIcon;
        private ContextMenu contextMenu1;
        private MenuItem enableItem;
        private MenuItem globalModeItem;
        private MenuItem modeItem;
        private MenuItem ruleBypassLan;
        private MenuItem ruleBypassChina;
        private MenuItem ruleBypassNotChina;
        private MenuItem ruleDisableBypass;
        private MenuItem SeperatorItem;
        private MenuItem ServersItem;
        private ConfigForm configForm;
        private SettingsForm settingsForm;
        private ServerLogForm serverLogForm;
        private SubscribeForm subScribeForm;
        private string _urlToOpen;
        private System.Timers.Timer timerDelayCheckUpdate;

        public MenuViewController(ShadowsocksController controller)
        {
            this.controller = controller;

            LoadMenu();

            controller.ToggleModeChanged += controller_ToggleModeChanged;
            controller.ToggleRuleModeChanged += controller_ToggleRuleModeChanged;
            controller.ConfigChanged += controller_ConfigChanged;
            controller.Errored += controller_Errored;

            _notifyIcon = new NotifyIcon();
            UpdateTrayIcon();
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenu = contextMenu1;
            _notifyIcon.MouseClick += notifyIcon1_Click;

            updateNodeChecker = new UpdateNode();
            updateNodeChecker.NewNodeFound += updateNodeChecker_NewNodeFound;
            updateSubscribeManager = new UpdateSubscribeManager();

            updateChinaIPChecker = new UpdateChinaIP();
            updateChinaIPChecker.NewChinaIPFound += updateChinaIPChecker_NewChinaIPFound;
            updateChinaIPManager = new UpdateChinaIPManager();

            LoadCurrentConfiguration();

            Configuration config = controller.GetCurrentConfiguration();
            if (config.nodeFeedAutoUpdate)
            {
                updateSubscribeManager.CreateTask(config, updateNodeChecker);
            }

            timerDelayCheckUpdate = new System.Timers.Timer(1000.0 * 10);
            timerDelayCheckUpdate.Elapsed += timer_Elapsed;
            timerDelayCheckUpdate.Start();
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (timerDelayCheckUpdate != null)
            {
                if (timerDelayCheckUpdate.Interval <= 1000.0 * 30)
                {
                    timerDelayCheckUpdate.Interval = 1000.0 * 60 * 5;
                }
                else
                {
                    timerDelayCheckUpdate.Interval = 1000.0 * 60 * 60 * 2;
                }
            }
        }

        void controller_Errored(object sender, System.IO.ErrorEventArgs e)
        {
            MessageBox.Show(e.GetException().ToString(),
                String.Format(I18N.GetString("ShadowsocksR Error: {0}"), e.GetException().Message),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateTrayIcon()
        {
            int dpi;
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpi = (int)graphics.DpiX;
            }
            Bitmap icon;
            if (dpi < 97)
            {
                // dpi = 96;
                icon = Resources.ss16;
            }
            else if (dpi < 121)
            {
                // dpi = 120;
                icon = Resources.ss20;
            }
            else
            {
                icon = Resources.ss24;
            }

            Configuration config = controller.GetCurrentConfiguration();
            bool enabled = config.sysProxyMode != (int)ProxyMode.Direct;
            bool bypass = config.proxyRuleMode != (int)ProxyRuleMode.Disable;

            double mul_r;
            double mul_g;
            double mul_b;
            if (enabled && !bypass)
            {
                mul_r = 1.0;
                mul_g = 0.0;
                mul_b = 0.5;
            }
            else if (enabled && bypass)
            {
                mul_r = 0.0;
                mul_g = 1.0;
                mul_b = 1.0;
            }
            else
            {
                mul_r = 1.0;
                mul_g = 1.0;
                mul_b = 1.0;
            }

            using (Bitmap iconCopy = new Bitmap(icon))
            {
                for (int x = 0; x < iconCopy.Width; x++)
                {
                    for (int y = 0; y < iconCopy.Height; y++)
                    {
                        Color color = icon.GetPixel(x, y);
                        iconCopy.SetPixel(x, y,
                            Color.FromArgb(color.A,
                            ((byte)(color.R * mul_r)),
                            ((byte)(color.G * mul_g)),
                            ((byte)(color.B * mul_b))));
                    }
                }
                _notifyIcon.Icon = Icon.FromHandle(iconCopy.GetHicon());
            }

            string text;
            try
            {
                if (enabled)
                {
                    if (!string.IsNullOrEmpty(config.configs[config.index].remarks))
                    {
                        text = config.configs[config.index].remarks;
                    }
                    else
                    {
                        text = config.configs[config.index].FriendlyName();
                    }
                }
                else
                {
                    text = String.Format(I18N.GetString("Running: Port {0}"), config.localPort);
                }
            }
            catch
            {
                text = String.Format(I18N.GetString("Running: Port {0}"), config.localPort);
                controller.ToggleMode(ProxyMode.Direct);
            }

            // we want to show more details but notify icon title is limited to 63 characters
            _notifyIcon.Text = text.Substring(0, Math.Min(63, text.Length));
        }

        private MenuItem CreateMenuItem(string text, EventHandler click)
        {
            return new MenuItem(I18N.GetString(text), click);
        }

        private MenuItem CreateMenuGroup(string text, MenuItem[] items)
        {
            return new MenuItem(I18N.GetString(text), items);
        }

        private void LoadMenu()
        {
            contextMenu1 = new ContextMenu(new MenuItem[] {
                modeItem = CreateMenuGroup("Mode", new MenuItem[] {
                    enableItem = CreateMenuItem("Disable system proxy", new EventHandler(EnableItem_Click)),
                    new MenuItem("-"),
                    globalModeItem = CreateMenuItem("Enable system proxy", new EventHandler(GlobalModeItem_Click)),
                }),
                CreateMenuGroup("Proxy rule", new MenuItem[] {
                    ruleDisableBypass = CreateMenuItem("Disable bypass", new EventHandler(RuleBypassDisableItem_Click)),
                    new MenuItem("-"),
                    ruleBypassLan = CreateMenuItem("Bypass LAN", new EventHandler(RuleBypassLanItem_Click)),
                    ruleBypassChina = CreateMenuItem("Bypass LAN and China", new EventHandler(RuleBypassChinaItem_Click)),
                    ruleBypassNotChina = CreateMenuItem("Bypass LAN and not China", new EventHandler(RuleBypassNotChinaItem_Click)),
                    new MenuItem("-"),
                    CreateMenuItem("Update China IP", new EventHandler(UpdateChnIpItem_Click)),
                }),
                new MenuItem("-"),
                ServersItem = CreateMenuGroup("Servers", new MenuItem[] {
                    SeperatorItem = new MenuItem("-"),
                    CreateMenuItem("Servers setting", new EventHandler(Config_Click)),
                    CreateMenuItem("Servers statistic", new EventHandler(ShowServerLogItem_Click)),
                    new MenuItem("-"),
                    CreateMenuItem("Import server from clipboard", new EventHandler(CopyAddress_Click)),
                    CreateMenuItem("Scan QRCode from screen", new EventHandler(ScanQRCodeItem_Click)),
                }),
                CreateMenuGroup("Servers Subscribe", new MenuItem[] {
                    CreateMenuItem("Subscribe setting", new EventHandler(Subscribe_Click)),
                    new MenuItem("-"),
                    CreateMenuItem("Update subscribe", new EventHandler(CheckNodeUpdate_Click)),
                }),
                new MenuItem("-"),
                CreateMenuItem("Settings", new EventHandler(Setting_Click)),
                new MenuItem("-"),
                CreateMenuItem("Quit", new EventHandler(Quit_Click))
            });
        }

        private void controller_ConfigChanged(object sender, EventArgs e)
        {
            LoadCurrentConfiguration();
            UpdateTrayIcon();
        }

        private void controller_ToggleModeChanged(object sender, EventArgs e)
        {
            Configuration config = controller.GetCurrentConfiguration();
            UpdateSysProxyMode(config);
        }

        private void controller_ToggleRuleModeChanged(object sender, EventArgs e)
        {
            Configuration config = controller.GetCurrentConfiguration();
            UpdateProxyRule(config);
        }

        private void ShowBalloonTip(string title, string content, ToolTipIcon icon, int timeout)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = content;
            _notifyIcon.BalloonTipIcon = icon;
            _notifyIcon.ShowBalloonTip(timeout);
        }

        private void updateNodeChecker_NewNodeFound(object sender, EventArgs e)
        {
            int count = 0;
            if (!String.IsNullOrEmpty(updateNodeChecker.NodeResult))
            {
                List<string> urls = new List<string>();
                updateNodeChecker.NodeResult = updateNodeChecker.NodeResult.TrimEnd('\r', '\n', ' ');
                Configuration config = controller.GetCurrentConfiguration();
                Server selected_server = null;
                if (config.index >= 0 && config.index < config.configs.Count)
                {
                    selected_server = config.configs[config.index];
                }
                try
                {
                    updateNodeChecker.NodeResult = Util.Base64.DecodeBase64(updateNodeChecker.NodeResult);
                }
                catch
                {
                    updateNodeChecker.NodeResult = "";
                }
                int max_node_num = 0;

                Match match_maxnum = Regex.Match(updateNodeChecker.NodeResult, "^MAX=([0-9]+)");
                if (match_maxnum.Success)
                {
                    try
                    {
                        max_node_num = Convert.ToInt32(match_maxnum.Groups[1].Value, 10);
                    }
                    catch { }
                }
                URL_Split(updateNodeChecker.NodeResult, ref urls);
                for (int i = urls.Count - 1; i >= 0; --i)
                {
                    if (!urls[i].StartsWith("ssr"))
                        urls.RemoveAt(i);
                }
                if (urls.Count > 0)
                {
                    bool keep_selected_server = false; // set 'false' if import all nodes
                    if (max_node_num <= 0 || max_node_num >= urls.Count)
                    {
                        urls.Reverse();
                    }
                    else
                    {
                        Random r = new Random();
                        Util.Utils.Shuffle(urls, r);
                        urls.RemoveRange(max_node_num, urls.Count - max_node_num);
                        if (!config.isDefaultConfig())
                            keep_selected_server = true;
                    }
                    string lastGroup = null;
                    string curGroup = null;
                    foreach (string url in urls)
                    {
                        try // try get group name
                        {
                            Server server = new Server(url, null);
                            if (!String.IsNullOrEmpty(server.group))
                            {
                                curGroup = server.group;
                                break;
                            }
                        }
                        catch
                        { }
                    }
                    string subscribeURL = updateSubscribeManager.URL;
                    if (String.IsNullOrEmpty(curGroup))
                    {
                        curGroup = subscribeURL;
                    }
                    for (int i = 0; i < config.serverSubscribes.Count; ++i)
                    {
                        if (subscribeURL == config.serverSubscribes[i].URL)
                        {
                            lastGroup = config.serverSubscribes[i].Group;
                            config.serverSubscribes[i].Group = curGroup;
                            break;
                        }
                    }
                    if (lastGroup == null)
                    {
                        lastGroup = curGroup;
                    }

                    if (keep_selected_server && selected_server.group == curGroup)
                    {
                        bool match = false;
                        for (int i = 0; i < urls.Count; ++i)
                        {
                            try
                            {
                                Server server = new Server(urls[i], null);
                                if (selected_server.isMatchServer(server))
                                {
                                    match = true;
                                    break;
                                }
                            }
                            catch
                            { }
                        }
                        if (!match)
                        {
                            urls.RemoveAt(0);
                            urls.Add(selected_server.GetSSRLinkForServer());
                        }
                    }

                    // import all, find difference
                    Dictionary<string, Server> old_servers = new Dictionary<string, Server>();
                    if (!String.IsNullOrEmpty(lastGroup))
                    {
                        for (int i = config.configs.Count - 1; i >= 0; --i)
                        {
                            if (lastGroup == config.configs[i].group)
                            {
                                old_servers[config.configs[i].id] = config.configs[i];
                            }
                        }
                    }
                    foreach (string url in urls)
                    {
                        try
                        {
                            Server server = new Server(url, curGroup);
                            bool match = false;
                            foreach (KeyValuePair<string, Server> pair in old_servers)
                            {
                                if (server.isMatchServer(pair.Value))
                                {
                                    match = true;
                                    old_servers.Remove(pair.Key);
                                    pair.Value.CopyServerInfo(server);
                                    ++count;
                                    break;
                                }
                            }
                            if (!match)
                            {
                                config.configs.Add(server);
                                ++count;
                            }
                        }
                        catch
                        { }
                    }
                    foreach (KeyValuePair<string, Server> pair in old_servers)
                    {
                        for (int i = config.configs.Count - 1; i >= 0; --i)
                        {
                            if (config.configs[i].id == pair.Key)
                            {
                                config.configs.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    controller.SaveServersConfig(config);

                    config = controller.GetCurrentConfiguration();
                    if (selected_server != null)
                    {
                        bool match = false;
                        for (int i = 0; i < config.configs.Count - 1; ++i)
                        {
                            if (config.configs[i].id == selected_server.id)
                            {
                                config.index = i;
                                match = true;
                                break;
                            }
                            else if (config.configs[i].group == selected_server.group)
                            {
                                if (config.configs[i].isMatchServer(selected_server))
                                {
                                    config.index = i;
                                    match = true;
                                    break;
                                }
                            }
                        }
                        if (!match)
                        {
                            config.index = 0;
                        }
                    }
                    else
                    {
                        config.index = 0;
                    }
                    controller.SaveServersConfig(config);
                }
            }
            if (count > 0)
            {
                ShowBalloonTip("ShadowsocksR", I18N.GetString("Update subscribe node success"),
                    ToolTipIcon.Info, 10000);
            }
            else
            {
                ShowBalloonTip("ShadowsocksR", I18N.GetString("Update subscribe node failure"),
                    ToolTipIcon.Info, 10000);
            }
            updateSubscribeManager.Next();
        }

        private void updateChinaIPChecker_NewChinaIPFound(object sender, EventArgs e)
        {
            int count = 0;
            if (!String.IsNullOrEmpty(updateChinaIPChecker.ChinaIPResult))
            {
                List<string> list = new List<string>();

                string[] lines = updateChinaIPChecker.ChinaIPResult
                    .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }
                    if (line.StartsWith("apnic|CN|ipv4"))
                    {
                        string[] temp = line.Split('|');
                        string[] array = temp[3].Split('.');
                        long value = long.Parse(array[0]) << 24 |
                            long.Parse(array[1]) << 16 |
                            long.Parse(array[2]) << 8 |
                            long.Parse(array[3]);
                        value += Convert.ToInt32(temp[4]) - 1;
                        string result = $"{temp[3]} " +
                            $"{(value >> 24) & 0xFF}." +
                            $"{(value >> 16) & 0xFF}." +
                            $"{(value >> 8) & 0xFF}." +
                            $"{value & 0xFF}";
                        list.Add(result);
                        count++;
                    }
                }
                if (list.Count > 0)
                {
                    string file = string.Join("\r\n", list) + "\r\n";
                    controller.ChinaIPFileUpdated(file);
                }
            }
            if (count > 0)
            {
                ShowBalloonTip("ShadowsocksR", I18N.GetString("Update China IP success"),
                  ToolTipIcon.Info, 10000);
            }
            else
            {
                ShowBalloonTip("ShadowsocksR", I18N.GetString("Update China IP failure"),
                    ToolTipIcon.Info, 10000);
            }
            updateChinaIPManager.ResetUpdate();
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            _notifyIcon.BalloonTipClicked -= notifyIcon1_BalloonTipClicked;
        }

        private void UpdateSysProxyMode(Configuration config)
        {
            enableItem.Checked = config.sysProxyMode == (int)ProxyMode.Direct;
            globalModeItem.Checked = config.sysProxyMode == (int)ProxyMode.Global;
        }

        private void UpdateProxyRule(Configuration config)
        {
            ruleDisableBypass.Checked = config.proxyRuleMode == (int)ProxyRuleMode.Disable;
            ruleBypassLan.Checked = config.proxyRuleMode == (int)ProxyRuleMode.BypassLan;
            ruleBypassChina.Checked = config.proxyRuleMode == (int)ProxyRuleMode.BypassLanAndChina;
            ruleBypassNotChina.Checked = config.proxyRuleMode == (int)ProxyRuleMode.BypassLanAndNotChina;
        }

        private void DisconnectAllConnections(Configuration config)
        {
            for (int id = 0; id < config.configs.Count; ++id)
            {
                Server server = config.configs[id];
                server.GetConnections().CloseAll();
            }
        }

        private void LoadCurrentConfiguration()
        {
            Configuration config = controller.GetCurrentConfiguration();
            UpdateServersMenu();
            UpdateSysProxyMode(config);
            UpdateProxyRule(config);
            DisconnectAllConnections(config);
        }

        private void UpdateServersMenu()
        {
            var items = ServersItem.MenuItems;
            while (items[0] != SeperatorItem)
            {
                items.RemoveAt(0);
            }

            Configuration configuration = controller.GetCurrentConfiguration();
            SortedDictionary<string, MenuItem> group = new SortedDictionary<string, MenuItem>();
            const string def_group = "!(no group)";
            string select_group = "";
            for (int i = 0; i < configuration.configs.Count; i++)
            {
                string group_name;
                Server server = configuration.configs[i];
                if (string.IsNullOrEmpty(server.group))
                    group_name = def_group;
                else
                    group_name = server.group;

                MenuItem item = new MenuItem(server.FriendlyName())
                {
                    Tag = i
                };
                item.Click += AServerItem_Click;
                if (configuration.index == i)
                {
                    item.Checked = true;
                    select_group = group_name;
                }

                if (group.ContainsKey(group_name))
                {
                    group[group_name].MenuItems.Add(item);
                }
                else
                {
                    group[group_name] = new MenuItem(group_name, new MenuItem[1] { item });
                }
            }
            {
                int i = 0;
                foreach (KeyValuePair<string, MenuItem> pair in group)
                {
                    if (pair.Key == def_group)
                    {
                        pair.Value.Text = I18N.GetString("(empty group)");
                    }
                    if (pair.Key == select_group)
                    {
                        pair.Value.Text = "● " + pair.Value.Text;
                    }
                    else
                    {
                        pair.Value.Text = "　" + pair.Value.Text;
                    }
                    items.Add(i, pair.Value);
                    ++i;
                }
            }
        }

        private void ShowConfigForm(bool addNode)
        {
            if (configForm != null)
            {
                configForm.Activate();
                if (addNode)
                {
                    Configuration cfg = controller.GetCurrentConfiguration();
                    configForm.SetServerListSelectedIndex(cfg.index + 1);
                }
            }
            else
            {
                configForm = new ConfigForm(controller, addNode ? -1 : -2);
                configForm.Show();
                configForm.Activate();
                configForm.BringToFront();
                configForm.FormClosed += configForm_FormClosed;
            }
        }

        private void ShowConfigForm(int index)
        {
            if (configForm != null)
            {
                configForm.Activate();
            }
            else
            {
                configForm = new ConfigForm(controller, index);
                configForm.Show();
                configForm.Activate();
                configForm.BringToFront();
                configForm.FormClosed += configForm_FormClosed;
            }
        }

        private void ShowSettingForm()
        {
            if (settingsForm != null)
            {
                settingsForm.Activate();
            }
            else
            {
                settingsForm = new SettingsForm(controller);
                settingsForm.Show();
                settingsForm.Activate();
                settingsForm.BringToFront();
                settingsForm.FormClosed += settingsForm_FormClosed;
            }
        }

        private void ShowServerLogForm()
        {
            Configuration config = controller.GetCurrentConfiguration();
            if (config.configs.Count == 0)
            {
                MessageBox.Show(I18N.GetString("Please add at least one server"), "ShadowsocksR",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (serverLogForm != null)
            {
                serverLogForm.Activate();
                serverLogForm.Update();
                if (serverLogForm.WindowState == FormWindowState.Minimized)
                {
                    serverLogForm.WindowState = FormWindowState.Normal;
                }
            }
            else
            {
                serverLogForm = new ServerLogForm(controller);
                serverLogForm.Show();
                serverLogForm.Activate();
                serverLogForm.BringToFront();
                serverLogForm.FormClosed += serverLogForm_FormClosed;
            }
        }

        private void ShowSubscribeSettingForm()
        {
            if (subScribeForm != null)
            {
                subScribeForm.Activate();
                subScribeForm.Update();
                if (subScribeForm.WindowState == FormWindowState.Minimized)
                {
                    subScribeForm.WindowState = FormWindowState.Normal;
                }
            }
            else
            {
                subScribeForm = new SubscribeForm(controller);
                subScribeForm.Show();
                subScribeForm.Activate();
                subScribeForm.BringToFront();
                subScribeForm.FormClosed += subScribeForm_FormClosed;
            }
        }

        void configForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            configForm = null;
            Util.Utils.ReleaseMemory();
        }

        void settingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            settingsForm = null;
            Util.Utils.ReleaseMemory();
        }

        void serverLogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            serverLogForm = null;
            Util.Utils.ReleaseMemory();
        }

        void subScribeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            subScribeForm = null;
        }

        private void Config_Click(object sender, EventArgs e)
        {
            if (typeof(int) == sender.GetType())
            {
                ShowConfigForm((int)sender);
            }
            else
            {
                ShowConfigForm(false);
            }
        }

        private void Setting_Click(object sender, EventArgs e)
        {
            ShowSettingForm();
        }

        private void Quit_Click(object sender, EventArgs e)
        {
            controller.Stop();
            if (configForm != null)
            {
                configForm.Close();
                configForm = null;
            }
            if (serverLogForm != null)
            {
                serverLogForm.Close();
                serverLogForm = null;
            }
            if (timerDelayCheckUpdate != null)
            {
                timerDelayCheckUpdate.Elapsed -= timer_Elapsed;
                timerDelayCheckUpdate.Stop();
                timerDelayCheckUpdate = null;
            }
            _notifyIcon.Visible = false;
            Application.Exit();
        }

        private void notifyIcon1_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowConfigForm(false);
            }
            else if (e.Button == MouseButtons.Middle)
            {
                ShowServerLogForm();
            }
        }

        private void EnableItem_Click(object sender, EventArgs e)
        {
            controller.ToggleMode(ProxyMode.Direct);
        }

        private void GlobalModeItem_Click(object sender, EventArgs e)
        {
            Configuration config = controller.GetCurrentConfiguration();
            if (config.configs.Count == 0)
            {
                MessageBox.Show(I18N.GetString("Please add at least one server"), "ShadowsocksR",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            controller.ToggleMode(ProxyMode.Global);
        }

        private void RuleBypassLanItem_Click(object sender, EventArgs e)
        {
            controller.ToggleRuleMode((int)ProxyRuleMode.BypassLan);
        }

        private void RuleBypassChinaItem_Click(object sender, EventArgs e)
        {
            controller.ToggleRuleMode((int)ProxyRuleMode.BypassLanAndChina);
        }

        private void RuleBypassNotChinaItem_Click(object sender, EventArgs e)
        {
            controller.ToggleRuleMode((int)ProxyRuleMode.BypassLanAndNotChina);
        }

        private void RuleBypassDisableItem_Click(object sender, EventArgs e)
        {
            controller.ToggleRuleMode((int)ProxyRuleMode.Disable);
        }

        private void UpdateChnIpItem_Click(object sender, EventArgs e)
        {
            Configuration config = controller.GetCurrentConfiguration();
            updateChinaIPManager.CreateTask(config, updateChinaIPChecker);
        }

        private void AServerItem_Click(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            controller.SelectServerIndex((int)item.Tag);
        }

        private void CheckNodeUpdate_Click(object sender, EventArgs e)
        {
            Configuration config = controller.GetCurrentConfiguration();
            if (config.serverSubscribes.Count == 0)
            {
                MessageBox.Show(I18N.GetString("Please add at least one server subscribe"), "ShadowsocksR",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            updateSubscribeManager.CreateTask(config, updateNodeChecker);
        }

        private void ShowServerLogItem_Click(object sender, EventArgs e)
        {
            ShowServerLogForm();
        }

        private void Subscribe_Click(object sender, EventArgs e)
        {
            ShowSubscribeSettingForm();
        }

        private void URL_Split(string text, ref List<string> out_urls)
        {
            if (String.IsNullOrEmpty(text))
            {
                return;
            }
            int ss_index = text.IndexOf("ss://", 1, StringComparison.OrdinalIgnoreCase);
            int ssr_index = text.IndexOf("ssr://", 1, StringComparison.OrdinalIgnoreCase);
            int index = ss_index;
            if (index == -1 || index > ssr_index && ssr_index != -1) index = ssr_index;
            if (index == -1)
            {
                out_urls.Insert(0, text);
            }
            else
            {
                out_urls.Insert(0, text.Substring(0, index));
                URL_Split(text.Substring(index), ref out_urls);
            }
        }

        private void CopyAddress_Click(object sender, EventArgs e)
        {
            try
            {
                IDataObject iData = Clipboard.GetDataObject();
                if (iData.GetDataPresent(DataFormats.Text))
                {
                    List<string> urls = new List<string>();
                    URL_Split((string)iData.GetData(DataFormats.Text), ref urls);
                    int count = 0;
                    foreach (string url in urls)
                    {
                        if (controller.AddServerBySSURL(url))
                        {
                            ++count;
                        }
                    }
                    if (count > 0)
                    {
                        ShowConfigForm(true);
                    }
                }
            }
            catch { }
        }

        private bool ScanQRCode(Screen screen, Bitmap fullImage, Rectangle cropRect, out string url, out Rectangle rect)
        {
            using (Bitmap target = new Bitmap(cropRect.Width, cropRect.Height))
            {
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(fullImage, new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                                    cropRect,
                                    GraphicsUnit.Pixel);
                }
                var source = new BitmapLuminanceSource(target);
                var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                QRCodeReader reader = new QRCodeReader();
                var result = reader.decode(bitmap);
                if (result != null)
                {
                    url = result.Text;
                    double minX = Int32.MaxValue, minY = Int32.MaxValue, maxX = 0, maxY = 0;
                    foreach (ResultPoint point in result.ResultPoints)
                    {
                        minX = Math.Min(minX, point.X);
                        minY = Math.Min(minY, point.Y);
                        maxX = Math.Max(maxX, point.X);
                        maxY = Math.Max(maxY, point.Y);
                    }
                    //rect = new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
                    rect = new Rectangle(cropRect.Left + (int)minX, cropRect.Top + (int)minY, (int)(maxX - minX), (int)(maxY - minY));
                    return true;
                }
            }
            url = "";
            rect = new Rectangle();
            return false;
        }

        private bool ScanQRCodeStretch(Screen screen, Bitmap fullImage, Rectangle cropRect, double mul, out string url, out Rectangle rect)
        {
            using (Bitmap target = new Bitmap((int)(cropRect.Width * mul), (int)(cropRect.Height * mul)))
            {
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(fullImage, new Rectangle(0, 0, target.Width, target.Height),
                                    cropRect,
                                    GraphicsUnit.Pixel);
                }
                var source = new BitmapLuminanceSource(target);
                var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                QRCodeReader reader = new QRCodeReader();
                var result = reader.decode(bitmap);
                if (result != null)
                {
                    url = result.Text;
                    double minX = Int32.MaxValue, minY = Int32.MaxValue, maxX = 0, maxY = 0;
                    foreach (ResultPoint point in result.ResultPoints)
                    {
                        minX = Math.Min(minX, point.X);
                        minY = Math.Min(minY, point.Y);
                        maxX = Math.Max(maxX, point.X);
                        maxY = Math.Max(maxY, point.Y);
                    }
                    //rect = new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
                    rect = new Rectangle(cropRect.Left + (int)(minX / mul), cropRect.Top + (int)(minY / mul), (int)((maxX - minX) / mul), (int)((maxY - minY) / mul));
                    return true;
                }
            }
            url = "";
            rect = new Rectangle();
            return false;
        }

        private Rectangle GetScanRect(int width, int height, int index, out double stretch)
        {
            stretch = 1;
            if (index < 5)
            {
                const int div = 5;
                int w = width * 3 / div;
                int h = height * 3 / div;
                Point[] pt = new Point[5] {
                    new Point(1, 1),

                    new Point(0, 0),
                    new Point(0, 2),
                    new Point(2, 0),
                    new Point(2, 2),
                };
                return new Rectangle(pt[index].X * width / div, pt[index].Y * height / div, w, h);
            }
            {
                const int base_index = 5;
                if (index < base_index + 6)
                {
                    double[] s = new double[] {
                        1,
                        2,
                        3,
                        4,
                        6,
                        8
                    };
                    stretch = 1 / s[index - base_index];
                    return new Rectangle(0, 0, width, height);
                }
            }
            {
                const int base_index = 11;
                if (index < base_index + 8)
                {
                    const int hdiv = 7;
                    const int vdiv = 5;
                    int w = width * 3 / hdiv;
                    int h = height * 3 / vdiv;
                    Point[] pt = new Point[8] {
                        new Point(1, 1),
                        new Point(3, 1),

                        new Point(0, 0),
                        new Point(0, 2),

                        new Point(2, 0),
                        new Point(2, 2),

                        new Point(4, 0),
                        new Point(4, 2),
                    };
                    return new Rectangle(pt[index - base_index].X * width / hdiv, pt[index - base_index].Y * height / vdiv, w, h);
                }
            }
            return new Rectangle(0, 0, 0, 0);
        }

        private void ScanScreenQRCode(bool ss_only)
        {
            Thread.Sleep(100);
            foreach (Screen screen in Screen.AllScreens)
            {
                Point screen_size = Util.Utils.GetScreenPhysicalSize();
                using (Bitmap fullImage = new Bitmap(screen_size.X,
                                                screen_size.Y))
                {
                    using (Graphics g = Graphics.FromImage(fullImage))
                    {
                        g.CopyFromScreen(screen.Bounds.X,
                                         screen.Bounds.Y,
                                         0, 0,
                                         fullImage.Size,
                                         CopyPixelOperation.SourceCopy);
                    }
                    bool decode_fail = false;
                    for (int i = 0; i < 100; i++)
                    {
                        Rectangle cropRect = GetScanRect(fullImage.Width, fullImage.Height, i, out double stretch);
                        if (cropRect.Width == 0)
                            break;
                        if (stretch == 1 ? ScanQRCode(screen, fullImage, cropRect, out string url, out Rectangle rect) : ScanQRCodeStretch(screen, fullImage, cropRect, stretch, out url, out rect))
                        {
                            var success = controller.AddServerBySSURL(url);
                            QRCodeSplashForm splash = new QRCodeSplashForm();
                            if (success)
                            {
                                splash.FormClosed += splash_FormClosed;
                            }
                            else if (!ss_only)
                            {
                                _urlToOpen = url;
                            }
                            else
                            {
                                decode_fail = true;
                                continue;
                            }
                            splash.Location = new Point(screen.Bounds.X, screen.Bounds.Y);
                            double dpi = Screen.PrimaryScreen.Bounds.Width / (double)screen_size.X;
                            splash.TargetRect = new Rectangle(
                                (int)(rect.Left * dpi + screen.Bounds.X),
                                (int)(rect.Top * dpi + screen.Bounds.Y),
                                (int)(rect.Width * dpi),
                                (int)(rect.Height * dpi));
                            splash.Size = new Size(fullImage.Width, fullImage.Height);
                            splash.Show();
                            return;
                        }
                    }
                    if (decode_fail)
                    {
                        MessageBox.Show(I18N.GetString("Failed to decode QRCode"), "ShadowsocksR",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            MessageBox.Show(I18N.GetString("No QRCode found. Try to zoom in or move it to the center of the screen."), "ShadowsocksR",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ScanQRCodeItem_Click(object sender, EventArgs e)
        {
            ScanScreenQRCode(false);
        }

        void splash_FormClosed(object sender, FormClosedEventArgs e)
        {
            ShowConfigForm(true);
        }

        void openURLFromQRCode(object sender, FormClosedEventArgs e)
        {
            Process.Start(_urlToOpen);
        }
    }
}
