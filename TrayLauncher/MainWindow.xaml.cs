//
// TrayLauncher - A customizable tray menu to launch applications, websites and folders.
//
// See App.xaml.cs for code that restricts app to one instance
//

#region using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;
using System.Diagnostics;
using System.Reflection;
using IWshRuntimeLibrary;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.ComponentModel;
using TKUtils;
using Path = System.IO.Path;
using File = System.IO.File;
using System.Configuration;
using System.Data;
using Microsoft.Win32;
#endregion using directives

namespace TrayLauncher
{

    public partial class MainWindow : Window
    {
        private static bool firstRun;
        public static bool altRows;
        private string xmlMenuFile;
        private MenuList XmlData;

        #region Icon filename variables
        //private readonly string blueIcon = "pack://application:,,,/Images/blue.ico";
        //private readonly string blackIcon = "pack://application:,,,/Images/black.ico";
        //private readonly string cyanIcon = "pack://application:,,,/Images/cyan.ico";
        //private readonly string greenIcon = "pack://application:,,,/Images/green.ico";
        //private readonly string orangeIcon = "pack://application:,,,/Images/orange.ico";
        //private readonly string redIcon = "pack://application:,,,/Images/red.ico";
        //private readonly string whiteIcon = "pack://application:,,,/Images/white.ico";
        //private readonly string yellowIcon = "pack://application:,,,/Images/yellow.ico";

        private readonly string blueIcon = @"Images\blue.ico";
        private readonly string blackIcon = @"Images\black.ico";
        private readonly string cyanIcon = @"Images\cyan.ico";
        private readonly string greenIcon = @"Images\green.ico";
        private readonly string orangeIcon = @"Images\orange.ico";
        private readonly string redIcon = @"Images\red.ico";
        private readonly string whiteIcon = @"Images\white.ico";
        private readonly string yellowIcon = @"Images\yellow.ico";
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            ReadSettings();

            LoadMenuDefaultItems();

            ConstructMenu();

            // trayMenu.Cursor = Cursors.Arrow;
        }


        ////////////////////////////// XML file methods //////////////////////////////

        #region Get the menu XML file
        private string GetXmlFile()
        {
            try
            {
                // This is the path where our user.config file is located
                string config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;

                // Get the parent directory
                string parent = Directory.GetParent(config).ToString();

                // Append the filename
                string menuFile = Path.Combine(Path.GetDirectoryName(parent), "MenuItems.xml");

                // If the menu file doesn't exist, create a new one and seed it with one record
                if (!File.Exists(menuFile))
                {
                    File.Create(menuFile).Dispose();
                    CreateXMLStarter(menuFile);
                }

                // Save the path to setting
                Properties.Settings.Default.XMLfile = menuFile;
                return menuFile;
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading or writing to menu file\n{ex.Message}",
                                     "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile($"Error reading or writing to menu file.");
                WriteLog.WriteTempFile(ex.Message);

                return "ERROR";
            }
        }
        #endregion

        #region Read & Sort the XML file
        private void SortXMLFile()
        {
            try
            {
                // Read the XML menu file
                XElement root = XElement.Load(xmlMenuFile);

                // Sort XML file
                WriteLog.WriteTempFile("  Sorting XML file");
                XElement[] menuList = root.Elements("TLMenuItem")
                                          .OrderBy(m => (int)m.Element("Position"))
                                          .ToArray();
                root.RemoveAll();
                foreach (XElement menuItem in menuList)
                {
                    root.Add(menuItem);
                }
                root.Save(xmlMenuFile);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading or writing to menu file\n{ex.Message}",
                    "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile($"Error reading or writing to menu file.");
                WriteLog.WriteTempFile(ex.Message);

            }
        }
        #endregion

        #region Refresh (reread, sort  and renumber)
        private void RefreshMenu()
        {
            // Reread file
            try
            {
                WriteLog.WriteTempFile("  Renumbering positions");
                XElement root = XElement.Load(xmlMenuFile);

                // Sort the file
                XElement[] menuList = root.Elements("TLMenuItem")
                                          .OrderBy(m => (int)m.Element("Position"))
                                          .ToArray();
                root.RemoveAll();

                // Renumber by 10 starting at 100
                int x = 100;
                foreach (XElement menuItem in menuList)
                {
                    menuItem.Element("Position").Value = x.ToString();
                    root.Add(menuItem);
                    x += 10;
                }

                // FInally save the file
                root.Save(xmlMenuFile);

            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading or writing to menu file\n{ex.Message}",
                    "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile($"Error reading or writing to menu file.");
                WriteLog.WriteTempFile(ex.Message);
            }
        }
        #endregion

