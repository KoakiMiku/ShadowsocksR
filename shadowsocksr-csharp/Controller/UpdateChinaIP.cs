using ShadowsocksR.Model;
using System;
using System.Net;

namespace ShadowsocksR.Controller
{
    public class UpdateChinaIP
    {
        private const string URL = "http://ftp.apnic.net/apnic/stats/apnic/delegated-apnic-latest";

        public event EventHandler NewChinaIPFound;
        public string ChinaIPResult;

        public void CheckUpdate(Configuration config)
        {
            ChinaIPResult = null;
            try
            {
                WebClient http = new WebClient();
                http.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.3319.102 Safari/537.36");
                http.QueryString["rnd"] = Util.Utils.RandUInt32().ToString();
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
                http.DownloadStringCompleted += http_DownloadStringCompleted;
                http.DownloadStringAsync(new Uri(URL));
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
            }
        }

        private void http_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                ChinaIPResult = e.Result;
                NewChinaIPFound?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                ChinaIPResult = null;
                if (e.Error != null)
                {
                    Logging.Debug(e.Error.ToString());
                }
                Logging.Debug(ex.ToString());
                NewChinaIPFound?.Invoke(this, new EventArgs());
            }
        }
    }

    public class UpdateChinaIPManager
    {
        Configuration _config;
        UpdateChinaIP _updater;

        public void CreateTask(Configuration config, UpdateChinaIP updater)
        {
            if (_config == null)
            {
                _config = config;
                _updater = updater;
                _updater.CheckUpdate(_config);
            }
        }

        public void Reset()
        {
            _config = null;
        }
    }
}
