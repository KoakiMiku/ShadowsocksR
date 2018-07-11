﻿using ShadowsocksR.Model;
using System;
using System.Collections.Generic;
using System.Net;

namespace ShadowsocksR.Controller
{
    public class UpdateNode
    {
        public event EventHandler NewNodeFound;
        public string NodeResult;

        public const string Name = "ShadowsocksR";

        public void CheckUpdate(Configuration config, string URL, bool use_proxy)
        {
            NodeResult = null;
            try
            {
                WebClient http = new WebClient();
                http.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.3319.102 Safari/537.36");
                http.QueryString["rnd"] = Util.Utils.RandUInt32().ToString();
                if (use_proxy)
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
                string response = e.Result;
                NodeResult = response;

                NewNodeFound?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                if (e.Error != null)
                {
                    Logging.Debug(e.Error.ToString());
                }
                Logging.Debug(ex.ToString());
                NewNodeFound?.Invoke(this, new EventArgs());
                return;
            }
        }
    }

    public class UpdateSubscribeManager
    {
        Configuration _config;
        List<ServerSubscribe> _serverSubscribes;
        UpdateNode _updater;
        bool _use_proxy;

        public void CreateTask(Configuration config, UpdateNode updater, bool use_proxy)
        {
            if (_config == null)
            {
                _config = config;
                _updater = updater;
                _use_proxy = use_proxy;
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
                URL = _serverSubscribes[0].URL;
                _updater.CheckUpdate(_config, URL, _use_proxy);
                _serverSubscribes.RemoveAt(0);
                return true;
            }
        }

        public string URL { get; set; }
    }
}
