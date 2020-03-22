// TrayLauncher - A customizable tray menu to launch applications and folders.
#region using directives

using System;
using System.Diagnostics;
using System.Windows;
using TKUtils;

#endregion using directives

namespace TrayLauncher
{
    public partial class App : Application
    {
        #region Only One Instance

        protected override void OnStartup(StartupEventArgs e)
        {
            Process currentProcess = Process.GetCurrentProcess();
            Debug.WriteLine($"+++ Current process = {currentProcess.ProcessName} {currentProcess.Id}");

            foreach (var AllProcesses in Process.GetProcesses())
            {
                if (AllProcesses.Id != currentProcess.Id && AllProcesses.ProcessName == currentProcess.ProcessName)
                {
                    WriteLog.WriteTempFile("");
                    string msg = $"* I am  {currentProcess.ProcessName} {currentProcess.Id}. " +
                                 $"- {AllProcesses.ProcessName} {AllProcesses.Id} is also running.";
                    WriteLog.WriteTempFile(msg);
                    WriteLog.WriteTempFile("* An instance of TrayLauncher is already running!  Shutting this one down.");
                    WriteLog.WriteTempFile("");

                    _ = MessageBox.Show("An instance of TrayLauncher is already running",
                                        "TrayLauncher",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Exclamation,
                                        MessageBoxResult.OK,
                                        MessageBoxOptions.DefaultDesktopOnly);

                    Voodoo.WindowsLogoffOrShutdown = true;
                    Environment.Exit(1);
                    break;
                }
            }
            base.OnStartup(e);
        }

        #endregion Only One Instance

        #region Session ending

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Voodoo.WindowsLogoffOrShutdown = true;
        }

        #endregion Session ending
    }
}