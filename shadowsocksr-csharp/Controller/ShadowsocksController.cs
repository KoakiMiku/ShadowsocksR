﻿using ShadowsocksR.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace ShadowsocksR.Controller
{
    public enum ProxyMode
    {
        Direct,
        Global
    }

    public class ShadowsocksController
    {
        // controller:
        // handle user actions
        // manipulates UI
        // interacts with low level logic

        private Listener _listener;
        private List<Listener> _port_map_listener;
        private Configuration _config;
        private ServerTransferTotal _transfer;
        private IPRangeSet _rangeSet;
        private HttpProxyRunner polipoRunner;
        private bool stopped = false;
        private bool firstRun = true;
        private object locker = new object();

        public class PathEventArgs : EventArgs
        {
            public string Path;
        }

        public event EventHandler ConfigChanged;
        public event EventHandler ToggleModeChanged;
        public event EventHandler ToggleRuleModeChanged;
        public event ErrorEventHandler Errored;

        public ShadowsocksController()
        {
            _config = Configuration.Load();
            _transfer = ServerTransferTotal.Load();

            foreach (Server server in _config.configs)
            {
                if (_transfer.servers.ContainsKey(server.server))
                {
                    ServerSpeedLog log = new ServerSpeedLog(_transfer.servers[server.server].totalUploadBytes, _transfer.servers[server.server].totalDownloadBytes);
                    server.SetServerSpeedLog(log);
                }
            }
        }

        public void Start()
        {
            Reload();
        }

        protected void ReportError(Exception e)
        {
            Errored?.Invoke(this, new ErrorEventArgs(e));
        }

        // always return copy
        public Configuration GetConfiguration()
        {
            return Configuration.Load();
        }

        public Configuration GetCurrentConfiguration()
        {
            return _config;
        }

        private int FindFirstMatchServer(Server server, List<Server> servers)
        {
            for (int i = 0; i < servers.Count; ++i)
            {
                if (server.isMatchServer(servers[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public void AppendConfiguration(Configuration mergeConfig, List<Server> servers)
        {
            if (servers != null)
            {
                for (int j = 0; j < servers.Count; ++j)
                {
                    if (FindFirstMatchServer(servers[j], mergeConfig.configs) == -1)
                    {
                        mergeConfig.configs.Add(servers[j]);
                    }
                }
            }
        }

        public List<Server> MergeConfiguration(Configuration mergeConfig, List<Server> servers)
        {
            List<Server> missingServers = new List<Server>();
            if (servers != null)
            {
                for (int j = 0; j < servers.Count; ++j)
                {
                    int i = FindFirstMatchServer(servers[j], mergeConfig.configs);
                    if (i != -1)
                    {
                        bool enable = servers[j].enable;
                        servers[j].CopyServer(mergeConfig.configs[i]);
                        servers[j].enable = enable;
                    }
                }
            }
            for (int i = 0; i < mergeConfig.configs.Count; ++i)
            {
                int j = FindFirstMatchServer(mergeConfig.configs[i], servers);
                if (j == -1)
                {
                    missingServers.Add(mergeConfig.configs[i]);
                }
            }
            return missingServers;
        }

        public Configuration MergeGetConfiguration(Configuration mergeConfig)
        {
            Configuration ret = Configuration.Load();
            if (mergeConfig != null)
            {
                MergeConfiguration(mergeConfig, ret.configs);
            }
            return ret;
        }

        public void MergeConfiguration(Configuration mergeConfig)
        {
            AppendConfiguration(_config, mergeConfig.configs);
            SaveConfig(_config);
        }

        public bool SaveServersConfig(string config)
        {
            Configuration new_cfg = Configuration.Load(config);
            if (new_cfg != null)
            {
                SaveServersConfig(new_cfg);
                return true;
            }
            return false;
        }

        public void SaveServersConfig(Configuration config)
        {
            List<Server> missingServers = MergeConfiguration(_config, config.configs);
            _config.CopyFrom(config);
            foreach (Server s in missingServers)
            {
                s.GetConnections().CloseAll();
            }
            SelectServerIndex(_config.index);
        }

        public bool AddServerBySSURL(string ssURL, string force_group = null, bool toLast = false)
        {
            if (ssURL.StartsWith("ss://", StringComparison.OrdinalIgnoreCase) || ssURL.StartsWith("ssr://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var server = new Server(ssURL, force_group);
                    if (toLast)
                    {
                        _config.configs.Add(server);
                    }
                    else
                    {
                        int index = _config.index + 1;
                        if (index < 0 || index > _config.configs.Count)
                            index = _config.configs.Count;
                        _config.configs.Insert(index, server);
                    }
                    SaveConfig(_config);
                    return true;
                }
                catch (Exception e)
                {
                    Logging.LogUsefulException(e);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void ToggleMode(ProxyMode mode)
        {
            _config.sysProxyMode = (int)mode;
            SaveConfig(_config);
            ToggleModeChanged?.Invoke(this, new EventArgs());
        }

        public void ToggleRuleMode(int mode)
        {
            _config.proxyRuleMode = mode;
            SaveConfig(_config);
            ToggleRuleModeChanged?.Invoke(this, new EventArgs());
        }

        public void SelectServerIndex(int index)
        {
            _config.index = index;
            SaveConfig(_config);
        }

        public void Stop()
        {
            if (stopped)
            {
                return;
            }
            stopped = true;

            if (_port_map_listener != null)
            {
                foreach (Listener l in _port_map_listener)
                {
                    l.Stop();
                }
                _port_map_listener = null;
            }
            if (_listener != null)
            {
                _listener.Stop();
            }
            if (polipoRunner != null)
            {
                polipoRunner.Stop();
            }
            if (_config.sysProxyMode != (int)ProxyMode.Direct)
            {
                SystemProxy.Update(_config, true);
            }
            ServerTransferTotal.Save(_transfer);
        }

        public void ClearTransferTotal(string server_addr)
        {
            _transfer.Clear(server_addr);
            foreach (Server server in _config.configs)
            {
                if (server.server == server_addr)
                {
                    if (_transfer.servers.ContainsKey(server.server))
                    {
                        server.ServerSpeedLog().ClearTrans();
                    }
                }
            }
        }

        private void Reload()
        {
            lock (locker)
            {
                ReloadWait();
            }
        }

        private void ReloadWait()
        {
            if (_port_map_listener != null)
            {
                foreach (Listener l in _port_map_listener)
                {
                    l.Stop();
                }
                _port_map_listener = null;
            }

            // some logic in configuration updated the config when saving, we need to read it again
            _config = MergeGetConfiguration(_config);
            _config.FlushPortMapCache();

            _rangeSet = new IPRangeSet();
            _rangeSet.TouchChnIpFile(null);
            _rangeSet.LoadChn();
            if (_config.proxyRuleMode == (int)ProxyRuleMode.BypassLanAndNotChina)
            {
                _rangeSet.Reverse();
            }

            if (polipoRunner == null)
            {
                polipoRunner = new HttpProxyRunner();
            }
            bool _firstRun = firstRun;
            for (int i = 1; i <= 5; ++i)
            {
                _firstRun = false;
                try
                {
                    if (_listener != null && !_listener.isConfigChange(_config))
                    {
                        Local local = new Local(_config, _transfer, _rangeSet);
                        _listener.GetServices()[0] = local;
                        if (polipoRunner.HasExited())
                        {
                            polipoRunner.Stop();
                            polipoRunner.Start(_config);

                            _listener.GetServices()[3] = new HttpPortForwarder(polipoRunner.RunningPort, _config);
                        }
                    }
                    else
                    {
                        if (_listener != null)
                        {
                            _listener.Stop();
                            _listener = null;
                        }

                        polipoRunner.Stop();
                        polipoRunner.Start(_config);

                        List<Listener.IService> services = new List<Listener.IService>
                        {
                            new Local(_config, _transfer, _rangeSet),
                            new HttpPortForwarder(polipoRunner.RunningPort, _config)
                        };
                        _listener = new Listener(services);
                        _listener.Start(_config, 0);
                    }
                    break;
                }
                catch (Exception e)
                {
                    // translate Microsoft language into human language
                    // i.e. An attempt was made to access a socket in a way forbidden by its access permissions => Port already in use
                    if (e is SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.AccessDenied)
                        {
                            e = new Exception(I18N.GetString("Port already in use") + string.Format(" {0}", _config.localPort), e);
                        }
                    }
                    Logging.LogUsefulException(e);
                    if (!_firstRun)
                    {
                        ReportError(e);
                        break;
                    }
                    else
                    {
                        Thread.Sleep(1000 * i * i);
                    }
                    if (_listener != null)
                    {
                        _listener.Stop();
                        _listener = null;
                    }
                }
            }

            _port_map_listener = new List<Listener>();
            foreach (KeyValuePair<int, PortMapConfigCache> pair in _config.GetPortMapCache())
            {
                try
                {
                    Local local = new Local(_config, _transfer, _rangeSet);
                    List<Listener.IService> services = new List<Listener.IService>();
                    services.Add(local);
                    Listener listener = new Listener(services);
                    listener.Start(_config, pair.Key);
                    _port_map_listener.Add(listener);
                }
                catch (Exception e)
                {
                    // translate Microsoft language into human language
                    // i.e. An attempt was made to access a socket in a way forbidden by its access permissions => Port already in use
                    if (e is SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.AccessDenied)
                        {
                            e = new Exception(I18N.GetString("Port already in use") + string.Format(" {0}", pair.Key), e);
                        }
                    }
                    Logging.LogUsefulException(e);
                    ReportError(e);
                }
            }
            ConfigChanged?.Invoke(this, new EventArgs());
            SystemProxy.Update(_config, false);
            Util.Utils.ReleaseMemory();
        }

        protected void SaveConfig(Configuration newConfig)
        {
            Configuration.Save(newConfig);
            Reload();
        }

        public void ChinaIPFileUpdated(string file)
        {
            _rangeSet.TouchChnIpFile(file);
            Reload();
        }
    }
}
