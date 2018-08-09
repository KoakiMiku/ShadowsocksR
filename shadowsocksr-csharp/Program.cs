﻿using Microsoft.Win32;
using ShadowsocksR.Controller;
using ShadowsocksR.Model;
using ShadowsocksR.View;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ShadowsocksR
{
    static class Program
    {
        static ShadowsocksController _controller;
        static MenuViewController _viewController;

        [STAThread]
        static void Main(string[] args)
        {
            using (Mutex mutex = new Mutex(false, "ShadowsocksR_" + Application.StartupPath.GetHashCode()))
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.EnableVisualStyles();
                Application.ApplicationExit += Application_ApplicationExit;
                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
                Application.SetCompatibleTextRenderingDefault(false);

                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show(I18N.GetString("Find ShadowsocksR icon in your notify tray.") + "\n" +
                        I18N.GetString("If you want to start multiple ShadowsocksR, make a copy in another directory."),
                        I18N.GetString("ShadowsocksR is already running"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Directory.SetCurrentDirectory(Application.StartupPath);
                Logging.OpenLogFile();
                if (!Configuration.CheckFile())
                {
                    MessageBox.Show(I18N.GetString("Failed to load config file."), "ShadowsocksR",
                         MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                _controller = new ShadowsocksController();
                _viewController = new MenuViewController(_controller);
                _controller.Start();
                Application.Run();
            }
        }

        private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    if (_controller != null)
                    {
                        System.Timers.Timer timer = new System.Timers.Timer(5 * 1000);
                        timer.Elapsed += Timer_Elapsed;
                        timer.AutoReset = false;
                        timer.Enabled = true;
                        timer.Start();
                    }
                    break;
                case PowerModes.Suspend:
                    if (_controller != null) _controller.Stop();
                    break;
            }
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_controller != null) _controller.Start();
            }
            catch (Exception ex)
            {
                Logging.LogUsefulException(ex);
            }
            finally
            {
                try
                {
                    System.Timers.Timer timer = (System.Timers.Timer)sender;
                    timer.Enabled = false;
                    timer.Stop();
                    timer.Dispose();
                }
                catch (Exception ex)
                {
                    Logging.LogUsefulException(ex);
                }
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (_controller != null) _controller.Stop();
            _controller = null;
        }

        private static int exited = 0;

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Interlocked.Increment(ref exited) == 1)
            {
                Logging.Log(LogLevel.Error, e.ExceptionObject != null ? e.ExceptionObject.ToString() : "");
                MessageBox.Show(I18N.GetString("Unexpected error, ShadowsocksR will exit.") +
                    Environment.NewLine + (e.ExceptionObject != null ? e.ExceptionObject.ToString() : ""),
                    "ShadowsocksR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }
}
