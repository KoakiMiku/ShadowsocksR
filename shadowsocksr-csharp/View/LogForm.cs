using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ShadowsocksR.Controller;
using ShadowsocksR.Properties;

namespace ShadowsocksR.View
{
    public partial class LogForm : Form
    {
        private readonly ShadowsocksController _controller;

        private const int MaxReadSize = 65536;

        private string _currentLogFile;
        private string _currentLogFileName;
        private long _currentOffset;

        public LogForm(ShadowsocksController controller)
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;
            Icon = Icon.FromHandle(Resources.ssw128.GetHicon());
            _controller = controller;

            UpdateTexts();
        }

        private void UpdateTexts()
        {
            Text = I18N.GetString("Log Viewer");
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            ReadLog();
        }

        private void ReadLog()
        {
            var newLogFile = Logging.LogFile;
            if (newLogFile != _currentLogFile)
            {
                _currentOffset = 0;
                _currentLogFile = newLogFile;
                _currentLogFileName = Logging.LogFileName;
            }
            try
            {
                using (var reader = new StreamReader(new FileStream(newLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    if (_currentOffset == 0)
                    {
                        var maxSize = reader.BaseStream.Length;
                        if (maxSize > MaxReadSize)
                        {
                            reader.BaseStream.Seek(-MaxReadSize, SeekOrigin.End);
                            reader.ReadLine();
                        }
                    }
                    else
                    {
                        reader.BaseStream.Seek(_currentOffset, SeekOrigin.Begin);
                    }
                    var txt = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(txt))
                    {
                        logTextBox.AppendText(txt);
                        logTextBox.ScrollToCaret();
                    }
                    _currentOffset = reader.BaseStream.Position;
                }
            }
            catch (FileNotFoundException)
            {
            }
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            ReadLog();
        }

        private void LogForm_Shown(object sender, EventArgs e)
        {
            logTextBox.ScrollToCaret();
        }
    }
}
