﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ShadowsocksR.Obfs
{
    class TlsAuthData
    {
        public byte[] clientID;
        public Dictionary<string, byte[]> ticket_buf;
    }

    class TlsTicketAuth : ObfsBase
    {
        public TlsTicketAuth(string method) : base(method)
        {
            handshake_status = 0;
            if (method == "tls1.2_ticket_fastauth")
                fastauth = true;
        }

        private static Dictionary<string, int[]> _obfs = new Dictionary<string, int[]> {
                {"tls1.2_ticket_auth", new int[]  {0, 1, 1}},
                {"tls1.2_ticket_fastauth", new int[]  {0, 1, 1}},
        };

        private int handshake_status;
        private List<byte[]> data_sent_buffer = new List<byte[]>();
        private byte[] data_recv_buffer = new byte[0];
        private uint send_id = 0;
        private bool fastauth = false;
        protected static RNGCryptoServiceProvider g_random = new RNGCryptoServiceProvider();
        protected Random random = new Random();
        protected const int overhead = 5;

        public static List<string> SupportedObfs()
        {
            return new List<string>(_obfs.Keys);
        }

        public override Dictionary<string, int[]> GetObfs()
        {
            return _obfs;
        }

        public override object InitData()
        {
            return new TlsAuthData();
        }

        public override bool isAlwaysSendback()
        {
            return true;
        }

        public override int GetOverhead()
        {
            return overhead;
        }

        protected byte[] sni(string url)
        {
            if (url == null)
            {
                url = "";
            }
            byte[] b_url = System.Text.Encoding.UTF8.GetBytes(url);
            int len = b_url.Length;
            byte[] ret = new byte[len + 9];
            Array.Copy(b_url, 0, ret, 9, len);
            ret[7] = (byte)(len >> 8);
            ret[8] = (byte)len;
            len += 3;
            ret[4] = (byte)(len >> 8);
            ret[5] = (byte)len;
            len += 2;
            ret[2] = (byte)(len >> 8);
            ret[3] = (byte)len;
            return ret;
        }

        protected byte to_val(char c)
        {
            if (c > '9')
            {
                return (byte)(c - 'a' + 10);
            }
            else
            {
                return (byte)(c - '0');
            }
        }

        protected byte[] to_bin(string str)
        {
            byte[] ret = new byte[str.Length / 2];
            for (int i = 0; i < str.Length; i += 2)
            {
                ret[i / 2] = (byte)((to_val(str[i]) << 4) | to_val(str[i + 1]));
            }
            return ret;
        }

        protected void hmac_sha1(byte[] data, int length)
        {
            byte[] key = new byte[Server.key.Length + 32];
            Server.key.CopyTo(key, 0);
            ((TlsAuthData)Server.data).clientID.CopyTo(key, Server.key.Length);

            HMACSHA1 sha1 = new HMACSHA1(key);
            byte[] sha1data = sha1.ComputeHash(data, 0, length - 10);

            Array.Copy(sha1data, 0, data, length - 10, 10);
        }

        public void PackAuthData(byte[] outdata)
        {
            int outlength = 32;
            {
                byte[] randomdata = new byte[18];
                g_random.GetBytes(randomdata);
                randomdata.CopyTo(outdata, 4);
            }
            TlsAuthData authData = (TlsAuthData)Server.data;
            lock (authData)
            {
                if (authData.clientID == null)
                {
                    authData.clientID = new byte[32];
                    g_random.GetBytes(authData.clientID);
                }
            }
            UInt64 utc_time_second = (UInt64)Math.Floor(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            UInt32 utc_time = (UInt32)(utc_time_second);
            byte[] time_bytes = BitConverter.GetBytes(utc_time);
            Array.Reverse(time_bytes);
            Array.Copy(time_bytes, 0, outdata, 0, 4);

            hmac_sha1(outdata, outlength);
        }

        protected void PackData(byte[] data, ref int start, int len, byte[] outdata, ref int outlength)
        {
            outdata[outlength] = 0x17;
            outdata[outlength + 1] = 0x3;
            outdata[outlength + 2] = 0x3;
            outdata[outlength + 3] = (byte)(len >> 8);
            outdata[outlength + 4] = (byte)(len);
            Array.Copy(data, start, outdata, outlength + 5, len);
            start += len;
            outlength += len + 5;
            ++send_id;
        }

        public override byte[] ClientEncode(byte[] encryptdata, int datalength, out int outlength)
        {
            if (handshake_status == -1)
            {
                SentLength += datalength;
                outlength = datalength;
                return encryptdata;
            }
            byte[] outdata = new byte[datalength + 4096];
            if ((handshake_status & 4) == 4)
            {
                int start = 0;
                outlength = 0;
                while (send_id <= 4 && datalength - start > 256)
                {
                    int len = random.Next(512) + 64;
                    if (len > datalength - start) len = datalength - start;
                    PackData(encryptdata, ref start, len, outdata, ref outlength);
                }
                while (datalength - start > 2048)
                {
                    int len = random.Next(4096) + 100;
                    if (len > datalength - start) len = datalength - start;
                    PackData(encryptdata, ref start, len, outdata, ref outlength);
                }
                if (datalength - start > 0)
                {
                    PackData(encryptdata, ref start, datalength - start, outdata, ref outlength);
                }
                return outdata;
            }
            if (datalength > 0)
            {
                byte[] data = new byte[datalength + 5];
                data[0] = 0x17;
                data[1] = 0x3;
                data[2] = 0x3;
                data[3] = (byte)(datalength >> 8);
                data[4] = (byte)(datalength);
                Array.Copy(encryptdata, 0, data, 5, datalength);
                data_sent_buffer.Add(data);
            }
            if ((handshake_status & 3) != 0)
            {
                outlength = 0;
                if ((handshake_status & 2) == 0)
                {
                    int[] finish_len_set = new int[] { 32 }; //, 40, 64
                    int finish_len = finish_len_set[random.Next(finish_len_set.Length)];
                    byte[] hmac_data = new byte[11 + finish_len];
                    byte[] rnd = new byte[finish_len - 10];
                    random.NextBytes(rnd);

                    byte[] handshake_finish = System.Text.Encoding.ASCII.GetBytes("\x14\x03\x03\x00\x01\x01" + "\x16\x03\x03\x00\x20");
                    handshake_finish[handshake_finish.Length - 1] = (byte)finish_len;
                    handshake_finish.CopyTo(hmac_data, 0);
                    rnd.CopyTo(hmac_data, handshake_finish.Length);

                    hmac_sha1(hmac_data, hmac_data.Length);

                    data_sent_buffer.Insert(0, hmac_data);
                    SentLength -= hmac_data.Length;
                    handshake_status |= 2;
                }
                if (datalength == 0 || fastauth)
                {
                    foreach (byte[] data in data_sent_buffer)
                    {
                        Util.Utils.SetArrayMinSize2(ref outdata, outlength + data.Length);
                        Array.Copy(data, 0, outdata, outlength, data.Length);
                        SentLength += data.Length;
                        outlength += data.Length;
                    }
                    data_sent_buffer.Clear();
                }
                if (datalength == 0)
                {
                    handshake_status |= 4;
                }
            }
            else
            {
                byte[] rnd = new byte[32];
                PackAuthData(rnd);
                List<byte> ssl_buf = new List<byte>();
                List<byte> ext_buf = new List<byte>();
                string str_buf = "001cc02bc02fcca9cca8cc14cc13c00ac014c009c013009c0035002f000a0100";
                ssl_buf.AddRange(rnd);
                ssl_buf.Add(32);
                ssl_buf.AddRange(((TlsAuthData)Server.data).clientID);
                ssl_buf.AddRange(to_bin(str_buf));

                str_buf = "ff01000100";
                ext_buf.AddRange(to_bin(str_buf));
                string host = Server.host;
                if (Server.param.Length > 0)
                {
                    string[] hosts = Server.param.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (hosts != null && hosts.Length > 0)
                    {
                        host = hosts[random.Next(hosts.Length)];
                        host = host.Trim(' ');
                    }
                }
                if (!string.IsNullOrEmpty(host) && host[host.Length - 1] >= '0' && host[host.Length - 1] <= '9' && Server.param.Length == 0)
                {
                    host = "";
                }
                ext_buf.AddRange(sni(host));
                string str_buf2 = "001700000023";
                ext_buf.AddRange(to_bin(str_buf2));
                {
                    TlsAuthData authData = (TlsAuthData)Server.data;
                    byte[] ticket = null;
                    lock (authData)
                    {
                        if (authData.ticket_buf == null)
                        {
                            authData.ticket_buf = new Dictionary<string, byte[]>();
                        }
                        if (!authData.ticket_buf.ContainsKey(host) || random.Next(16) == 0)
                        {
                            int ticket_size = random.Next(32, 196) * 2;
                            ticket = new byte[ticket_size];
                            g_random.GetBytes(ticket);
                            authData.ticket_buf[host] = ticket;
                        }
                        else
                        {
                            ticket = authData.ticket_buf[host];
                        }
                    }
                    ext_buf.Add((byte)(ticket.Length >> 8));
                    ext_buf.Add((byte)(ticket.Length & 0xff));
                    ext_buf.AddRange(ticket);
                }
                string str_buf3 = "000d0016001406010603050105030401040303010303020102030005000501000000000012000075500000000b00020100000a0006000400170018";
                str_buf3 += "00150066000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
                ext_buf.AddRange(to_bin(str_buf3));
                ext_buf.Insert(0, (byte)(ext_buf.Count % 256));
                ext_buf.Insert(0, (byte)((ext_buf.Count - 1) / 256));

                ssl_buf.AddRange(ext_buf);
                // client version
                ssl_buf.Insert(0, 3); // version
                ssl_buf.Insert(0, 3);
                // length
                ssl_buf.Insert(0, (byte)(ssl_buf.Count % 256));
                ssl_buf.Insert(0, (byte)((ssl_buf.Count - 1) / 256));
                ssl_buf.Insert(0, 0);
                ssl_buf.Insert(0, 1); // client hello
                // length
                ssl_buf.Insert(0, (byte)(ssl_buf.Count % 256));
                ssl_buf.Insert(0, (byte)((ssl_buf.Count - 1) / 256));
                //
                ssl_buf.Insert(0, 0x1); // version
                ssl_buf.Insert(0, 0x3);
                ssl_buf.Insert(0, 0x16);
                for (int i = 0; i < ssl_buf.Count; ++i)
                {
                    outdata[i] = ssl_buf[i];
                }
                outlength = ssl_buf.Count;

                handshake_status = 1;
            }
            return outdata;
        }

        public override byte[] ClientDecode(byte[] encryptdata, int datalength, out int outlength, out bool needsendback)
        {
            if (handshake_status == -1)
            {
                outlength = datalength;
                needsendback = false;
                return encryptdata;
            }
            else if ((handshake_status & 8) == 8)
            {
                Array.Resize(ref data_recv_buffer, data_recv_buffer.Length + datalength);
                Array.Copy(encryptdata, 0, data_recv_buffer, data_recv_buffer.Length - datalength, datalength);
                needsendback = false;
                byte[] outdata = new byte[65536];
                outlength = 0;
                while (data_recv_buffer.Length > 5)
                {
                    if (data_recv_buffer[0] != 0x17)
                        throw new ObfsException("ClientDecode appdata error");
                    int len = (data_recv_buffer[3] << 8) + data_recv_buffer[4];
                    int pack_len = len + 5;
                    if (pack_len > data_recv_buffer.Length)
                        break;
                    Array.Copy(data_recv_buffer, 5, outdata, outlength, len);
                    outlength += len;
                    byte[] buffer = new byte[data_recv_buffer.Length - pack_len];
                    Array.Copy(data_recv_buffer, pack_len, buffer, 0, buffer.Length);
                    data_recv_buffer = buffer;
                }
                return outdata;
            }
            else
            {
                Array.Resize(ref data_recv_buffer, data_recv_buffer.Length + datalength);
                Array.Copy(encryptdata, 0, data_recv_buffer, data_recv_buffer.Length - datalength, datalength);
                outlength = 0;
                needsendback = false;
                byte[] outdata = new byte[data_recv_buffer.Length];
                if (data_recv_buffer.Length >= 11 + 32 + 1 + 32)
                {
                    byte[] data = new byte[32];
                    Array.Copy(data_recv_buffer, 11, data, 0, 22);
                    hmac_sha1(data, data.Length);

                    if (!Util.Utils.BitCompare(data_recv_buffer, 11 + 22, data, 22, 10))
                    {
                        throw new ObfsException("ClientDecode data error: wrong sha1");
                    }

                    int headerlength = data_recv_buffer.Length;
                    data = new byte[headerlength];
                    Array.Copy(data_recv_buffer, 0, data, 0, headerlength - 10);
                    hmac_sha1(data, headerlength);
                    if (!Util.Utils.BitCompare(data_recv_buffer, headerlength - 10, data, headerlength - 10, 10))
                    {
                        headerlength = 0;
                        while (headerlength < data_recv_buffer.Length &&
                            (data_recv_buffer[headerlength] == 0x14 || data_recv_buffer[headerlength] == 0x16))
                        {
                            headerlength += 5;
                            if (headerlength >= data_recv_buffer.Length)
                            {
                                return encryptdata;
                            }
                            headerlength += (data_recv_buffer[headerlength - 2] << 8) | data_recv_buffer[headerlength - 1];
                            if (headerlength > data_recv_buffer.Length)
                            {
                                return encryptdata;
                            }
                        }
                        data = new byte[headerlength];
                        Array.Copy(data_recv_buffer, 0, data, 0, headerlength - 10);
                        hmac_sha1(data, headerlength);

                        if (!Util.Utils.BitCompare(data_recv_buffer, headerlength - 10, data, headerlength - 10, 10))
                        {
                            throw new ObfsException("ClientDecode data error: wrong sha1");
                        }
                    }
                    byte[] buffer = new byte[data_recv_buffer.Length - headerlength];
                    Array.Copy(data_recv_buffer, headerlength, buffer, 0, buffer.Length);
                    data_recv_buffer = buffer;
                    handshake_status |= 8;
                    byte[] ret = ClientDecode(encryptdata, 0, out outlength, out needsendback);
                    needsendback = true;
                    return ret;
                }
                return encryptdata;
            }
        }
    }
}