        #region Create XML Starter
        private void CreateXMLStarter(string menuFile)
        {
            WriteLog.WriteTempFile("  Creating starter menu");
            DateTime dt = DateTime.Now;
            string comment = $"File created {dt}";

            try
            {
                XDocument xDoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XComment(comment),
                    new XElement("MenuList",
                        new XElement("TLMenuItem",
                            new XElement("Position", "0"),
                            new XElement("MenuHeader", "Separator"),
                            new XElement("AppPath", "Not applicable"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a separator")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "100"),
                            new XElement("MenuHeader", "Command Prompt"),
                            new XElement("AppPath", @"C:\Windows\System32\Cmd.exe"),
                            new XElement("Arguments", "/k Echo Hello from TrayLauncher!"),
                            new XElement("ToolTip", "This is an example menu item with arguments")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "200"),
                            new XElement("MenuHeader", "Sums and minuses"),
                            new XElement("AppPath", "Calc.exe"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "This is an app example")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "300"),
                            new XElement("MenuHeader", "Windows Folder"),
                            new XElement("AppPath", "Explorer.exe"),
                            new XElement("Arguments", "%WINDIR%"),
                            new XElement("ToolTip", "This is a folder example")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "400"),
                            new XElement("MenuHeader", "Separator"),
                            new XElement("AppPath", "Not applicable"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a separator")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "500"),
                            new XElement("MenuHeader", "Search for Puppies"),
                            new XElement("AppPath", "www.bing.com/images/search?q=puppies"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "It works for websites too")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "600"),
                            new XElement("MenuHeader", "Separator"),
                            new XElement("AppPath", "Not applicable"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a separator")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "700"),
                            new XElement("MenuHeader", "Taskbar Settings"),
                            new XElement("AppPath", "ms-settings:taskbar"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "This is a settings example")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "900"),
                            new XElement("MenuHeader", "Separator"),
                            new XElement("AppPath", "Not applicable"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a separator"))
                        ));
                xDoc.Save(menuFile);
            }

            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error writing to menu file\n{ex.Message}",
                                     "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile($"Error writing to menu file.");
                WriteLog.WriteTempFile(ex.Message);
            }
        }
        #endregion

        //////////////////////////// Menu & Datagrid methods ////////////////////////

        #region Load menu items
        private void LoadMenuItems(List<TLMenuItem> sortedList)
        {
            int x = 0;

            foreach (var item in sortedList)
            {
                if (string.IsNullOrEmpty(item.Header))
                {
                    Debug.WriteLine($"I didn't like item {x}");
                    WriteLog.WriteTempFile($"  NOT Loaded: null item {x}");
                    continue;
                }

                x++;
                MenuItem mi = new MenuItem();

                // If menu item is a separator
                if (item.Header.PadRight(9).Substring(0, 9).ToUpper() == "SEPARATOR")
                {
                    Debug.WriteLine("Adding a separator");
                    Color selectedColor = (Color)(CmbSeparator.SelectedItem as PropertyInfo).
                        GetValue(null, null);

                    Separator sep = new Separator
                    {
                        BorderBrush = new SolidColorBrush(selectedColor),
                        BorderThickness = new Thickness(0, 1, 0, 0)
                    };
                    trayMenu.Items.Insert(x, sep);
                    WriteLog.WriteTempFile($"  Loaded: Separator, item {x}");
                }
                else
                // otherwise
                {
                    mi.Header = item.Header;
                    if (!string.IsNullOrEmpty(item.ToolTip))
                    {
                        mi.ToolTip = $"{item.ToolTip}";
                    }
                    mi.Click += Mi_Click;
                    mi.Tag = item;
                    Debug.WriteLine($"Adding item {item.Header}");
                    trayMenu.Items.Insert(x, mi);
                    WriteLog.WriteTempFile($"  Loaded: {item.Header}, item {x}");
                }
            }
            WriteLog.WriteTempFile($"  Load menu items is complete");
        }
        #endregion

        #region Load Datagrid
        private List<TLMenuItem> LoadDataGrid()
        {
            //Sort the list for the data grid
            List<TLMenuItem> sortedList = XmlData.menuList.OrderBy(z => z.Pos).ToList();

            // This is equivalent to the line above
            //var query = from MenuList in XmlData.menuList
            //            orderby MenuList.Pos
            //            select MenuList;
            //List<TLMenuItem> sortedList = query.ToList();

            theDataGrid.ItemsSource = sortedList;
            return sortedList;
        }
        #endregion

        #region Load Menu Default Items
        private void LoadMenuDefaultItems()
        {
            WriteLog.WriteTempFile("Entering LoadMenuDefaultItems");

            //MenuItem menuTest = new MenuItem
            //{
            //    Header = "TrayLauncher",
            //    Name = "mnuTest",
            //    FontStyle = FontStyles.Italic,
            //    //FontWeight = FontWeights.Bold,
            //};
            //menuTest.IsHitTestVisible = false;
            //menuTest.Margin = new Thickness(-20, 0, 0, 0);
            //_ = trayMenu.Items.Add(menuTest);

            MenuItem menuManage = new MenuItem
            {
                Header = "Manage",
                Name = "mnuManage",
                FontWeight = FontWeights.Bold,
                ToolTip = "Manage TrayLauncher"
            };
            menuManage.Click += TbcmRestore_Click;
            _ = trayMenu.Items.Add(menuManage);
            WriteLog.WriteTempFile("  Loaded: Manage");

            MenuItem menuExit = new MenuItem
            {
                Header = "Exit",
                Name = "mnuExit",
                FontWeight = FontWeights.Normal,
                ToolTip = "Exit TrayLauncher"
            };
            menuExit.Click += TbcmExit_Click;
            _ = trayMenu.Items.Add(menuExit);
            WriteLog.WriteTempFile("  Loaded: Exit");
            WriteLog.WriteTempFile("Leaving LoadMenuDefaultItems");
        }
        #endregion

        #region Construct the menu
        public void ConstructMenu()
        {
            WriteLog.WriteTempFile("Entering ConstructMenu");
            if (!File.Exists(xmlMenuFile))
            {
                _ = MessageBox.Show("The menu XML file cannot be found.",
                    "ERROR", MessageBoxButton.OK, MessageBoxImage.Error); ;
                WriteLog.WriteTempFile("The menu XML file cannot be found.");
                Show();
            }
            else
            {
                try
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(MenuList));
                    TextReader reader = new StreamReader(xmlMenuFile);
                    object obj = deserializer.Deserialize(reader);
                    XmlData = (MenuList)obj;
                    reader.Close();
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Failed to read menu XML file\n\n{ex.Message}\n\nUnable to continue.",
                        "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    WriteLog.WriteTempFile($"Unable to read {xmlMenuFile}");
                    WriteLog.WriteTempFile(ex.Message);
                    WriteLog.WriteTempFile($"Unable to continue. Shutting down.");

                    Environment.Exit(-1);
                }
                List<TLMenuItem> sortedList = LoadDataGrid();
                LoadMenuItems(sortedList);
            }
            WriteLog.WriteTempFile("Leaving ConstructMenu");
            WriteLog.WriteTempFile("TrayLauncher is Ready");
        }
        #endregion

        ///////////////////////////// NotifyIcon events /////////////////////////////

        #region Notify icon events
        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }
        #endregion

        ////////////////////////////// Tray menu events /////////////////////////////

        #region Tray Menu events
        private void TbcmExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TbcmRestore_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }
        #endregion Tray Menu events

        ////////////////////////////// Window events ////////////////////////////////

        #region Window events
        // Window closing
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //clean up notify icon (would otherwise stay open until application finishes)
            myNotifyIcon.Dispose();

            base.OnClosing(e);
        }

        // Hide window when minimized
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
        }

        // Save window position and other settings, write current path to file at shutdown
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.WindowLeft = this.Left;
            Properties.Settings.Default.WindowTop = this.Top;

            // save the property settings
            Properties.Settings.Default.Save();
            WriteLog.WriteTempFile("Shutting down.");
        }
        #endregion

        ////////////////////////////// Datagrid events //////////////////////////////

        #region Datagrid events
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mnuUpdate.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }
        #endregion

        ////////////////////////////// Menu events //////////////////////////////////

        #region Menu click handler  <- this guy does the launching
        // Menu click event handler
        private void Mi_Click(object sender, RoutedEventArgs e)
        {
            MenuItem m = e.Source as MenuItem;

            using (Process launch = new Process())
            {
                TLMenuItem x = (TLMenuItem)m.Tag;
                if (string.IsNullOrEmpty(x.AppPath))
                {
                    myNotifyIcon.ShowBalloonTip("Launch Error",
                        $"{x.Header}\nAppPath cannot be empty",
                        BalloonIcon.Warning);
                    WriteLog.WriteTempFile($"* Failed to launch {x.Header} ");
                    WriteLog.WriteTempFile($"* AppPath is empty.");
                }
                else
                {
                    try
                    {
                        launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(x.AppPath);
                        launch.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(x.Arguments);
                        launch.Start();
                        WriteLog.WriteTempFile($"Launching: {x.Header}," +
                                               $" Position: {x.Pos}," +
                                               $" AppPath: {x.AppPath}," +
                                               $" Arguments: {x.Arguments}," +
                                               $" ToolTip: {x.ToolTip}");
                    }
                    catch (Exception ex)
                    {
                        myNotifyIcon.ShowBalloonTip("Launch Error",
                            $"Failed to launch {x.Header} - {x.AppPath} - {x.Arguments}\n{ex.Message}",
                            BalloonIcon.Error);
                        WriteLog.WriteTempFile($"* Failed to launch {x.Header} -  {x.AppPath}" +
                            $" - {x.Arguments}");
                        WriteLog.WriteTempFile($"* {ex.Message}");
                    }
                }
            }
            e.Handled = true;
        }
        #endregion

        #region Menu events
        ////////////////////////////// File menu //////////////////////////////////
        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        ////////////////////////////// Menu maintenance ///////////////////////////
        private void MnuAdd_Click(object sender, RoutedEventArgs e)
        {
            AddItem add = new AddItem
            {
                Owner = Application.Current.MainWindow
            };
            add.ShowDialog();
            SortXMLFile();
            trayMenu.Items.Clear();
            LoadMenuDefaultItems();
            ConstructMenu();
        }

        private void MnuDel_Click(object sender, RoutedEventArgs e)
        {
            if (theDataGrid.SelectedItem != null)
            {
                SortXMLFile();
                try
                {
                    XDocument xDoc = XDocument.Load(xmlMenuFile);
                    var item = xDoc.Descendants("TLMenuItem").ElementAt(theDataGrid.SelectedIndex);
                    WriteLog.WriteTempFile($"  Removing menu item {item.Element("MenuHeader").Value} ");
                    item.Remove();
                    xDoc.Save(xmlMenuFile);
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Error reading or writing to menu file\n{ex.Message}",
                        "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    WriteLog.WriteTempFile($"Error reading or writing to menu file.");
                    WriteLog.WriteTempFile(ex.Message);
                }
                trayMenu.Items.Clear();
                LoadMenuDefaultItems();
                ConstructMenu();
            }
        }

        private void MnuUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (theDataGrid.SelectedItem != null)
            {
                TLMenuItem x = (TLMenuItem)theDataGrid.SelectedItem;
                string header = x.Header;
                string appPath = x.AppPath;
                string args = x.Arguments;
                string ttip = x.ToolTip;
                int pos = x.Pos;
                int index = theDataGrid.SelectedIndex;

                UpdateItem update = new UpdateItem(header, appPath, args, ttip, pos, index)
                {
                    Owner = this
                };
                update.ShowDialog();
                SortXMLFile();
                trayMenu.Items.Clear();
                LoadMenuDefaultItems();
                ConstructMenu();
            }
        }

        private void MnuStartWW_Checked(object sender, RoutedEventArgs e)
        {
            StartupShortcut("Create");
            Properties.Settings.Default.StartWithWindows = true;
        }

        private void MnuStartWW_Unchecked(object sender, RoutedEventArgs e)
        {
            StartupShortcut("Delete");
            Properties.Settings.Default.StartWithWindows = true;
        }

        private void MnuHideOnStart_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.HideOnStart = true;
            WriteLog.WriteTempFile($"  Hide on startup set to True");
        }

        private void MnuHideOnStart_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.HideOnStart = false;
            WriteLog.WriteTempFile($"  Hide on startup set to False");
        }

        private void CmbBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Color selectedColor = (Color)(CmbBackground.SelectedItem as PropertyInfo).
                GetValue(null, null);
            SetMenuBackground(selectedColor);
            tbColorSample.Background = new SolidColorBrush(selectedColor);
            Properties.Settings.Default.BackColor = CmbBackground.SelectedIndex;
            if (Visibility == Visibility.Visible)
            {
                mnuBackColor.IsSubmenuOpen = false;
            }
            Debug.WriteLine($"Background SelectedIndex is {CmbBackground.SelectedIndex}");
            WriteLog.WriteTempFile($"  Menu background color is {CmbBackground.SelectedItem}, " +
                                   $"index is {CmbBackground.SelectedIndex}");
        }

        private void CmbForeground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Color selectedColor = (Color)(CmbForeground.SelectedItem as PropertyInfo).
                GetValue(null, null);
            trayMenu.Foreground = new SolidColorBrush(selectedColor);

            tbColorSample.Foreground = new SolidColorBrush(selectedColor);
            Properties.Settings.Default.ForeColor = CmbForeground.SelectedIndex;

            if (Visibility == Visibility.Visible)
            {
                mnuTextColor.IsSubmenuOpen = false;
            }

            Debug.WriteLine($"Foreground SelectedIndex is {CmbForeground.SelectedIndex}");
            WriteLog.WriteTempFile($"  Menu text color is {CmbForeground.SelectedItem.ToString()}, " +
                                   $"index is {CmbForeground.SelectedIndex}");
        }

        private void CmbSeparator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SeparatorColor = CmbSeparator.SelectedIndex;
            if (Visibility == Visibility.Visible)
            {
                trayMenu.Items.Clear();
                LoadMenuDefaultItems();
                SortXMLFile();
                ConstructMenu();
                mnuSepColor.IsSubmenuOpen = false;
            }
            Debug.WriteLine($"Separator SelectedIndex is {CmbSeparator.SelectedIndex}");
            WriteLog.WriteTempFile($"  Separator color is {CmbSeparator.SelectedItem.ToString()}, " +
                                   $"index is {CmbSeparator.SelectedIndex}");
        }

        private void MnuStartNotify_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartNotification = true;
            WriteLog.WriteTempFile($"  Startup notification set to True");
        }

        private void MnuStartNotify_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartNotification = false;
            WriteLog.WriteTempFile($"  Startup notification set to False");
        }

        private void MnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("  Refreshing menu");
            trayMenu.Items.Clear();
            LoadMenuDefaultItems();
            RefreshMenu();
            ConstructMenu();
            WriteLog.WriteTempFile($"  Menu refreshed");
        }

        private void MnuTest_Click(object sender, RoutedEventArgs e)
        {
            if (theDataGrid.SelectedItem != null)
            {
                using (Process launch = new Process())
                {
                    TLMenuItem x = (TLMenuItem)theDataGrid.SelectedItem;
                    if (string.IsNullOrEmpty(x.AppPath))
                    {
                        myNotifyIcon.ShowBalloonTip("Launch Error",
                            $"{x.Header}\nAppPath cannot be empty",
                            BalloonIcon.Warning);
                        WriteLog.WriteTempFile($"* Failed to launch {x.Header} ");
                        WriteLog.WriteTempFile($"* AppPath is empty.");
                    }
                    else if (x.Header == "Separator")
                    {
                        myNotifyIcon.ShowBalloonTip("Test Launch Error",
                            $"Cannot test a separator",
                            BalloonIcon.Info);
                    }
                    else
                    {
                        try
                        {
                            launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(x.AppPath);
                            launch.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(x.Arguments);
                            launch.Start();
                            WriteLog.WriteTempFile($"Launching: {x.Header}," +
                                                   $" Position: {x.Pos}," +
                                                   $" AppPath: {x.AppPath}," +
                                                   $" Arguments: {x.Arguments}," +
                                                   $" ToolTip: {x.ToolTip}");
                        }
                        catch (Exception ex)
                        {
                            myNotifyIcon.ShowBalloonTip("Test Launch Error",
                                $"Failed to launch \"{x.Header} - {x.AppPath}\"\n{ex.Message}",
                                BalloonIcon.Error);
                            WriteLog.WriteTempFile($"* Failed to launch {x.Header} -  {x.AppPath}" +
                                $" - {x.Arguments} ");
                            WriteLog.WriteTempFile($"* {ex.Message}");
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private void MnuBackupXML_Click(object sender, RoutedEventArgs e)
        {
            BackupXMLFile();
        }

        ////////////////////////////// Options menu ///////////////////////////////
        private void MnuFontIncrease_Click(object sender, RoutedEventArgs e)
        {
            FontLarger();
        }

        private void MnuFontDecrease_Click(object sender, RoutedEventArgs e)
        {
            FontSmaller();
        }

        private void MnuBlueIco_Click(object sender, RoutedEventArgs e)
        {
            IconFromFile(blueIcon);
            Properties.Settings.Default.Icon = "blue";
        }

        private void MnuBlackIco_Click(object sender, RoutedEventArgs e)
        {
            IconFromFile(blackIcon);
            Properties.Settings.Default.Icon = "black";
        }

        private void MnuCyanIco_Click(object sender, RoutedEventArgs e)
        {
            IconFromFile(cyanIcon);
            Properties.Settings.Default.Icon = "cyan";
        }

        private void MnuGreenIco_Click(object sender, RoutedEventArgs e)
        {
            IconFromFile(greenIcon);
            Properties.Settings.Default.Icon = "green";
        }

        private void MnuOrangeIco_Click(object sender, RoutedEventArgs e)
        {
            IconFromFile(orangeIcon);
            Properties.Settings.Default.Icon = "orange";
        }

        private void MnuRedIco_Click(object sender, RoutedEventArgs e)
        {
            IconFromFile(redIcon);
            Properties.Settings.Default.Icon = "red";
        }

        private void MnuWhiteIco_Click(object sender, RoutedEventArgs e)
        {
            IconFromFile(whiteIcon);
            Properties.Settings.Default.Icon = "white";
        }

        private void MnuYellowIco_Click(object sender, RoutedEventArgs e)
        {
            IconFromFile(yellowIcon);
            Properties.Settings.Default.Icon = "yellow";
        }

        ////////////////////////////// Help menu //////////////////////////////////
        private void MnuReadme_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(@".\ReadMe.txt");
        }

        private void MnuAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            _ = about.ShowDialog();
        }

        #endregion Menu events

        ////////////////////////////// Keyboard events //////////////////////////////

        #region Keyboard events
        // Handle keyboard events
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            //F1 = About
            if (e.Key == Key.F1 )
            {
                mnuAbout.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            }

            //F2 = Show settings
            if (e.Key == Key.F2 && (e.KeyboardDevice.Modifiers == ModifierKeys.None))
            {
                string settingsMsg = $"Current Settings:" +
                                     $"First Run = {Properties.Settings.Default.FirstRun}\n" +
                                     $"Window Top = {Properties.Settings.Default.WindowTop}\n" +
                                     $"Window Left = {Properties.Settings.Default.WindowLeft}\n" +
                                     $"Start with Windows = {Properties.Settings.Default.StartWithWindows}\n" +
                                     $"Startup Notification = {Properties.Settings.Default.StartNotification}\n" +
                                     $"Hide Main Window on Startup = {Properties.Settings.Default.HideOnStart}\n" +
                                     $"Background Color Index = {Properties.Settings.Default.BackColor}\n" +
                                     $"Menu Text Color Index = { Properties.Settings.Default.ForeColor}\n" +
                                     $"Separator Color Index = {Properties.Settings.Default.SeparatorColor}\n" +
                                     $"Font Size = {Properties.Settings.Default.FontSize}\n" +
                                     $"Icon Color = {Properties.Settings.Default.Icon}\n" +
                                     $"Menu Filename = {Properties.Settings.Default.XMLfile}\n" +
                                     $"";
                MessageBox.Show(settingsMsg, "Settings");
            }

            // F3 = View temp file
            if (e.Key == Key.F3)
            {
                if (File.Exists(xmlMenuFile))
                {
                    try
                    {
                        using (Process fileExp = new Process())
                        {
                            fileExp.StartInfo.FileName = xmlMenuFile;
                            fileExp.StartInfo.UseShellExecute = true;
                            fileExp.StartInfo.ErrorDialog = false;
                            fileExp.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show($"Unable to start default application used to open" +
                            $" {xmlMenuFile}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        WriteLog.WriteTempFile($"Unable to open {xmlMenuFile}");
                        WriteLog.WriteTempFile(ex.Message);
                    }
                }
            }

            // F4 = View temp file
            if (e.Key == Key.F4)
            {
                string path = WriteLog.GetTempFile();
                if (File.Exists(path))
                {
                    try
                    {
                        using (Process fileExp = new Process())
                        {
                            fileExp.StartInfo.FileName = path;
                            fileExp.StartInfo.UseShellExecute = true;
                            fileExp.StartInfo.ErrorDialog = false;
                            fileExp.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show($"Unable to start default application used to open" +
                            $" {path}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        WriteLog.WriteTempFile($"Unable to open {path}");
                        WriteLog.WriteTempFile(ex.Message);
                    }
                }
            }

            // F5 = Refresh
            if (e.Key == Key.F5)
            {
                mnuRefresh.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            }

            // Number Pad + and - change font size
            if (e.Key == Key.Add)
            {
                if (Keyboard.Modifiers != ModifierKeys.Control)
                    return;
                FontLarger();
            }
            if (e.Key == Key.Subtract)
            {
                if (Keyboard.Modifiers != ModifierKeys.Control)
                    return;
                FontSmaller();
            }

            if (e.Key == Key.F7)
            {
                if (altRows)
                {
                    AltRowShadingOff();
                    altRows = false;
                }
                else
                {
                    AltRowShadingOn();
                    altRows = true;
                }

            }
        }
            #endregion Keyboard events

        ////////////////////////////// Helper Methods ///////////////////////////////

        #region Helper methods

        #region Settings file

        private void ReadSettings()
        {
            WriteLog.WriteTempFile(" ");
            WriteLog.WriteTempFile("TrayLauncher is starting up");
            WriteLog.WriteTempFile("Entering ReadSettings");

            // Settings upgrade if needed
            if (Properties.Settings.Default.SettingsUpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.SettingsUpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            // Put version number in title bar
            WindowTitleVersion();

            // Background color - all except Transparent
            CmbBackground.SelectedIndex = Properties.Settings.Default.BackColor;
            var x = typeof(Colors).GetProperties();
            List<PropertyInfo> bkColor = new List<PropertyInfo>();
            foreach (var item in x)
            {
                if (item.Name.ToLower() == "transparent")
                {
                    continue;
                }
                bkColor.Add(item);
            }
            CmbBackground.ItemsSource = bkColor;
            //CmbBackground.ItemsSource = typeof(Colors).GetProperties();

            // Menu text color
            CmbForeground.SelectedIndex = Properties.Settings.Default.ForeColor;
            CmbForeground.ItemsSource = typeof(Colors).GetProperties();
            Color selectedColor = (Color)(CmbForeground.SelectedItem as PropertyInfo)
                .GetValue(null, null);
            trayMenu.Foreground = new SolidColorBrush(selectedColor);

            // Menu separator color
            CmbSeparator.SelectedIndex = Properties.Settings.Default.SeparatorColor;
            CmbSeparator.ItemsSource = typeof(Colors).GetProperties();

            // XML menu filename
            xmlMenuFile = GetXmlFile();
            WriteLog.WriteTempFile($"  XML menu file is: {xmlMenuFile}");

            // Start with Windows
            if (Properties.Settings.Default.StartWithWindows == true)
            {
                mnuStartWW.IsChecked = true;
            }
            else
            {
                mnuStartWW.IsChecked = false;
            }
            WriteLog.WriteTempFile($"  Start with Windows is {mnuStartWW.IsChecked}");

            // Startup Notification
            if (Properties.Settings.Default.StartNotification == true)
            {
                myNotifyIcon.ShowBalloonTip("TrayLauncher is Running",
                    "Right-click for launch menu", BalloonIcon.Info);
                mnuStartNotify.IsChecked = true;
            }
            else
            {
                mnuStartNotify.IsChecked = false;
            }
            WriteLog.WriteTempFile($"  Startup notification is {mnuStartNotify.IsChecked}");

            // Datagrid font size
            theDataGrid.FontSize = Properties.Settings.Default.FontSize;
            trayMenu.FontSize = Properties.Settings.Default.FontSize;

            // Icon file
            string iconFile = Properties.Settings.Default.Icon;
            switch (iconFile.ToLower())
            {
                case "blue":
                    {
                        IconFromFile(blueIcon);
                        break;
                    }
                case "black":
                    {
                        IconFromFile(blackIcon);
                        break;
                    }
                case "cyan":
                    {
                        IconFromFile(cyanIcon);
                        break;
                    }
                case "green":
                    {
                        IconFromFile(greenIcon);
                        break;
                    }
                case "red":
                    {
                        IconFromFile(redIcon);
                        break;
                    }
                case "white":
                    {
                        IconFromFile(whiteIcon);
                        break;
                    }
                case "yellow":
                    {
                        IconFromFile(yellowIcon);
                        break;
                    }
                default:
                    {
                        IconFromFile(orangeIcon);
                        break;
                    }
            }
            WriteLog.WriteTempFile($"  Icon color is {iconFile}");

            // Hide main window on startup
            if (Properties.Settings.Default.HideOnStart == true)
            {
                Hide();
                mnuHideOnStart.IsChecked = true;
                WriteLog.WriteTempFile($"  Main window is hidden on startup");
            }
            else
            {
                mnuHideOnStart.IsChecked = false;
                Show();
                WriteLog.WriteTempFile($"  Main window is shown on startup");
            }

            // First time?
            firstRun = Properties.Settings.Default.FirstRun;
            if (firstRun)
            {
                Properties.Settings.Default.FirstRun = false;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                this.Show();
            }
            else
            {
                // Window position
                this.Top = Properties.Settings.Default.WindowTop;
                this.Left = Properties.Settings.Default.WindowLeft;
            }
            WriteLog.WriteTempFile($"  FirstRun is {firstRun}");

            // Alternate row shading
            if (Properties.Settings.Default.ShadeAltRows == true)
            {
                altRows = true;
                AltRowShadingOn();
            }
            else
            {
                altRows = false;
                AltRowShadingOff();
            }
            WriteLog.WriteTempFile($"  ShadeAltRows is {altRows}");

            WriteLog.WriteTempFile("Leaving ReadSettings");
        }
        #endregion Settings file

        #region Title version
        public void WindowTitleVersion()
        {
            // Get the assembly version
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string myExe = Assembly.GetExecutingAssembly().GetName().Name;

            // Remove the release (last) node
            string titleVer = version.ToString().Remove(version.ToString().LastIndexOf("."));

            // Set the windows title
            this.Title = String.Format($"{myExe} -v{titleVer}");

            WriteLog.WriteTempFile($"  Version is {titleVer}");
        }
        #endregion Title version

        #region Startup Shortcut
        // This will create/delete a shortcut in the users Startup folder
        private static void StartupShortcut(string mode)
        {
            string startupfolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string myName = Process.GetCurrentProcess().ProcessName;
            string myExe = Assembly.GetEntryAssembly().Location;
            string shortcutPath = System.IO.Path.Combine(startupfolder, myName + ".lnk");

            if (mode.ToLower() == "create")
            {
                if (!File.Exists(shortcutPath))
                {
                    try
                    {
                        // WshShell requires a Reference and using statement
                        WshShell shell = new WshShell();
                        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                        shortcut.Description = "Shortcut for TrayLauncher";
                        shortcut.TargetPath = myExe;
                        //shortcut.Hotkey =
                        //shortcut.WorkingDirectory =
                        //shortcut.IconLocation =
                        shortcut.Save();
                        WriteLog.WriteTempFile($"Shortcut saved to {shortcutPath}");
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show($"Error creating shortcut\n{ex.Message}",
                                                "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        WriteLog.WriteTempFile($"Error creating shortcut.");
                        WriteLog.WriteTempFile(ex.Message);
                    }
                }
            }

            if (mode.ToLower() == "delete")
            {
                if (File.Exists(shortcutPath))
                {
                    try
                    {
                        File.Delete(shortcutPath);
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show($"Error deleting shortcut\n{ex.Message}",
                                                "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        WriteLog.WriteTempFile($"Error deleting shortcut.");
                        WriteLog.WriteTempFile(ex.Message);
                    }
                }
                WriteLog.WriteTempFile($"Shortcut removed from {shortcutPath}");
            }
        }
        #endregion

        #region Get icon from Images folder
        public void IconFromFile(string iconFile)
        {
            string myPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string myIcon = Path.Combine(myPath, iconFile);

            if (File.Exists(myIcon))
            {
                try
                {
                    myNotifyIcon.Icon = new System.Drawing.Icon(myIcon);
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"{myIcon} is not a valid icon file.\n\n{ex.Message}", "ERROR",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            //string myIconFile = Path.Combine(myIcons, iconFile);

            //Uri iconUri = new Uri(myUri, UriKind.RelativeOrAbsolute);
            //this.Icon = BitmapFrame.Create(iconUri);

            //myNotifyIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(
            //    new Uri(myUri)).Stream);
            //myNotifyIcon.Icon = new System.Drawing.Icon(myIcon);
        }
        #endregion

        #region Set Menu background color
        public void SetMenuBackground(Color selectedColor)
        {
            // from https://www.dotnetcurry.com/wpf/1142/resources-wpf-static-dynamic-difference

            SolidColorBrush findsolidColorBrush = FindResource("brush1") as SolidColorBrush;
            findsolidColorBrush.Color = selectedColor;
            Resources.Remove("brush1");
            Resources.Add("brush1", findsolidColorBrush);
        }
        #endregion

        #region Font size
        // Increase / Decrease font size
        private void FontSmaller()
        {
            if (trayMenu.FontSize > 10)
            {
                trayMenu.FontSize -= 1;
                theDataGrid.FontSize -= 1;
                theDataGrid.RowHeight -= 2;
                Properties.Settings.Default.FontSize = trayMenu.FontSize;
                Debug.WriteLine($"Font size is {trayMenu.FontSize}");
                WriteLog.WriteTempFile($"  Font size set to {trayMenu.FontSize}");
            }
        }

        private void FontLarger()
        {
            if (trayMenu.FontSize < 18)
            {
                trayMenu.FontSize += 1;
                theDataGrid.FontSize += 1;
                theDataGrid.RowHeight += 2;
                Properties.Settings.Default.FontSize = trayMenu.FontSize;
                Debug.WriteLine($"Font size is {trayMenu.FontSize}");
                WriteLog.WriteTempFile($"  Font size set to {trayMenu.FontSize}");
            }
        }
        #endregion

        #region Backup XML data
        private void BackupXMLFile()
        {
            SaveFileDialog dlgSave = new SaveFileDialog
            {
                Title = "Choose backup location",
                CheckPathExists = true,
                CheckFileExists = false,
                FileName = "TrayLauncher_Menu_backup.xml",
                Filter = "XML (*.xml|*.XML| All files (*.*)|*.*"
            };
            bool? result = dlgSave.ShowDialog();
            if (result == true)
            {
                try
                {
                    File.Copy(xmlMenuFile, dlgSave.FileName);
                    WriteLog.WriteTempFile($"Menu items backed up to {dlgSave.FileName} ");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Backup failed\n{ex.Message}", "ERROR",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    WriteLog.WriteTempFile($"Backup failed.");
                    WriteLog.WriteTempFile(ex.Message);
                }
            }
        }
        #endregion

        #region Alternate row shading
        private void AltRowShadingOff()
        {
            theDataGrid.AlternationCount = 0;
            theDataGrid.RowBackground = new SolidColorBrush(Colors.White);
            theDataGrid.AlternatingRowBackground = new SolidColorBrush(Colors.White);
            theDataGrid.Items.Refresh();
            Properties.Settings.Default.ShadeAltRows = false;
            altRows = false;
        }

        private void AltRowShadingOn()
        {
            theDataGrid.AlternationCount = 1;
            theDataGrid.RowBackground = new SolidColorBrush(Colors.White);
            theDataGrid.AlternatingRowBackground = new SolidColorBrush(Colors.WhiteSmoke);
            theDataGrid.Items.Refresh();
            Properties.Settings.Default.ShadeAltRows = true;
            altRows = true;
        }
        #endregion Alternate row shading

        #endregion Helper methods
    }
}
