// TrayLauncher - A customizable tray menu to launch applications and folders.
#region using directives

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using TKUtils;

#endregion using directives

namespace TrayLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            OneInstance();

            SplashScreen();

            base.OnStartup(e);
        }

        #region Only One Instance
        private static void OneInstance()
        {
            // Ensure only one instance of the process running
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
        }

        #endregion Only One Instance

        #region Splash screen
        private void SplashScreen()
        {
            //initialize the splash screen and set it as the application main window
            Splash splashScreen = new Splash();
            MainWindow = splashScreen;
            splashScreen.Show();

            // in order to ensure the UI stays responsive, we need to do the work on a different thread
            _ = Task.Factory.StartNew(() =>
            {
                // simulate some work being done the sleep value should be >= the total animation time
                //System.Threading.Thread.Sleep(2500);

                // since we're not on the UI thread once we're done we need to use the Dispatcher to
                // create and show the main window
                Dispatcher.Invoke(() =>
                {
                    // initialize the main window, set it as the application main window and close
                    // the splash screen
                    MainWindow mainWindow = new MainWindow();
                    MainWindow = mainWindow;

                    // Normally we'd show the MainWindow here, but we don't want to show it for TrayLauncher
                    //mainWindow.Show();
                    //splashScreen.Close();
                });
            });


        }

        #endregion Splish Splash

        #region Session ending

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Voodoo.WindowsLogoffOrShutdown = true;
        }

        #endregion Session ending
    }
}