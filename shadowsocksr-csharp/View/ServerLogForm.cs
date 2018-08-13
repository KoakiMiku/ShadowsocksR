using ShadowsocksR.Controller;
using ShadowsocksR.Model;
using ShadowsocksR.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ShadowsocksR.View
{
    public partial class ServerLogForm : Form
    {
        class DoubleBufferListView : DataGridView
        {
            public DoubleBufferListView()
            {
                SetStyle(ControlStyles.DoubleBuffer |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint,
                    true);
                UpdateStyles();
            }
        }

        private ShadowsocksController _controller;
        //private ContextMenu contextMenu1;
        private List<int> listOrder = new List<int>();
        private int lastRefreshIndex = 0;
        private bool firstDispley = true;
        private bool rowChange = false;
        private int updatePause = 0;
        private int updateTick = 0;
        private int updateSize = 0;
        private int pendingUpdate = 0;
        private string title_perfix = "";
        private ServerSpeedLogShow[] ServerSpeedLogList;
        private Thread workerThread;
        private AutoResetEvent workerEvent = new AutoResetEvent(false);

        public ServerLogForm(ShadowsocksController controller)
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;
            Icon = Icon.FromHandle(Resources.ssw128.GetHicon());
            _controller = controller;

            Width = 810;
            int dpi_mul = Util.Utils.GetDpiMul();

            Configuration config = controller.GetCurrentConfiguration();
            if (config.configs.Count < 8)
            {
                Height = 300 * dpi_mul / 4;
            }
            else if (config.configs.Count < 20)
            {
                Height = (300 + (config.configs.Count - 8) * 16) * dpi_mul / 4;
            }
            else
            {
                Height = 500 * dpi_mul / 4;
            }
            UpdateTexts();
            UpdateLog();

            ContextMenu = new ContextMenu(new MenuItem[] {
                CreateMenuItem("Clear", new EventHandler(ClearItem_Click)),
            });

            for (int i = 0; i < ServerDataGrid.Columns.Count; ++i)
            {
                ServerDataGrid.Columns[i].Width = ServerDataGrid.Columns[i].Width * dpi_mul / 4;
            }

            ServerDataGrid.RowTemplate.Height = 20 * dpi_mul / 4;
            //ServerDataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            int width = 0;
            for (int i = 0; i < ServerDataGrid.Columns.Count; ++i)
            {
                if (!ServerDataGrid.Columns[i].Visible)
                    continue;
                width += ServerDataGrid.Columns[i].Width;
            }
            Width = width + SystemInformation.VerticalScrollBarWidth + (Width - ClientSize.Width) + 1;
            ServerDataGrid.AutoResizeColumnHeadersHeight();
        }

        private MenuItem CreateMenuItem(string text, EventHandler click)
        {
            return new MenuItem(I18N.GetString(text), click);
        }

        private void UpdateTexts()
        {
            Text = title_perfix + I18N.GetString("ServerLog");
            for (int i = 0; i < ServerDataGrid.Columns.Count; ++i)
            {
                ServerDataGrid.Columns[i].HeaderText = I18N.GetString(ServerDataGrid.Columns[i].HeaderText);
            }
        }

        private string FormatBytes(long bytes)
        {
            const long K = 1024L;
            const long M = K * 1024L;
            const long G = M * 1024L;
            const long T = G * 1024L;
            const long P = T * 1024L;
            const long E = P * 1024L;

            if (bytes >= M * 990)
            {
                if (bytes >= G * 990)
                {
                    if (bytes >= P * 990)
                        return (bytes / (double)E).ToString("F3") + "E";
                    if (bytes >= T * 990)
                        return (bytes / (double)P).ToString("F3") + "P";
                    return (bytes / (double)T).ToString("F3") + "T";
                }
                else
                {
                    if (bytes >= G * 99)
                        return (bytes / (double)G).ToString("F2") + "G";
                    if (bytes >= G * 9)
                        return (bytes / (double)G).ToString("F3") + "G";
                    return (bytes / (double)G).ToString("F4") + "G";
                }
            }
            else
            {
                if (bytes >= K * 990)
                {
                    if (bytes >= M * 100)
                        return (bytes / (double)M).ToString("F1") + "M";
                    if (bytes > M * 9.9)
                        return (bytes / (double)M).ToString("F2") + "M";
                    return (bytes / (double)M).ToString("F3") + "M";
                }
                else
                {
                    if (bytes > K * 99)
                        return (bytes / (double)K).ToString("F0") + "K";
                    if (bytes > 900)
                        return (bytes / (double)K).ToString("F1") + "K";
                    return bytes.ToString();
                }
            }
        }

        public bool SetBackColor(DataGridViewCell cell, Color newColor)
        {
            if (cell.Style.BackColor != newColor)
            {
                cell.Style.BackColor = newColor;
                rowChange = true;
                return true;
            }
            return false;
        }

        public bool SetCellToolTipText(DataGridViewCell cell, string newString)
        {
            if (cell.ToolTipText != newString)
            {
                cell.ToolTipText = newString;
                rowChange = true;
                return true;
            }
            return false;
        }

        public bool SetCellText(DataGridViewCell cell, string newString)
        {
            if ((string)cell.Value != newString)
            {
                cell.Value = newString;
                rowChange = true;
                return true;
            }
            return false;
        }

        public bool SetCellText(DataGridViewCell cell, long newInteger)
        {
            if ((string)cell.Value != newInteger.ToString())
            {
                cell.Value = newInteger.ToString();
                rowChange = true;
                return true;
            }
            return false;
        }

        byte ColorMix(byte a, byte b, double alpha)
        {
            return (byte)(b * alpha + a * (1 - alpha));
        }

        Color ColorMix(Color a, Color b, double alpha)
        {
            return Color.FromArgb(ColorMix(a.R, b.R, alpha),
                ColorMix(a.G, b.G, alpha),
                ColorMix(a.B, b.B, alpha));
        }

        public void UpdateLogThread()
        {
            while (workerThread != null)
            {
                Configuration config = _controller.GetCurrentConfiguration();
                ServerSpeedLogShow[] _ServerSpeedLogList = new ServerSpeedLogShow[config.configs.Count];
                for (int i = 0; i < config.configs.Count; ++i)
                {
                    _ServerSpeedLogList[i] = config.configs[i].ServerSpeedLog().Translate();
                }
                ServerSpeedLogList = _ServerSpeedLogList;

                workerEvent.WaitOne();
            }
        }

        public void UpdateLog()
        {
            if (workerThread == null)
            {
                workerThread = new Thread(UpdateLogThread);
                workerThread.Start();
            }
            else
            {
                workerEvent.Set();
            }
        }

        public void RefreshLog()
        {
            if (ServerSpeedLogList == null)
                return;

            int last_rowcount = ServerDataGrid.RowCount;
            Configuration config = _controller.GetCurrentConfiguration();
            if (listOrder.Count > config.configs.Count)
            {
                listOrder.RemoveRange(config.configs.Count, listOrder.Count - config.configs.Count);
            }
            while (listOrder.Count < config.configs.Count)
            {
                listOrder.Add(0);
            }
            while (ServerDataGrid.RowCount < config.configs.Count && ServerDataGrid.RowCount < ServerSpeedLogList.Length)
            {
                ServerDataGrid.Rows.Add();
                int id = ServerDataGrid.RowCount - 1;
                ServerDataGrid[0, id].Value = id;
            }
            if (ServerDataGrid.RowCount > config.configs.Count)
            {
                for (int list_index = 0; list_index < ServerDataGrid.RowCount; ++list_index)
                {
                    DataGridViewCell id_cell = ServerDataGrid[0, list_index];
                    int id = (int)id_cell.Value;
                    if (id >= config.configs.Count)
                    {
                        ServerDataGrid.Rows.RemoveAt(list_index);
                        --list_index;
                    }
                }
            }
            int displayBeginIndex = ServerDataGrid.FirstDisplayedScrollingRowIndex;
            int displayEndIndex = displayBeginIndex + ServerDataGrid.DisplayedRowCount(true);
            try
            {
                for (int list_index = (lastRefreshIndex >= ServerDataGrid.RowCount) ? 0 : lastRefreshIndex, rowChangeCnt = 0;
                    list_index < ServerDataGrid.RowCount && rowChangeCnt <= 100; ++list_index)
                {
                    lastRefreshIndex = list_index + 1;

                    DataGridViewCell id_cell = ServerDataGrid[0, list_index];
                    int id = (int)id_cell.Value;
                    Server server = config.configs[id];
                    ServerSpeedLogShow serverSpeedLog = ServerSpeedLogList[id];
                    listOrder[id] = list_index;
                    rowChange = false;
                    for (int curcol = 0; curcol < ServerDataGrid.Columns.Count; ++curcol)
                    {
                        if (!firstDispley && (ServerDataGrid.SortedColumn == null || ServerDataGrid.SortedColumn.Index != curcol) &&
                            (list_index < displayBeginIndex || list_index >= displayEndIndex))
                            continue;
                        DataGridViewCell cell = ServerDataGrid[curcol, list_index];
                        string columnName = ServerDataGrid.Columns[curcol].Name;
                        // Server
                        if (columnName == "Server")
                        {
                            if (config.index == id)
                            {
                                SetBackColor(cell, Color.Cyan);
                            }
                            else
                            {
                                SetBackColor(cell, Color.White);
                            }
                            SetCellText(cell, server.FriendlyName());
                        }
                        if (columnName == "Group")
                        {
                            SetCellText(cell, server.group);
                        }
                        // TotalConnectTimes
                        else if (columnName == "TotalConnect")
                        {
                            SetCellText(cell, serverSpeedLog.totalConnectTimes);
                        }
                        // TotalConnecting
                        else if (columnName == "Connecting")
                        {
                            long connections = serverSpeedLog.totalConnectTimes - serverSpeedLog.totalDisconnectTimes;
                            Color[] colList = new Color[5] { Color.White, Color.LightGreen, Color.Yellow, Color.Red, Color.Red };
                            long[] bytesList = new long[5] { 0, 16, 32, 64, 65536 };
                            for (int i = 1; i < colList.Length; ++i)
                            {
                                if (connections < bytesList[i])
                                {
                                    SetBackColor(cell, ColorMix(colList[i - 1], colList[i], (double)(connections - bytesList[i - 1]) / (bytesList[i] - bytesList[i - 1])));
                                    break;
                                }
                            }
                            SetCellText(cell, serverSpeedLog.totalConnectTimes - serverSpeedLog.totalDisconnectTimes);
                        }
                        // AvgConnectTime
                        else if (columnName == "AvgLatency")
                        {
                            if (serverSpeedLog.avgConnectTime >= 0)
                            {
                                SetCellText(cell, serverSpeedLog.avgConnectTime / 1000);
                            }
                            else
                            {
                                SetCellText(cell, "-");
                            }
                        }
                        // AvgDownSpeed
                        else if (columnName == "AvgDownSpeed")
                        {
                            long avgBytes = serverSpeedLog.avgDownloadBytes;
                            string valStr = FormatBytes(avgBytes);
                            Color[] colList = new Color[6] { Color.White, Color.LightGreen, Color.Yellow, Color.Pink, Color.Red, Color.Red };
                            long[] bytesList = new long[6] { 0, 1024 * 64, 1024 * 512, 1024 * 1024 * 4, 1024 * 1024 * 16, 1024L * 1024 * 1024 * 1024 };
                            for (int i = 1; i < colList.Length; ++i)
                            {
                                if (avgBytes < bytesList[i])
                                {
                                    SetBackColor(cell, ColorMix(colList[i - 1], colList[i], (double)(avgBytes - bytesList[i - 1]) / (bytesList[i] - bytesList[i - 1])));
                                    break;
                                }
                            }
                            SetCellText(cell, valStr);
                        }
                        // MaxDownSpeed
                        else if (columnName == "MaxDownSpeed")
                        {
                            long maxBytes = serverSpeedLog.maxDownloadBytes;
                            string valStr = FormatBytes(maxBytes);
                            Color[] colList = new Color[6] { Color.White, Color.LightGreen, Color.Yellow, Color.Pink, Color.Red, Color.Red };
                            long[] bytesList = new long[6] { 0, 1024 * 64, 1024 * 512, 1024 * 1024 * 4, 1024 * 1024 * 16, 1024 * 1024 * 1024 };
                            for (int i = 1; i < colList.Length; ++i)
                            {
                                if (maxBytes < bytesList[i])
                                {
                                    SetBackColor(cell, ColorMix(colList[i - 1], colList[i], (double)(maxBytes - bytesList[i - 1]) / (bytesList[i] - bytesList[i - 1])));
                                    break;
                                }
                            }
                            SetCellText(cell, valStr);
                        }
                        // AvgUpSpeed
                        else if (columnName == "AvgUpSpeed")
                        {
                            long avgBytes = serverSpeedLog.avgUploadBytes;
                            string valStr = FormatBytes(avgBytes);
                            Color[] colList = new Color[6] { Color.White, Color.LightGreen, Color.Yellow, Color.Pink, Color.Red, Color.Red };
                            long[] bytesList = new long[6] { 0, 1024 * 64, 1024 * 512, 1024 * 1024 * 4, 1024 * 1024 * 16, 1024L * 1024 * 1024 * 1024 };
                            for (int i = 1; i < colList.Length; ++i)
                            {
                                if (avgBytes < bytesList[i])
                                {
                                    SetBackColor(cell, ColorMix(colList[i - 1], colList[i], (double)(avgBytes - bytesList[i - 1]) / (bytesList[i] - bytesList[i - 1])));
                                    break;
                                }
                            }
                            SetCellText(cell, valStr);
                        }
                        // MaxUpSpeed
                        else if (columnName == "MaxUpSpeed")
                        {
                            long maxBytes = serverSpeedLog.maxUploadBytes;
                            string valStr = FormatBytes(maxBytes);
                            Color[] colList = new Color[6] { Color.White, Color.LightGreen, Color.Yellow, Color.Pink, Color.Red, Color.Red };
                            long[] bytesList = new long[6] { 0, 1024 * 64, 1024 * 512, 1024 * 1024 * 4, 1024 * 1024 * 16, 1024 * 1024 * 1024 };
                            for (int i = 1; i < colList.Length; ++i)
                            {
                                if (maxBytes < bytesList[i])
                                {
                                    SetBackColor(cell, ColorMix(colList[i - 1], colList[i], (double)(maxBytes - bytesList[i - 1]) / (bytesList[i] - bytesList[i - 1])));
                                    break;
                                }
                            }
                            SetCellText(cell, valStr);
                        }
                        // TotalUploadBytes
                        else if (columnName == "Upload")
                        {
                            string valStr = FormatBytes(serverSpeedLog.totalUploadBytes);
                            string fullVal = serverSpeedLog.totalUploadBytes.ToString();
                            if (cell.ToolTipText != fullVal)
                            {
                                if (fullVal == "0")
                                {
                                    SetBackColor(cell, Color.FromArgb(0xf4, 0xff, 0xf4));
                                }
                                else
                                {
                                    SetBackColor(cell, Color.LightGreen);
                                    cell.Tag = 8;
                                }
                            }
                            else if (cell.Tag != null)
                            {
                                cell.Tag = (int)cell.Tag - 1;
                                if ((int)cell.Tag == 0) SetBackColor(cell, Color.FromArgb(0xf4, 0xff, 0xf4));
                            }
                            SetCellToolTipText(cell, fullVal);
                            SetCellText(cell, valStr);
                        }
                        // TotalDownloadBytes
                        else if (columnName == "Download")
                        {
                            string valStr = FormatBytes(serverSpeedLog.totalDownloadBytes);
                            string fullVal = serverSpeedLog.totalDownloadBytes.ToString();
                            if (cell.ToolTipText != fullVal)
                            {
                                if (fullVal == "0")
                                {
                                    SetBackColor(cell, Color.FromArgb(0xff, 0xf0, 0xf0));
                                }
                                else
                                {
                                    SetBackColor(cell, Color.LightGreen);
                                    cell.Tag = 8;
                                }
                            }
                            else if (cell.Tag != null)
                            {
                                cell.Tag = (int)cell.Tag - 1;
                                if ((int)cell.Tag == 0) SetBackColor(cell, Color.FromArgb(0xff, 0xf0, 0xf0));
                            }
                            SetCellToolTipText(cell, fullVal);
                            SetCellText(cell, valStr);
                        }
                        else if (columnName == "DownloadRaw")
                        {
                            string valStr = FormatBytes(serverSpeedLog.totalDownloadRawBytes);
                            string fullVal = serverSpeedLog.totalDownloadRawBytes.ToString();
                            SetCellToolTipText(cell, fullVal);
                            SetCellText(cell, valStr);
                        }
                        // ErrorConnectTimes
                        else if (columnName == "ConnectError")
                        {
                            long val = serverSpeedLog.errorConnectTimes + serverSpeedLog.errorDecodeTimes;
                            Color col = Color.FromArgb(255, (byte)Math.Max(0, 255 - val * 2.5), (byte)Math.Max(0, 255 - val * 2.5));
                            SetBackColor(cell, col);
                            SetCellText(cell, val);
                        }
                        // ErrorTimeoutTimes
                        else if (columnName == "ConnectTimeout")
                        {
                            SetCellText(cell, serverSpeedLog.errorTimeoutTimes);
                        }
                        // ErrorTimeoutTimes
                        else if (columnName == "ConnectEmpty")
                        {
                            long val = serverSpeedLog.errorEmptyTimes;
                            Color col = Color.FromArgb(255, (byte)Math.Max(0, 255 - val * 8), (byte)Math.Max(0, 255 - val * 8));
                            SetBackColor(cell, col);
                            SetCellText(cell, val);
                        }
                        // ErrorContinurousTimes
                        else if (columnName == "Continuous")
                        {
                            long val = serverSpeedLog.errorContinurousTimes;
                            Color col = Color.FromArgb(255, (byte)Math.Max(0, 255 - val * 8), (byte)Math.Max(0, 255 - val * 8));
                            SetBackColor(cell, col);
                            SetCellText(cell, val);
                        }
                        // ErrorPersent
                        else if (columnName == "ErrorPercent")
                        {
                            if (serverSpeedLog.errorLogTimes + serverSpeedLog.totalConnectTimes - serverSpeedLog.totalDisconnectTimes > 0)
                            {
                                double percent = (serverSpeedLog.errorConnectTimes + serverSpeedLog.errorTimeoutTimes + serverSpeedLog.errorDecodeTimes)
                                    * 100.00 / (serverSpeedLog.errorLogTimes + serverSpeedLog.totalConnectTimes - serverSpeedLog.totalDisconnectTimes);
                                SetBackColor(cell, Color.FromArgb(255, (byte)(255 - percent * 2), (byte)(255 - percent * 2)));
                                SetCellText(cell, percent.ToString("F0") + "%");
                            }
                            else
                            {
                                SetBackColor(cell, Color.White);
                                SetCellText(cell, "-");
                            }
                        }
                    }
                    if (rowChange && list_index >= displayBeginIndex && list_index < displayEndIndex)
                    {
                        rowChangeCnt++;
                    }
                }
            }
            catch { }
            if (ServerDataGrid.SortedColumn != null)
            {
                ServerDataGrid.Sort(ServerDataGrid.SortedColumn, (ListSortDirection)((int)ServerDataGrid.SortOrder - 1));
            }
            if (last_rowcount == 0 && config.index >= 0 && config.index < ServerDataGrid.RowCount)
            {
                ServerDataGrid[0, config.index].Selected = true;
            }
            if (firstDispley)
            {
                ServerDataGrid.FirstDisplayedScrollingRowIndex = Math.Max(0, config.index - ServerDataGrid.DisplayedRowCount(true) / 2);
                firstDispley = false;
            }
        }

        private void ClearItem_Click(object sender, EventArgs e)
        {
            Configuration config = _controller.GetCurrentConfiguration();
            foreach (Server server in config.configs)
            {
                server.ServerSpeedLog().Clear();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (updatePause > 0)
            {
                updatePause -= 1;
                return;
            }
            if (WindowState == FormWindowState.Minimized)
            {
                if (++pendingUpdate < 40)
                {
                    return;
                }
            }
            else
            {
                ++updateTick;
            }
            pendingUpdate = 0;
            RefreshLog();
            UpdateLog();
            if (updateSize > 1) --updateSize;
            if (updateTick == 2 || updateSize == 1)
            {
                updateSize = 0;
            }
        }

        private void ServerDataGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int id = (int)ServerDataGrid[0, e.RowIndex].Value;
                if (ServerDataGrid.Columns[e.ColumnIndex].Name == "Server")
                {
                    Configuration config = _controller.GetCurrentConfiguration();
                    _controller.SelectServerIndex(id);
                }
                ServerDataGrid[0, e.RowIndex].Selected = true;
            }
        }

        private void ServerLogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Thread thread = workerThread;
            workerThread = null;
            while (thread.IsAlive)
            {
                workerEvent.Set();
                Thread.Sleep(50);
            }
        }

        private void ServerLogForm_Move(object sender, EventArgs e)
        {
            updatePause = 0;
        }
    }
}
