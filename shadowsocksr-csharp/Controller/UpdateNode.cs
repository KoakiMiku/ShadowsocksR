using ShadowsocksR.Model;
using System;
using System.Collections.Generic;
using System.Net;

namespace ShadowsocksR.Controller
{
    public class UpdateNode
    {
        public event EventHandler NewNodeFound;
        public string NodeResult;

        public void CheckUpdate(Configuration config, string URL)
        {
            NodeResult = null;
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
                NodeResult = e.Result;
                NewNodeFound?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                NodeResult = null;
                if (e.Error != null)
                {
                    Logging.Debug(e.Error.ToString());
                }
                Logging.Debug(ex.ToString());
                NewNodeFound?.Invoke(this, new EventArgs());
            }
        }
    }

    public class UpdateSubscribeManager
    {
        Configuration _config;
        List<ServerSubscribe> _serverSubscribes;
        UpdateNode _updater;
        string _URL;

        public void CreateTask(Configuration config, UpdateNode updater)
        {
            if (_config == null)
            {
                _config = config;
                _updater = updater;
                _serverSubscribes = new List<ServerSubscribe>();
                for (int i = 0; i < config.serverSubscribes.Count; ++i)
                {
                    _serverSubscribes.Add(config.serverSubscribes[i]);
                }
                Next();
            }
        }

        public bool Next()
        {
            if (_serverSubscribes.Count == 0)
            {
                _config = null;
                return false;
            }
            else
            {
                _URL = _serverSubscribes[0].URL;
                _updater.CheckUpdate(_config, _URL);
                _serverSubscribes.RemoveAt(0);
                return true;
            }
        }

        public string URL { get; set; }
    }
}
