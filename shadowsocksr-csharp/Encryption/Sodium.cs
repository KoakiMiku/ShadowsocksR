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
            catch (IOException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void crypto_stream_salsa20_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, ulong ic, byte[] k);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void crypto_stream_chacha20_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, ulong ic, byte[] k);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int crypto_stream_chacha20_ietf_xor_ic(byte[] c, byte[] m, ulong mlen, byte[] n, uint ic, byte[] k);
    }
}
