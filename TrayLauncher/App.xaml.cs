//
// TrayLauncher - A customizable tray menu to launch applications and folders.
//
#region using directives
using System.Threading;
using System.Windows;
using TKUtils;
#endregion

namespace TrayLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Change name if debugging
#if DEBUG   // Change name when debugging
            const string appName = "TrayLauncher_Debug";
#else
        const string appName = "TrayLauncher";
#endif
        #endregion

        #region Only One Instance
        /////////////////////////////////////////////////
        // Make sure that only one instance is running //
        /////////////////////////////////////////////////
        protected override void OnStartup(StartupEventArgs e)
        {
            _ = new Mutex(true, appName, out bool createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application
                WriteLog.WriteTempFile("");
                WriteLog.WriteTempFile("* An instance of TrayLauncher is already running!  Shutting this one down.");
                _ = MessageBox.Show("An instance of TrayLauncher is already running",
                                    "TrayLauncher", MessageBoxButton.OK, MessageBoxImage.Exclamation,
                                    MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                Voodoo.WindowsLogoffOrShutdown = true;
                Current.Shutdown();
            }

            base.OnStartup(e);
        }
        #endregion

        #region Session ending
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Voodoo.WindowsLogoffOrShutdown = true;
        }
        #endregion
    }
}
