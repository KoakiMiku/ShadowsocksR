using ShadowsocksR.Controller;
using ShadowsocksR.Properties;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ShadowsocksR.Encryption
{
    public class MbedTLS
    {
        const string DLLNAME = "libsscrypto";
        public const int MBEDTLS_DECRYPT = 0;
        public const int MBEDTLS_ENCRYPT = 1;
        public const int MBEDTLS_MD_MD5 = 3;

        static MbedTLS()
        {
            string runningPath = Path.Combine(System.Windows.Forms.Application.StartupPath, @"temp");
            if (!Directory.Exists(runningPath))
            {
                Directory.CreateDirectory(runningPath);
            }
            string dllPath = Path.Combine(runningPath, "libsscrypto.dll");
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
            catch (IOException) { }
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
        public static extern IntPtr cipher_info_from_string(string cipher_name);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void cipher_init(IntPtr ctx);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_setup(IntPtr ctx, IntPtr cipher_info);

        // XXX: Check operation before using it
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_setkey(IntPtr ctx, byte[] key, int key_bitlen, int operation);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_set_iv(IntPtr ctx, byte[] iv, int iv_len);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_get_size_ex();

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_reset(IntPtr ctx);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cipher_update(IntPtr ctx, byte[] input, int ilen, byte[] output, ref int olen);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void cipher_free(IntPtr ctx);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void md5(byte[] input, int ilen, byte[] output);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ss_hmac_ex(int md_type, byte[] key, int keylen, byte[] input, int offset, int ilen, byte[] output);
    }
}
