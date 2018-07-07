using Newtonsoft.Json;
using ShadowsocksR.Model;
using ShadowsocksR.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ShadowsocksR.Controller
{
    public class GFWListUpdater
    {
        private const string GFWLIST_URL = "https://raw.githubusercontent.com/gfwlist/gfwlist/master/gfwlist.txt";

        private static string PAC_FILE = PACServer.PAC_FILE;

        private static string USER_RULE_FILE = PACServer.USER_RULE_FILE;

        private static string gfwlist_template = Resources.ssr_gfw;

        public int update_type;

        public event EventHandler<ResultEventArgs> UpdateCompleted;

        public event ErrorEventHandler Error;

        public class ResultEventArgs : EventArgs
        {
            public bool Success;

            public ResultEventArgs(bool success)
            {
                Success = success;
            }
        }

        private void http_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string tempPath = Path.Combine(System.Windows.Forms.Application.StartupPath, @"temp");
                File.WriteAllText($"{tempPath}\\gfwlist.txt", e.Result, Encoding.UTF8);
                List<string> lines = ParseResult(e.Result);
                if (lines.Count == 0)
                {
                    throw new Exception("Empty GFWList");
                }
                if (File.Exists(USER_RULE_FILE))
                {
                    string local = File.ReadAllText(USER_RULE_FILE, Encoding.UTF8);
                    string[] rules = local.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string rule in rules)
                    {
                        if (rule.StartsWith("!") || rule.StartsWith("["))
                            continue;
                        lines.Add(rule);
                    }
                }
                string abpContent = gfwlist_template;
                abpContent = abpContent.Replace("__RULES__", JsonConvert.SerializeObject(lines, Formatting.Indented));
                if (File.Exists(PAC_FILE))
                {
                    string original = File.ReadAllText(PAC_FILE, Encoding.UTF8);
                    if (original == abpContent)
                    {
                        update_type = 0;
                        UpdateCompleted(this, new ResultEventArgs(false));
                        return;
                    }
                }
                File.WriteAllText(PAC_FILE, abpContent, Encoding.UTF8);
                if (UpdateCompleted != null)
                {
                    update_type = 0;
                    UpdateCompleted(this, new ResultEventArgs(true));
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new ErrorEventArgs(ex));
            }
        }

        public void UpdatePACFromGFWList(Configuration config)
        {
            try
            {
                WebClient http = new WebClient();
                if (config.sysProxyMode != (int)ProxyMode.Direct)
                {
                    WebProxy proxy = new WebProxy(IPAddress.Loopback.ToString(), config.localPort);
                    if (!string.IsNullOrEmpty(config.authPass))
                    {
                        proxy.Credentials = new NetworkCredential(config.authUser, config.authPass);
                    }
                    http.Proxy = proxy;
                }
                else
                {
                    http.Proxy = null;
                }
                http.BaseAddress = GFWLIST_URL;
                http.DownloadStringCompleted += http_DownloadStringCompleted;
                http.DownloadStringAsync(new Uri(GFWLIST_URL + "?rnd=" + Util.Utils.RandUInt32().ToString()));
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new ErrorEventArgs(ex));
            }
        }

        public List<string> ParseResult(string response)
        {
            byte[] bytes = Convert.FromBase64String(response);
            string content = Encoding.ASCII.GetString(bytes);
            string[] lines = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> valid_lines = new List<string>(lines.Length);
            foreach (string line in lines)
            {
                if (line.StartsWith("!") || line.StartsWith("["))
                    continue;
                valid_lines.Add(line);
            }
            return valid_lines;
        }
    }
}