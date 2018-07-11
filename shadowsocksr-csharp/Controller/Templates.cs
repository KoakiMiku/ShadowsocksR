using ShadowsocksR.Properties;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace ShadowsocksR.Controller
{
    class Templates
    {
        public static void UncompressFile()
        {
            try
            {
                string templatesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "templates");
                if (!Directory.Exists(templatesDirectory))
                {
                    Directory.CreateDirectory(templatesDirectory);
                    using (var templatesStream = new MemoryStream(Resources.templates))
                    {
                        var zipArchive = new ZipArchive(templatesStream, ZipArchiveMode.Read);
                        foreach (var item in zipArchive.Entries)
                        {
                            try
                            {
                                byte[] buffer = new byte[item.Length];
                                using (var stream = item.Open())
                                {
                                    stream.Read(buffer, 0, buffer.Length);
                                    string filename = Path.Combine(templatesDirectory, item.Name);
                                    using (var fileStream = new FileStream(filename, FileMode.Create))
                                    {
                                        fileStream.Write(buffer, 0, buffer.Length);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
