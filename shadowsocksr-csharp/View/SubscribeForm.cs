using ShadowsocksR.Controller;
using ShadowsocksR.Model;
using ShadowsocksR.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShadowsocksR.View
{
    public partial class SubscribeForm : Form
    {
        private ShadowsocksController _controller;
        // this is a copy of configuration that we are working on
        private Configuration _modifiedConfiguration;
        private int _old_select_index;

        public SubscribeForm(ShadowsocksController controller)
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;
            Icon = Icon.FromHandle(Resources.ssw128.GetHicon());
            _controller = controller;

            UpdateTexts();
            controller.ConfigChanged += controller_ConfigChanged;

            LoadCurrentConfiguration();
        }

        private void UpdateTexts()
        {
            Text = I18N.GetString("Subscribe Settings");
            label1.Text = I18N.GetString("URL");
            label2.Text = I18N.GetString("Group");
            checkBoxAutoUpdate.Text = I18N.GetString("Auto update");
            buttonOK.Text = I18N.GetString("OK");
            buttonCancel.Text = I18N.GetString("Cancel");
            buttonAdd.Text = I18N.GetString("Add");
            buttonModify.Text = I18N.GetString("Modify");
            buttonDel.Text = I18N.GetString("Delete");
        }

        private void SubscribeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _controller.ConfigChanged -= controller_ConfigChanged;
        }

        private void controller_ConfigChanged(object sender, EventArgs e)
        {
            LoadCurrentConfiguration();
        }

        private void LoadCurrentConfiguration()
        {
            _modifiedConfiguration = _controller.GetConfiguration();
            LoadAllSettings();
            if (listServerSubscribe.Items.Count == 0)
            {
                buttonModify.Enabled = false;
                buttonDel.Enabled = false;
            }
            else
            {
                buttonModify.Enabled = true;
                buttonDel.Enabled = true;
            }
        }

        private void LoadAllSettings()
        {
            int select_index = 0;
            checkBoxAutoUpdate.Checked = _modifiedConfiguration.nodeFeedAutoUpdate;
            UpdateList();
            UpdateSelected(select_index);
            SetSelectIndex(select_index);
        }

        private void SaveAllSettings()
        {
            if (listServerSubscribe.Items.Count == 0)
            {
                _modifiedConfiguration.nodeFeedAutoUpdate = false;
            }
            else
            {
                _modifiedConfiguration.nodeFeedAutoUpdate = checkBoxAutoUpdate.Checked;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            int select_index = listServerSubscribe.SelectedIndex;
            SaveSelected(select_index);
            SaveAllSettings();
            _controller.SaveServersConfig(_modifiedConfiguration);
            Close();
        }

        private void textBoxURL_TextChanged(object sender, EventArgs e)
        {
            textBoxGroup.Text = "";
        }

        private void UpdateList()
        {
            listServerSubscribe.Items.Clear();
            for (int i = 0; i < _modifiedConfiguration.serverSubscribes.Count; ++i)
            {
                ServerSubscribe ss = _modifiedConfiguration.serverSubscribes[i];
                listServerSubscribe.Items.Add((String.IsNullOrEmpty(ss.Group) ? "" : ss.Group + " - ") + ss.URL);
            }
        }

        private void SetSelectIndex(int index)
        {
            if (index >= 0 && index < _modifiedConfiguration.serverSubscribes.Count)
            {
                listServerSubscribe.SelectedIndex = index;
            }
        }

        private void UpdateSelected(int index)
        {
            if (index >= 0 && index < _modifiedConfiguration.serverSubscribes.Count)
            {
                ServerSubscribe ss = _modifiedConfiguration.serverSubscribes[index];
                textBoxURL.Text = ss.URL;
                textBoxGroup.Text = ss.Group;
                _old_select_index = index;
            }
        }

        private void SaveSelected(int index)
        {
            if (index >= 0 && index < _modifiedConfiguration.serverSubscribes.Count)
            {
                ServerSubscribe ss = _modifiedConfiguration.serverSubscribes[index];
                ss.URL = textBoxURL.Text;
                ss.Group = textBoxGroup.Text;
            }
        }

        private void listServerSubscribe_SelectedIndexChanged(object sender, EventArgs e)
        {
            int select_index = listServerSubscribe.SelectedIndex;
            if (_old_select_index == select_index)
                return;
            UpdateSelected(select_index);
            SetSelectIndex(select_index);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxURL.Text)) return;
            int select_index = _modifiedConfiguration.serverSubscribes.Count;
            if (_old_select_index >= 0 && _old_select_index < _modifiedConfiguration.serverSubscribes.Count)
            {
                var serverSubscribe = new ServerSubscribe
                {
                    URL = textBoxURL.Text
                };
                _modifiedConfiguration.serverSubscribes.Insert(select_index, serverSubscribe);
            }
            else
            {
                var serverSubscribe = new ServerSubscribe
                {
                    URL = textBoxURL.Text
                };
                _modifiedConfiguration.serverSubscribes.Add(serverSubscribe);
            }
            UpdateList();
            UpdateSelected(select_index);
            SetSelectIndex(select_index);
            if (listServerSubscribe.Items.Count == 0)
            {
                buttonModify.Enabled = false;
                buttonDel.Enabled = false;
            }
            else
            {
                buttonModify.Enabled = true;
                buttonDel.Enabled = true;
            }
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            int select_index = listServerSubscribe.SelectedIndex;
            if (select_index >= 0 && select_index < _modifiedConfiguration.serverSubscribes.Count)
            {
                _modifiedConfiguration.serverSubscribes.RemoveAt(select_index);
                if (select_index >= _modifiedConfiguration.serverSubscribes.Count)
                {
                    select_index = _modifiedConfiguration.serverSubscribes.Count - 1;
                }
                UpdateList();
                UpdateSelected(select_index);
                SetSelectIndex(select_index);
            }
            if (listServerSubscribe.Items.Count == 0)
            {
                buttonModify.Enabled = false;
                buttonDel.Enabled = false;
            }
            else
            {
                buttonModify.Enabled = true;
                buttonDel.Enabled = true;
            }
        }

        private void buttonModify_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxURL.Text)) return;
            int select_index = listServerSubscribe.SelectedIndex;
            SaveSelected(select_index);
            UpdateList();
            UpdateSelected(select_index);
            SetSelectIndex(select_index);
        }
    }
}
