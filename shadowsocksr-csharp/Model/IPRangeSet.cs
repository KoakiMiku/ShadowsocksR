﻿using ShadowsocksR.Properties;
using System;
using System.IO;
using System.Net;

namespace ShadowsocksR.Model
{
    public class IPRangeSet
    {
        private const string CHN_FILENAME = "chn-ip.txt";
        private uint[] _set;

        public IPRangeSet()
        {
            _set = new uint[256 * 256 * 8];
        }

        public void TouchChnIpFile(string file)
        {
            if (file == null)
            {
                if (!File.Exists(CHN_FILENAME))
                {
                    File.WriteAllText(CHN_FILENAME, Resources.chn_ip);
                }
            }
            else
            {
                File.WriteAllText(CHN_FILENAME, file);
            }
        }

        public void InsertRange(uint begin, uint end)
        {
            begin /= 256;
            end /= 256;
            for (uint i = begin; i <= end; ++i)
            {
                uint pos = i / 32;
                int mv = (int)(i & 31);
                _set[pos] |= (1u << mv);
            }
        }

        public void Insert(uint begin, uint size)
        {
            InsertRange(begin, begin + size - 1);
        }

        public void Insert(IPAddress addr, uint size)
        {
            byte[] bytes_addr = addr.GetAddressBytes();
            Array.Reverse(bytes_addr);
            Insert(BitConverter.ToUInt32(bytes_addr, 0), size);
        }

        public void Insert(IPAddress addr_beg, IPAddress addr_end)
        {
            byte[] bytes_addr_beg = addr_beg.GetAddressBytes();
            Array.Reverse(bytes_addr_beg);
            byte[] bytes_addr_end = addr_end.GetAddressBytes();
            Array.Reverse(bytes_addr_end);
            InsertRange(BitConverter.ToUInt32(bytes_addr_beg, 0), BitConverter.ToUInt32(bytes_addr_end, 0));
        }

        public bool isIn(uint ip)
        {
            ip /= 256;
            uint pos = ip / 32;
            int mv = (int)(ip & 31);
            return (_set[pos] & (1u << mv)) != 0;
        }

        public bool IsInIPRange(IPAddress addr)
        {
            byte[] bytes_addr = addr.GetAddressBytes();
            Array.Reverse(bytes_addr);
            return isIn(BitConverter.ToUInt32(bytes_addr, 0));
        }

        public bool LoadChn()
        {
            string absFilePath = Path.Combine(System.Windows.Forms.Application.StartupPath, CHN_FILENAME);
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
                            {
                                break;
                            }
                            string[] parts = line.Split(' ');
                            if (parts.Length < 2)
                            {
                                continue;
                            }
                            bool ifBegin = IPAddress.TryParse(parts[0], out IPAddress addr_beg);
                            bool ifEnd = IPAddress.TryParse(parts[1], out IPAddress addr_end);
                            if (ifBegin && ifEnd)
                            {
                                Insert(addr_beg, addr_end);
                            }
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

        public void Reverse()
        {
            IPAddress.TryParse("240.0.0.0", out IPAddress addr_beg);
            IPAddress.TryParse("255.255.255.255", out IPAddress addr_end);
            Insert(addr_beg, addr_end);
            for (uint i = 0; i < _set.Length; ++i)
            {
                _set[i] = ~_set[i];
            }
        }
    }
}
