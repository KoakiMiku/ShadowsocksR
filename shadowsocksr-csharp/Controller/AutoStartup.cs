using System;
using Microsoft.Win32;

namespace ShadowsocksR.Controller
{
    class AutoStartup
    {
        static string Key = "ShadowsocksR";
        static string RegistryRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static bool Set(bool enabled)
        {
            RegistryKey runKey = null;
            try
            {
                string path = Util.Utils.GetExecutablePath();
                runKey = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true);
                if (enabled)
                {
                    runKey.SetValue(Key, path);
                }
                else
                {
                    runKey.DeleteValue(Key);
                }
                runKey.Close();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (runKey != null)
                {
                    try
                    {
                        runKey.Close();
                    }
                    catch (Exception e)
                    {
                        Logging.LogUsefulException(e);
                    }
                }
            }
        }

        public static bool Check()
        {
            RegistryKey runKey = null;
            try
            {
                string path = Util.Utils.GetExecutablePath();
                runKey = Registry.CurrentUser.OpenSubKey(RegistryRunPath, false);
                string[] runList = runKey.GetValueNames();
                runKey.Close();
                foreach (string item in runList)
                {
                    if (item.Equals(Key))
                        return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                return false;
            }
            finally
            {
                if (runKey != null)
                {
                    try
                    {
                        runKey.Close();
                    }
                    catch (Exception e)
                    {
                        Logging.LogUsefulException(e);
                    }
                }
            }
        }
    }
}
