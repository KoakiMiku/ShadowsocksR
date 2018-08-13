﻿using ShadowsocksR.Model;
using ShadowsocksR.Util;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace ShadowsocksR.Controller
{
    class Socks5Forwarder : Listener.IService
    {
        private Configuration _config;
        private IPRangeSet _IPRange;
        const int CONNECT_DIRECT = 1;
        const int CONNECT_LOCALPROXY = 2;
        const int CONNECT_REMOTEPROXY = 0;

        public Socks5Forwarder(Configuration config, IPRangeSet IPRange)
        {
            _config = config;
            _IPRange = IPRange;
        }

        public bool Handle(byte[] firstPacket, int length, Socket socket)
        {
            return Handle(firstPacket, length, socket, null);
        }

        public bool Handle(byte[] firstPacket, int length, Socket socket, string local_sendback_protocol)
        {
            int handle = IsHandle(firstPacket, length, socket);
            if (handle > 0)
            {
                new Handler().Start(_config, _IPRange, firstPacket, length, socket, local_sendback_protocol);
                return true;
            }
            return false;
        }

        public int IsHandle(byte[] firstPacket, int length, Socket socket)
        {
            if (length >= 7 && _config.proxyRuleMode != (int)ProxyRuleMode.Disable)
            {
                IPAddress ipAddress = null;
                if (firstPacket[0] == 1)
                {
                    byte[] addr = new byte[4];
                    Array.Copy(firstPacket, 1, addr, 0, addr.Length);
                    ipAddress = new IPAddress(addr);
                }
                else if (firstPacket[0] == 3)
                {
                    int len = firstPacket[1];
                    byte[] addr = new byte[len];
                    if (length >= len + 2)
                    {
                        Array.Copy(firstPacket, 2, addr, 0, addr.Length);
                        string host = Encoding.UTF8.GetString(firstPacket, 2, len);
                        if (IPAddress.TryParse(host, out ipAddress))
                        {
                            //pass
                        }
                        else
                        {
                            if ((_config.proxyRuleMode == (int)ProxyRuleMode.BypassLanAndChina ||
                                _config.proxyRuleMode == (int)ProxyRuleMode.BypassLanAndNotChina) &&
                                _IPRange != null)
                            {
                                if (!IPAddress.TryParse(host, out ipAddress))
                                {
                                    if (ipAddress == null)
                                    {
                                        ipAddress = Utils.DnsBuffer.Get(host);
                                    }
                                }
                                if (ipAddress == null)
                                {
                                    if (host.IndexOf('.') >= 0)
                                    {
                                        ipAddress = Utils.QueryDns(host, _config.dnsServer);
                                    }
                                    else
                                    {
                                        ipAddress = Utils.QueryDns(host, null);
                                    }
                                    if (ipAddress != null)
                                    {
                                        Utils.DnsBuffer.Set(host, new IPAddress(ipAddress.GetAddressBytes()));
                                        if (host.IndexOf('.') >= 0)
                                        {
                                            if (Utils.isLocal(ipAddress)) // assume that it is polution if return LAN address
                                            {
                                                return CONNECT_REMOTEPROXY;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Logging.Log(LogLevel.Debug, "DNS query fail: " + host);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (firstPacket[0] == 4)
                {
                    byte[] addr = new byte[16];
                    Array.Copy(firstPacket, 1, addr, 0, addr.Length);
                    ipAddress = new IPAddress(addr);
                }
                if (ipAddress != null)
                {
                    if (Utils.isLocal(ipAddress))
                    {
                        return CONNECT_DIRECT;
                    }
                    if ((_config.proxyRuleMode == (int)ProxyRuleMode.BypassLanAndChina ||
                        _config.proxyRuleMode == (int)ProxyRuleMode.BypassLanAndNotChina) &&
                        _IPRange != null &&
                        ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (_IPRange.IsInIPRange(ipAddress))
                        {
                            return CONNECT_LOCALPROXY;
                        }
                        Utils.DnsBuffer.Sweep();
                    }
                }
            }
            return CONNECT_REMOTEPROXY;
        }

        class Handler : IHandler
        {
            private IPRangeSet _IPRange;
            private Configuration _config;
            private byte[] _firstPacket;
            private int _firstPacketLength;
            private ProxySocketTunLocal _local;
            private ProxySocketTun _remote;
            private bool _closed = false;
            private string _remote_host;
            private int _remote_port;
            public const int RecvSize = 1460 * 8;
            // remote receive buffer
            private byte[] remoteRecvBuffer = new byte[RecvSize];
            // connection receive buffer
            private byte[] connetionRecvBuffer = new byte[RecvSize];
            private int _totalRecvSize = 0;
            protected int TTL = 600;
            protected System.Timers.Timer timer;
            protected object timerLock = new object();
            protected DateTime lastTimerSetTime;

            public void Start(Configuration config, IPRangeSet IPRange, byte[] firstPacket, int length, Socket socket, string local_sendback_protocol)
            {
                _IPRange = IPRange;
                _firstPacket = firstPacket;
                _firstPacketLength = length;
                _local = new ProxySocketTunLocal(socket)
                {
                    local_sendback_protocol = local_sendback_protocol
                };
                _config = config;
                Connect();
            }

            private void Connect()
            {
                try
                {
                    IPAddress ipAddress = null;
                    int _targetPort = 0;
                    {
                        if (_firstPacket[0] == 1)
                        {
                            byte[] addr = new byte[4];
                            Array.Copy(_firstPacket, 1, addr, 0, addr.Length);
                            ipAddress = new IPAddress(addr);
                            _targetPort = (_firstPacket[5] << 8) | _firstPacket[6];
                            _remote_host = ipAddress.ToString();
                            Logging.Info("Direct" + " connect " + _remote_host + ":" + _targetPort.ToString());
                        }
                        else if (_firstPacket[0] == 4)
                        {
                            byte[] addr = new byte[16];
                            Array.Copy(_firstPacket, 1, addr, 0, addr.Length);
                            ipAddress = new IPAddress(addr);
                            _targetPort = (_firstPacket[17] << 8) | _firstPacket[18];
                            _remote_host = ipAddress.ToString();
                            Logging.Info("Direct" + " connect " + _remote_host + ":" + _targetPort.ToString());
                        }
                        else if (_firstPacket[0] == 3)
                        {
                            int len = _firstPacket[1];
                            byte[] addr = new byte[len];
                            Array.Copy(_firstPacket, 2, addr, 0, addr.Length);
                            _remote_host = Encoding.UTF8.GetString(_firstPacket, 2, len);
                            _targetPort = (_firstPacket[len + 2] << 8) | _firstPacket[len + 3];
                            Logging.Info("Direct" + " connect " + _remote_host + ":" + _targetPort.ToString());

                            if (!IPAddress.TryParse(_remote_host, out ipAddress))
                            {
                                if (ipAddress == null)
                                {
                                    ipAddress = Utils.DnsBuffer.Get(_remote_host);
                                }
                            }
                            if (ipAddress == null)
                            {
                                if (_remote_host.IndexOf('.') >= 0)
                                {
                                    ipAddress = Utils.QueryDns(_remote_host, _config.dnsServer);
                                }
                                else
                                {
                                    ipAddress = Utils.QueryDns(_remote_host, null);
                                }
                            }
                            if (ipAddress != null)
                            {
                                Utils.DnsBuffer.Set(_remote_host, new IPAddress(ipAddress.GetAddressBytes()));
                                Utils.DnsBuffer.Sweep();
                            }
                            else
                            {
                                throw new SocketException((int)SocketError.HostNotFound);
                            }
                        }
                        _remote_port = _targetPort;
                    }

                    // ProxyAuth recv only socks5 head, so don't need to save anything else
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, _targetPort);

                    _remote = new ProxySocketTun(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
                    _remote.GetSocket().NoDelay = true;

                    // Connect to the remote endpoint.
                    _remote.BeginConnect(remoteEP,
                        new AsyncCallback(ConnectCallback), null);
                }
                catch (Exception e)
                {
                    Logging.LogUsefulException(e);
                    Close();
                }
            }

            private void ConnectCallback(IAsyncResult ar)
            {
                if (_closed)
                {
                    return;
                }
                try
                {
                    _remote.EndConnect(ar);
                    StartPipe();
                }
                catch (Exception e)
                {
                    Logging.LogUsefulException(e);
                    Close();
                }
            }

            private void ResetTimeout(Double time)
            {
                if (time <= 0 && timer == null)
                    return;

                if (time <= 0)
                {
                    if (timer != null)
                    {
                        lock (timerLock)
                        {
                            if (timer != null)
                            {
                                timer.Enabled = false;
                                timer.Elapsed -= timer_Elapsed;
                                timer.Dispose();
                                timer = null;
                            }
                        }
                    }
                }
                else
                {
                    if (lastTimerSetTime != null && (DateTime.Now - lastTimerSetTime).TotalMilliseconds > 500)
                    {
                        lock (timerLock)
                        {
                            if (timer == null)
                            {
                                timer = new System.Timers.Timer(time * 1000.0);
                                timer.Elapsed += timer_Elapsed;
                            }
                            else
                            {
                                timer.Interval = time * 1000.0;
                                timer.Stop();
                            }
                            timer.Start();
                            lastTimerSetTime = DateTime.Now;
                        }
                    }
                }
            }

            private void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                if (_closed)
                {
                    return;
                }
                Close();
            }

            private void StartPipe()
            {
                if (_closed)
                {
                    return;
                }
                try
                {
                    Server.GetForwardServerRef().GetConnections().AddRef(this);
                    _remote.BeginReceive(remoteRecvBuffer, RecvSize, 0,
                        new AsyncCallback(PipeRemoteReceiveCallback), null);
                    _local.BeginReceive(connetionRecvBuffer, RecvSize, 0,
                        new AsyncCallback(PipeConnectionReceiveCallback), null);

                    _local.Send(connetionRecvBuffer, 0, 0);
                    ResetTimeout(TTL);
                }
                catch (Exception e)
                {
                    Logging.LogUsefulException(e);
                    Close();
                }
            }

            private void PipeRemoteReceiveCallback(IAsyncResult ar)
            {
                if (_closed)
                {
                    return;
                }
                try
                {
                    int bytesRead = _remote.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        ResetTimeout(TTL);
                        //_local.BeginSend(remoteRecvBuffer, bytesRead, 0, new AsyncCallback(PipeConnectionSendCallback), null);
                        _local.Send(remoteRecvBuffer, bytesRead, 0);
                        _totalRecvSize += bytesRead;
                        if (_totalRecvSize <= 1024 * 1024 * 2)
                        {
                            _remote.BeginReceive(remoteRecvBuffer, RecvSize, 0,
                                new AsyncCallback(PipeRemoteReceiveCallback), null);
                        }
                        else
                            PipeRemoteReceiveLoop();
                    }
                    else
                    {
                        Close();
                    }
                }
                catch (Exception e)
                {
                    Logging.LogUsefulException(e);
                    Close();
                }
            }

            private void PipeRemoteReceiveLoop()
            {
                bool final_close = false;
                byte[] recv_buffer = new byte[RecvSize];
                DateTime beforeReceive = DateTime.Now;
                while (!_closed)
                {
                    try
                    {
                        int bytesRead = _remote.Receive(recv_buffer, RecvSize, 0);
                        DateTime now = DateTime.Now;
                        if (_remote != null && _remote.IsClose)
                        {
                            final_close = true;
                            break;
                        }
                        if (_closed)
                        {
                            break;
                        }
                        ResetTimeout(TTL);

                        if (bytesRead > 0)
                        {
                            _local.Send(recv_buffer, bytesRead, 0);

                            if ((now - beforeReceive).TotalSeconds > 5)
                            {
                                _totalRecvSize = 0;
                                _remote.BeginReceive(remoteRecvBuffer, RecvSize, 0,
                                    new AsyncCallback(PipeRemoteReceiveCallback), null);
                                return;
                            }
                            else
                            {
                                beforeReceive = now;
                            }
                        }
                        else
                        {
                            Close();
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogUsefulException(e);
                        final_close = true;
                        break;
                    }
                }
                if (final_close)
                    Close();
            }

            private void PipeConnectionReceiveCallback(IAsyncResult ar)
            {
                if (_closed)
                {
                    return;
                }
                try
                {
                    int bytesRead = _local.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        ResetTimeout(TTL);
                        //_remote.BeginSend(connetionRecvBuffer, bytesRead, 0, new AsyncCallback(PipeRemoteSendCallback), null);
                        _remote.Send(connetionRecvBuffer, bytesRead, 0);
                        _local.BeginReceive(connetionRecvBuffer, RecvSize, 0,
                            new AsyncCallback(PipeConnectionReceiveCallback), null);
                    }
                    else
                    {
                        Close();
                    }
                }
                catch (Exception e)
                {
                    Logging.LogUsefulException(e);
                    Close();
                }
            }

            private void CloseSocket(ProxySocketTun sock)
            {
                lock (this)
                {
                    if (sock != null)
                    {
                        ProxySocketTun s = sock;
                        try
                        {
                            s.Shutdown(SocketShutdown.Both);
                        }
                        catch { }
                        try
                        {
                            s.Close();
                        }
                        catch { }
                    }
                }
            }

            public void Close()
            {
                lock (this)
                {
                    if (_closed)
                    {
                        return;
                    }
                    _closed = true;
                }
                ResetTimeout(0);
                Thread.Sleep(100);
                CloseSocket(_remote);
                CloseSocket(_local);
                Server.GetForwardServerRef().GetConnections().DecRef(this);
            }

            public override void Shutdown()
            {
                InvokeHandler handler = () => Close();
                handler.BeginInvoke(null, null);
            }
        }
    }
}
