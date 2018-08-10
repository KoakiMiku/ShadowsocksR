using ShadowsocksR.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ShadowsocksR.Model
{
    class HostNode
    {
        public bool include_sub;
        public string addr;
        public Dictionary<string, HostNode> subnode;

        public HostNode()
        {
            include_sub = false;
            addr = "";
            subnode = new Dictionary<string, HostNode>();
        }

        public HostNode(bool sub, string addr)
        {
            include_sub = sub;
            this.addr = addr;
            subnode = null;
        }
    }

    public class HostMap
    {
        Dictionary<string, HostNode> root = new Dictionary<string, HostNode>();
        IPSegment ips = new IPSegment("remoteproxy");
        FileSystemWatcher HostFileWatcher;

        static HostMap instance = new HostMap();
        const string HOST_FILENAME = "host.txt";

        public event EventHandler HostFileChanged;

        public HostMap()
        {
            WatchHostFile();
        }

        public static HostMap Instance()
        {
            return instance;
        }

        private void WatchHostFile()
        {
            if (HostFileWatcher != null)
            {
                HostFileWatcher.Dispose();
            }
            HostFileWatcher = new FileSystemWatcher(Directory.GetCurrentDirectory())
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = HOST_FILENAME
            };
            HostFileWatcher.Changed += HostFileWatcher_Changed;
            HostFileWatcher.Created += HostFileWatcher_Changed;
            HostFileWatcher.Deleted += HostFileWatcher_Changed;
            HostFileWatcher.Renamed += HostFileWatcher_Changed;
            HostFileWatcher.EnableRaisingEvents = true;
        }

        private void HostFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            HostFileChanged?.Invoke(this, new EventArgs());
        }

        public string TouchHostFile()
        {
            if (File.Exists(HOST_FILENAME))
            {
                return HOST_FILENAME;
            }
            else
            {
                File.WriteAllText(HOST_FILENAME, Resources.host);
                return HOST_FILENAME;
            }
        }

        public void Clear(HostMap newInstance)
        {
            if (newInstance == null)
            {
                instance = new HostMap();
            }
            else
            {
                instance = newInstance;
            }
        }

        public void AddHost(string host, string addr)
        {
            if (IPAddress.TryParse(host, out IPAddress ip_addr))
            {
                string[] addr_parts = addr.Split(new char[] { ' ', '\t', }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (addr_parts.Length >= 2)
                {
                    ips.insert(new IPAddressCmp(host), new IPAddressCmp(addr_parts[0]), addr_parts[1]);
                    return;
                }
            }

            string[] parts = host.Split('.');
            Dictionary<string, HostNode> node = root;
            bool include_sub = false;
            int end = 0;
            if (parts[0].Length == 0)
            {
                end = 1;
                include_sub = true;
            }
            for (int i = parts.Length - 1; i > end; --i)
            {
                if (!node.ContainsKey(parts[i]))
                {
                    node[parts[i]] = new HostNode();
                }
                if (node[parts[i]].subnode == null)
                {
                    node[parts[i]].subnode = new Dictionary<string, HostNode>();
                }
                node = node[parts[i]].subnode;
            }
            node[parts[end]] = new HostNode(include_sub, addr);
        }

        public bool GetHost(string host, out string addr)
        {
            string[] parts = host.Split('.');
            Dictionary<string, HostNode> node = root;
            addr = null;
            for (int i = parts.Length - 1; i >= 0; --i)
            {
                if (!node.ContainsKey(parts[i]))
                {
                    return false;
                }
                if (node[parts[i]].addr.Length > 0 || node[parts[i]].include_sub)
                {
                    addr = node[parts[i]].addr;
                    return true;
                }
                if (node.ContainsKey("*"))
                {
                    addr = node["*"].addr;
                    return true;
                }
                if (node[parts[i]].subnode == null)
                {
                    return false;
                }
                node = node[parts[i]].subnode;
            }
            return false;
        }

        public bool GetIP(IPAddress ip, out string addr)
        {
            string host = ip.ToString();
            addr = ips.Get(new IPAddressCmp(host)) as string;
            return addr != null;
        }

        public bool LoadHostFile()
        {
            string filename = HOST_FILENAME;
            string absFilePath = Path.Combine(System.Windows.Forms.Application.StartupPath, filename);
            if (File.Exists(absFilePath))
            {
                try
                {
                    using (StreamReader stream = File.OpenText(absFilePath))
                    {
                        while (true)
                        {
                            string line = stream.ReadLine();
                            if (line == null)
                                break;
                            if (line.Length > 0 && line.StartsWith("#"))
                                continue;
                            string[] parts = line.Split(new char[] { ' ', '\t', }, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length < 2)
                                continue;
                            AddHost(parts[0], parts[1]);
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}
