using ShadowsocksR.Controller;
using ShadowsocksR.Properties;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ShadowsocksR.Encryption
{
    public class Sodium
    {
        const string DLLNAME = "libsscrypto";
        const int MBEDTLS_MD_MD5 = 3;

        static Sodium()
        {
            string dllPath = Path.Combine(Path.Combine(System.Windows.Forms.Application.StartupPath, @"temp"), "libsscrypto.dll");
            try
            {
                if (IntPtr.Size == 4)
                {
                    FileManager.UncompressFile(dllPath, Resources.libsscrypto_dll);
                }
                else
                {
                    FileManager.UncompressFile(dllPath, Resources.libsscrypto64_dll);
                }
                LoadLibrary(dllPath);
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
            }
        }

        public static byte[] ComputeHash(byte[] key, byte[] buffer, int offset, int count)
        {
            byte[] output = new byte[64];
            ss_hmac_ex(MBEDTLS_MD_MD5, key, key.Length, buffer, offset, count, output);
            return output;
        }

        public static byte[] MD5(byte[] input)
        {
            byte[] output = new byte[16];
            md5(input, input.Length, output);
            return output;
        }

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void md5(byte[] input, int ilen, byte[] output);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ss_hmac_ex(int md_type, byte[] key, int keylen, byte[] input, int offset, int ilen, byte[] output);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void crypto_stream_salsa20_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, ulong ic, byte[] k);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void crypto_stream_chacha20_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, ulong ic, byte[] k);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void crypto_stream_chacha20_ietf_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, ulong ic, byte[] k);
    }
}
