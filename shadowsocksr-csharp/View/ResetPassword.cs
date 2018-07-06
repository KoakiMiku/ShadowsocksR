﻿using ShadowsocksR.Controller;
using ShadowsocksR.Model;
using ShadowsocksR.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShadowsocksR.View
{
    public partial class ResetPassword : Form
    {
        public ResetPassword()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Resources.ssw128.GetHicon());
            Text = I18N.GetString("ResetPassword");
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textPassword.Text == textPassword2.Text && Configuration.SetPasswordTry(textOld.Text, textPassword.Text))
            {
                Configuration cfg = Configuration.Load();
                Configuration.SetPassword(textPassword.Text);
                Configuration.Save(cfg);
                Close();
            }
            else
            {
                MessageBox.Show(I18N.GetString("Password NOT match"), "SSR error", MessageBoxButtons.OK);
            }
        }

        private void ResetPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textOld.Focused)
                {
                    textPassword.Focus();
                }
                else if (textPassword.Focused)
                {
                    textPassword2.Focus();
                }
                else
                {
                    buttonOK_Click(this, new EventArgs());
                }
            }
        }
    }
}
