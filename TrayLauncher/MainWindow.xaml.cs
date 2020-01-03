// TrayLauncher - A customizable tray menu to launch applications, websites and folders.
//
// See App.xaml.cs for code that restricts app to one instance
//
//! Build as 64 bit!

#region using directives
using Hardcodet.Wpf.TaskbarNotification;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.Serialization;
using TKUtils;
using File = System.IO.File;
using Path = System.IO.Path;
#endregion using directives

namespace TrayLauncher
{
    public partial class MainWindow : Window
    {
        private static bool firstRun;
        public static bool altRows;
        private string xmlMenuFile;
        private MenuList XmlData;
        private bool explicitClose;

        #region Icon filename variables
        private readonly string blackIcon = @"Images\black.ico";
        private readonly string blue2Icon = @"Images\blue2.ico";
        private readonly string blueIcon = @"Images\blue.ico";
        private readonly string cyanIcon = @"Images\cyan.ico";
        private readonly string grayIcon = @"Images\gray.ico";
        private readonly string greenIcon = @"Images\green.ico";
        private readonly string magentaIcon = @"Images\magenta.ico";
        private readonly string orangeIcon = @"Images\orange.ico";
        private readonly string redIcon = @"Images\red.ico";
        private readonly string tealIcon = @"Images\teal.ico";
        private readonly string whiteIcon = @"Images\white.ico";
        private readonly string yellowIcon = @"Images\yellow.ico";
        #endregion Icon filename variables

        public MainWindow()
        {
            InitializeComponent();

            ReadSettings();

            ConstructMenu();
        }

        ////////////////////////////// XML file methods //////////////////////////////

        #region Get the menu XML file  <- determine path and see if file exists
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
                WriteLog.WriteTempFile($"* Error reading or writing to menu file.");
                WriteLog.WriteTempFile($"* {ex.Message}");

                return "ERROR";
            }
        }
        #endregion Get the menu XML file  <- determine path and see if file exists

        #region Read & Sort the XML file  <- Used when renumbering is not desired
        private void SortXMLFile()
        {
            try
            {
                // Read the XML menu file
                XElement root = XElement.Load(xmlMenuFile);

                // Sort XML file
                WriteLog.WriteTempFile("  Sorting menu file");
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
                WriteLog.WriteTempFile($"* Error reading or writing to menu file.");
                WriteLog.WriteTempFile($"* {ex.Message}");
            }
        }
        #endregion Read & Sort the XML file  <- Used when renumbering is not desired

        #region Refresh  <- reread, sort and renumber
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
                WriteLog.WriteTempFile($"* Error reading or writing to menu file.");
                WriteLog.WriteTempFile($"* {ex.Message}");
            }
        }
        #endregion Refresh  <- reread, sort and renumber

        #region Create XML Starter  <- creates starter menu items and separators
        private void CreateXMLStarter(string menuFile)
        {
            WriteLog.WriteTempFile("    Creating starter menu");
            DateTime dt = DateTime.Now;
            string comment = $"File created {dt}";

            try
            {
                XDocument xDoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XComment(comment),
                    new XElement("MenuList",
                        new XElement("TLMenuItem",
                            new XElement("Position", "100"),
                            new XElement("MenuHeader", "Examples"),
                            new XElement("AppPath", "Section Header"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a section header"),
                            new XElement("Type", "H")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "200"),
                            new XElement("MenuHeader", "Command Prompt"),
                            new XElement("AppPath", @"C:\Windows\System32\Cmd.exe"),
                            new XElement("Arguments", "/k Echo Hello from TrayLauncher!"),
                            new XElement("ToolTip", "This is an example menu item with arguments")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "300"),
                            new XElement("MenuHeader", "Calculator"),
                            new XElement("AppPath", "Calc.exe"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "This is an app example")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "400"),
                            new XElement("MenuHeader", "Windows Folder"),
                            new XElement("AppPath", "Explorer.exe"),
                            new XElement("Arguments", "%WINDIR%"),
                            new XElement("ToolTip", "This is a folder example")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "500"),
                            new XElement("MenuHeader", "Separator"),
                            new XElement("AppPath", "Not applicable"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a separator"),
                            new XElement("Type", "S")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "600"),
                            new XElement("MenuHeader", "Search for Puppies"),
                            new XElement("AppPath", "www.bing.com/images/search?q=puppies"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "It works for websites too")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "700"),
                            new XElement("MenuHeader", "Separator"),
                            new XElement("AppPath", "Not applicable"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a separator"),
                            new XElement("Type", "S")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "800"),
                            new XElement("MenuHeader", "Taskbar Settings"),
                            new XElement("AppPath", "ms-settings:taskbar"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "This is a settings example")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "900"),
                            new XElement("MenuHeader", "Separator"),
                            new XElement("AppPath", "Not applicable"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a separator"),
                            new XElement("Type", "S"))
                        ));
                xDoc.Save(menuFile);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error writing to menu file\n{ex.Message}",
                                     "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile($"* Error writing to menu file.");
                WriteLog.WriteTempFile($"* {ex.Message}");
            }
        }
        #endregion Create XML Starter  <- creates starter menu items and separators

        //////////////////////////// Menu & Datagrid methods ////////////////////////

        #region Load Menu Default Items  <- loads Manage and Exit
        private void LoadMenuDefaultItems()
        {
            WriteLog.WriteTempFile("  Entering LoadMenuDefaultItems");

            // Manage - Top item
            MenuItem menuManage = new MenuItem
            {
                Header = "Manage TrayLauncher",
                Name = "mnuManage",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(-5, 0, 0, 0),
                ToolTip = "Open TrayLauncher main window"
            };
            menuManage.Click += TbcmRestore_Click;
            _ = trayMenu.Items.Add(menuManage);
            WriteLog.WriteTempFile("    Loaded: Default item - Manage");

            // Separator under Manage
            Color selectedColor = (Color)(CmbSeparator.SelectedItem as PropertyInfo).
                GetValue(null, null);
            Separator sep = new Separator
            {
                BorderBrush = new SolidColorBrush(selectedColor),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            _ = trayMenu.Items.Add(sep);
            WriteLog.WriteTempFile($"    Loaded: Default item - Separator");

            // Exit - Bottom item
            MenuItem menuExit = new MenuItem
            {
                Header = "Exit",
                Name = "mnuExit",
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(-5, 0, 0, 0),
                ToolTip = "Exit TrayLauncher"
            };
            menuExit.Click += TbcmExit_Click;
            _ = trayMenu.Items.Add(menuExit);
            WriteLog.WriteTempFile("    Loaded: Default item - Exit");
            WriteLog.WriteTempFile("  Leaving LoadMenuDefaultItems");
        }
        #endregion Load Menu Default Items  <- loads Manage and Exit

        #region Construct the menu  <- reads the XML file then calls LoadDataGrid() & LoadMenuItems()
        public void ConstructMenu()
        {
            WriteLog.WriteTempFile("  Entering ConstructMenu");
            if (!File.Exists(xmlMenuFile))
            {
                _ = MessageBox.Show("The menu XML file cannot be found.",
                    "ERROR", MessageBoxButton.OK, MessageBoxImage.Error); ;
                WriteLog.WriteTempFile("* The menu XML file cannot be found.");
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
                    WriteLog.WriteTempFile("    Menu file read successfully");
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Failed to read menu XML file\n\n{ex.Message}\n\nUnable to continue.",
                        "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    WriteLog.WriteTempFile($"* Unable to read {xmlMenuFile}");
                    WriteLog.WriteTempFile($"* {ex.Message}");
                    WriteLog.WriteTempFile($"* Unable to continue. Shutting down.");

                    Environment.Exit(-1);
                }
                LoadMenuDefaultItems();
                List<TLMenuItem> sortedList = LoadDataGrid();
                LoadMenuItems(sortedList);
            }
            WriteLog.WriteTempFile("  Leaving ConstructMenu");
            WriteLog.WriteTempFile("TrayLauncher is Ready!");
        }
        #endregion Construct the menu  <- reads the XML file then calls LoadDataGrid() & LoadMenuItems()

        #region Load Datagrid  <- sorts the menu items by position then loads the datagrid
        private List<TLMenuItem> LoadDataGrid()
        {
            WriteLog.WriteTempFile("  Entering LoadDataGrid");
            //Sort the list for the data grid
            List<TLMenuItem> sortedList = XmlData.menuList.OrderBy(z => z.Pos).ToList();

                    // This is equivalent to the line above
                    //var query = from MenuList in XmlData.menuList
                    //            orderby MenuList.Pos
                    //            select MenuList;
                    //List<TLMenuItem> sortedList = query.ToList();

            theDataGrid.ItemsSource = sortedList;
            WriteLog.WriteTempFile($"    Loaded {sortedList.Count()} items");
            WriteLog.WriteTempFile("  Leaving LoadDataGrid");
            return sortedList;
        }
        #endregion Load Datagrid  <- sorts the menu items by position then loads the datagrid

        #region Load menu items  <- loads menu items from the list sorted in LoadDataGrid()
        private void LoadMenuItems(List<TLMenuItem> sortedList)
        {
            WriteLog.WriteTempFile("  Entering LoadMenuItems");

            // An offset to the item count is needed to insert the items in the correct location
            int itemCounter = 0;
            int itemOffset = 1;

            foreach (var item in sortedList)
            {
                if (string.IsNullOrEmpty(item.Header))
                {
                    Debug.WriteLine($"I didn't like item {itemCounter}");
                    WriteLog.WriteTempFile($"    NOT Loaded: null item {itemCounter}");
                    continue;
                }
                itemCounter++;
                MenuItem mi = new MenuItem();

                // If menu item is a Separator
                if (item.ItemType == "S")
                {
                    Debug.WriteLine("Adding a separator");
                    Color selectedColor = (Color)(CmbSeparator.SelectedItem as PropertyInfo).
                        GetValue(null, null);

                    Separator sep = new Separator
                    {
                        BorderBrush = new SolidColorBrush(selectedColor),
                        BorderThickness = new Thickness(0, 1, 0, 0)
                    };
                    trayMenu.Items.Insert(itemCounter + itemOffset, sep);
                    string ic = itemCounter.ToString("D2");
                    WriteLog.WriteTempFile($"    Loaded: item {ic}, Separator");
                }

                // If menu item is a Section Header
                else if (item.ItemType == "H")
                {
                    Debug.WriteLine("Adding a section header");
                    mi.Header = item.Header;
                    mi.IsHitTestVisible = false;
                    mi.Margin = new Thickness(-5, 0, 0, 0);
                    Color selectedColor = (Color)(CmbHeader.SelectedItem as PropertyInfo).
                        GetValue(null, null);
                    mi.Foreground = new SolidColorBrush(selectedColor);
                    if (Properties.Settings.Default.SectionHeaderBold)
                    {
                        mi.FontWeight = FontWeights.Bold;
                    }
                    trayMenu.Items.Insert(itemCounter + itemOffset, mi);
                    string ic = itemCounter.ToString("D2");
                    WriteLog.WriteTempFile($"    Loaded: item {ic}, Section header");
                }
                else

                // otherwise a normal menu item
                {
                    mi.Header = item.Header;
                    if (!string.IsNullOrEmpty(item.ToolTip))
                    {
                        mi.ToolTip = $"{item.ToolTip}";
                    }
                    mi.Click += Mi_Click;
                    mi.Tag = item;
                    mi.Margin = new Thickness(5, 0, 0, 0);
                    Debug.WriteLine($"Adding item {item.Header}");
                    trayMenu.Items.Insert(itemCounter + itemOffset, mi);
                    string ic = itemCounter.ToString("D2");
                    WriteLog.WriteTempFile($"    Loaded: item {ic}, {item.Header}");
                }
            }
            WriteLog.WriteTempFile($"    Loading of menu items is complete");
            WriteLog.WriteTempFile("  Leaving LoadMenuItems");
        }
        #endregion Load menu items  <- loads menu items from list sorted in LoadDataGrid()

        #region Remove an item
        // Removes item based on index in datagrid
        private void RemoveItem()
        {
            WriteLog.WriteTempFile("  Entering RemoveItem");
            try
            {
                XDocument xDoc = XDocument.Load(xmlMenuFile);
                var item = xDoc.Descendants("TLMenuItem").ElementAt(theDataGrid.SelectedIndex);
                WriteLog.WriteTempFile($"    Removing menu item {item.Element("MenuHeader").Value} ");
                item.Remove();
                xDoc.Save(xmlMenuFile);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading or writing to menu file\n{ex.Message}",
                    "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile($"* Error reading or writing to menu file.");
                WriteLog.WriteTempFile($"* {ex.Message}");
            }
            WriteLog.WriteTempFile("  Leaving RemoveItem");
        }
        #endregion Remove an item

        ///////////////////////////// NotifyIcon events /////////////////////////////

        #region Notify icon events

        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        #endregion Notify icon events

        ////////////////////////////// Tray menu events /////////////////////////////

        #region Tray Menu events
        // Set explicit shutdown flag and exit
        private void TbcmExit_Click(object sender, RoutedEventArgs e)
        {
            explicitClose = true;
            Application.Current.Shutdown();
        }

        // This is the "Manage" item at the top of the context menu
        private void TbcmRestore_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }
        #endregion Tray Menu events

        ////////////////////////////// Window events ////////////////////////////////

        #region Window events
        // Window closing
        protected override void OnClosing(CancelEventArgs e)
        {
            // Intercept close from the X in the title bar or from the taskbar
            // in case user didn't really mean to exit. Could change this to minimize.
            if (!explicitClose)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to exit TrayLauncher?",
                                                          "Exit TrayLauncher?",
                                                          MessageBoxButton.YesNo,
                                                          MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    //clean up notify icon (would otherwise stay open until application finishes)
                    myNotifyIcon.Dispose();
                    base.OnClosing(e);
                }
            }
            // Exit selected from menu so don't ask intention
            else
            {
                WriteLog.WriteTempFile("  Explicit exit");
                myNotifyIcon.Dispose();
                base.OnClosing(e);
            }
        }

        // Hide window when minimized. No icon on task bar. 
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
            WriteLog.WriteTempFile("TrayLauncher is shutting down.");
        }
        #endregion Window events

        ////////////////////////////// Datagrid events //////////////////////////////

        #region Datagrid events
        // Double-click in datagrid = update selected item
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mnuUpdate.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }
        #endregion Datagrid events

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
                        // Resolve environmental variables
                        launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(x.AppPath);
                        launch.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(x.Arguments);
                        // Next line does the magic.
                        launch.Start();
                        WriteLog.WriteTempFile($"  Launching: {x.Header}," +
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
        #endregion Menu click handler  <- this guy does the launching

        //////////////////////// Main window menu events ////////////////////////////

        #region Menu events  <- the other menu events

        ////////////////////////////// File menu //////////////////////////////////
        #region File menu
        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            explicitClose = true;
            Application.Current.Shutdown();
        }

        // Backup the menu file
        private void MnuBackupXML_Click(object sender, RoutedEventArgs e)
        {
            BackupXMLFile();
        }
        #endregion

        ////////////////////////////// Configuration menu //////////////////////////
        #region Config menu
        // Add
        private void MnuAdd_Click(object sender, RoutedEventArgs e)
        {
            AddItem add = new AddItem
            {
                Owner = Application.Current.MainWindow
            };
            add.ShowDialog();
            SortXMLFile();
            trayMenu.Items.Clear();
            ConstructMenu();
        }

        // Add
        private void MnuDel_Click(object sender, RoutedEventArgs e)
        {
            if (theDataGrid.SelectedItem != null)
            {
                SortXMLFile();
                RemoveItem();
                trayMenu.Items.Clear();
                ConstructMenu();
            }
        }

        // Update
        private void MnuUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (theDataGrid.SelectedItem != null)
            {
                TLMenuItem x = (TLMenuItem)theDataGrid.SelectedItem;
                string header = x.Header;
                string appPath = x.AppPath;
                string args = x.Arguments;
                string ttip = x.ToolTip;
                string iType = x.ItemType;
                int pos = x.Pos;
                int index = theDataGrid.SelectedIndex;

                UpdateItem update = new UpdateItem(header, appPath, args, ttip, pos, iType, index)
                {
                    Owner = this
                };
                update.ShowDialog();
                SortXMLFile();
                trayMenu.Items.Clear();
                ConstructMenu();
            }
        }

        // Test menu item
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
                    else if (x.ItemType == "S" || x.ItemType == "H")
                    {
                        myNotifyIcon.ShowBalloonTip("Test Launch Error",
                            $"Cannot test a separator or\nsection header",
                            BalloonIcon.Info);
                    }
                    else
                    {
                        try
                        {
                            launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(x.AppPath);
                            launch.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(x.Arguments);
                            launch.Start();
                            WriteLog.WriteTempFile($"  Launching: {x.Header}," +
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

        // Menu refresh
        private void MnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("  Refreshing menu");
            trayMenu.Items.Clear();
            RefreshMenu();
            ConstructMenu();
            WriteLog.WriteTempFile($"    Menu refreshed");
        }
        #endregion

        ////////////////////////////// Options menu ///////////////////////////////
        #region Options menu
        //Star with Windows?
        private void MnuStartWW_Checked(object sender, RoutedEventArgs e)
        {
            // No shortcut creation in debug mode
#if !DEBUG
            StartupShortcut("Create");
            Properties.Settings.Default.StartWithWindows = true;
#endif
        }

        private void MnuStartWW_Unchecked(object sender, RoutedEventArgs e)
        {
            // No shortcut deletion in debug mode
#if !DEBUG
            StartupShortcut("Delete");
            Properties.Settings.Default.StartWithWindows = true;
#endif
        }

        // Startup notification
        private void MnuStartNotify_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartNotification = true;
            WriteLog.WriteTempFile($"    Startup notification set to True");
        }

        private void MnuStartNotify_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartNotification = false;
            WriteLog.WriteTempFile($"    Startup notification set to False");
        }

        // Hide main window on startup
        private void MnuHideOnStart_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.HideOnStart = true;
            WriteLog.WriteTempFile($"    Hide on startup set to True");
        }

        private void MnuHideOnStart_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.HideOnStart = false;
            WriteLog.WriteTempFile($"    Hide on startup set to False");
        }

        // Font size
        private void MnuFontIncrease_Click(object sender, RoutedEventArgs e)
        {
            FontLarger();
        }

        private void MnuFontDecrease_Click(object sender, RoutedEventArgs e)
        {
            FontSmaller();
        }

        // Background color
        private void CmbBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Color selectedColor = (Color)(CmbBackground.SelectedItem as PropertyInfo).
                GetValue(null, null);
            SetMenuBackground(selectedColor);
            //tbColorSample.Background = new SolidColorBrush(selectedColor);
            Properties.Settings.Default.BackColor = CmbBackground.SelectedIndex;
            if (Visibility == Visibility.Visible)
            {
                mnuBackColor.IsSubmenuOpen = false;
            }
            Debug.WriteLine($"Background SelectedIndex is {CmbBackground.SelectedIndex}");
            WriteLog.WriteTempFile($"    Menu background color is {CmbBackground.SelectedItem}, " +
                                   $"index is {CmbBackground.SelectedIndex}");
        }

        //Foreground (menu text) color
        private void CmbForeground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Color selectedColor = (Color)(CmbForeground.SelectedItem as PropertyInfo).
                GetValue(null, null);
            trayMenu.Foreground = new SolidColorBrush(selectedColor);

            //tbColorSample.Foreground = new SolidColorBrush(selectedColor);
            Properties.Settings.Default.ForeColor = CmbForeground.SelectedIndex;

            if (Visibility == Visibility.Visible)
            {
                mnuTextColor.IsSubmenuOpen = false;
            }

            Debug.WriteLine($"Foreground SelectedIndex is {CmbForeground.SelectedIndex}");
            WriteLog.WriteTempFile($"    Menu text color is {CmbForeground.SelectedItem.ToString()}, " +
                                   $"index is {CmbForeground.SelectedIndex}");
        }

        // Section header color
        private void CmbHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SectionHeaderColor = CmbHeader.SelectedIndex;

            if (Visibility == Visibility.Visible)
            {
                trayMenu.Items.Clear();
                SortXMLFile();
                ConstructMenu();
                mnuHeaderColor.IsSubmenuOpen = false;
            }

            Debug.WriteLine($"Section Header SelectedIndex is {CmbHeader.SelectedIndex}");
            WriteLog.WriteTempFile($"    Menu section header color is {CmbHeader.SelectedItem.ToString()}, " +
                                   $"index is {CmbHeader.SelectedIndex}");
        }

        // Section header bold?
        private void MnuHeaderBold_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SectionHeaderBold = true;

            if (Visibility == Visibility.Visible)
            {
                trayMenu.Items.Clear();
                //RefreshMenu();
                ConstructMenu();
            }
        }

        private void MnuHeaderBold_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SectionHeaderBold = false;
            trayMenu.Items.Clear();
            //RefreshMenu();
            ConstructMenu();
        }

        // Separator color
        private void CmbSeparator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SeparatorColor = CmbSeparator.SelectedIndex;
            if (Visibility == Visibility.Visible)
            {
                trayMenu.Items.Clear();
                SortXMLFile();
                ConstructMenu();
                mnuSepColor.IsSubmenuOpen = false;
            }
            Debug.WriteLine($"Separator SelectedIndex is {CmbSeparator.SelectedIndex}");
            WriteLog.WriteTempFile($"    Separator color is {CmbSeparator.SelectedItem.ToString()}, " +
                                   $"index is {CmbSeparator.SelectedIndex}");
        }

        // Tray icon color
        private void TrayIcoColor_Click(object sender, RoutedEventArgs e)
        {
            MenuItem trayIconColor = e.Source as MenuItem;

            // uncheck all
            mnuBlueIco.IsChecked = false;
            mnuBlue2Ico.IsChecked = false;
            mnuBlackIco.IsChecked = false;
            mnuCyanIco.IsChecked = false;
            mnuGrayIco.IsChecked = false;
            mnuGreenIco.IsChecked = false;
            mnuOrangeIco.IsChecked = false;
            mnuRedIco.IsChecked = false;
            mnuTealIco.IsChecked = false;
            mnuWhiteIco.IsChecked = false;
            mnuYellowIco.IsChecked = false;
            mnuMagentaIco.IsChecked = false;

            // check the one we want
            trayIconColor.IsChecked = true;

            // change the icon
            string icoColor = trayIconColor.Tag.ToString().ToLower();
            Properties.Settings.Default.Icon = icoColor;
            switch (icoColor)
            {
                case "blue":
                    {
                        IconFromFile(blueIcon);
                        break;
                    }
                case "blue2":
                    {
                        IconFromFile(blue2Icon);
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
                case "gray":
                    {
                        IconFromFile(grayIcon);
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
                case "teal":
                    {
                        IconFromFile(tealIcon);
                        break;
                    }
                case "yellow":
                    {
                        IconFromFile(yellowIcon);
                        break;
                    }
                case "magenta":
                    {
                        IconFromFile(magentaIcon);
                        break;
                    }
                case "orange":
                    {
                        IconFromFile(orangeIcon);
                        break;
                    }
                default:
                    {
                        IconFromFile(whiteIcon);
                        break;
                    }
            }
        }
        #endregion region

        ////////////////////////////// Help menu //////////////////////////////////
        #region Help menu
        // Readme
        private void MnuReadme_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(@".\ReadMe.txt");
        }

        // About
        private void MnuAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            _ = about.ShowDialog();
        }
        #endregion region

        #endregion Menu events  <- the other menu events

        ////////////////////////////// Keyboard events //////////////////////////////

        #region Keyboard events
        // Handle keyboard events
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            //F1 = About
            if (e.Key == Key.F1)
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
                                     $"Menu Section Header Index = { Properties.Settings.Default.SectionHeaderColor}\n" +
                                     $"Menu Section Header Bold = {Properties.Settings.Default.SectionHeaderBold}\n" +
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
                        WriteLog.WriteTempFile($"* Unable to open {xmlMenuFile}");
                        WriteLog.WriteTempFile($"* {ex.Message}");
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
                        WriteLog.WriteTempFile($"* Unable to open {path}");
                        WriteLog.WriteTempFile($"* {ex.Message}");
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

            // F7 = Shade alternate rows
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
            WriteLog.WriteTempFile($"  Application architecture is " +
                                   $"{RuntimeInformation.ProcessArchitecture.ToString()} ");
            WriteLog.WriteTempFile("  Entering ReadSettings");

            // Settings upgrade if needed
            if (Properties.Settings.Default.SettingsUpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.SettingsUpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            // First time?
            firstRun = Properties.Settings.Default.FirstRun;
            WriteLog.WriteTempFile($"    FirstRun is {firstRun}");
            if (firstRun)
            {
                Properties.Settings.Default.FirstRun = false;
                //Show();
            }
            else
            {
                // Window position
                Top = Properties.Settings.Default.WindowTop;
                Left = Properties.Settings.Default.WindowLeft;
            }

            // XML menu filename
            xmlMenuFile = GetXmlFile();
            WriteLog.WriteTempFile($"    Menu file is: {xmlMenuFile}");

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

            // Menu text color
            CmbForeground.SelectedIndex = Properties.Settings.Default.ForeColor;
            CmbForeground.ItemsSource = typeof(Colors).GetProperties();
            Color selectedColor = (Color)(CmbForeground.SelectedItem as PropertyInfo)
                .GetValue(null, null);
            trayMenu.Foreground = new SolidColorBrush(selectedColor);

            // Menu Section Header color
            CmbHeader.SelectedIndex = Properties.Settings.Default.SectionHeaderColor;
            CmbHeader.ItemsSource = typeof(Colors).GetProperties();

            // Menu Section Header bold
            if (Properties.Settings.Default.SectionHeaderBold)
            {
                mnuHeaderBold.IsChecked = true;
            }
            else
            {
                mnuHeaderBold.IsChecked = false;
            }
            WriteLog.WriteTempFile($"    Menu section header bold is {Properties.Settings.Default.SectionHeaderBold}");

            // Menu separator color
            CmbSeparator.SelectedIndex = Properties.Settings.Default.SeparatorColor;
            CmbSeparator.ItemsSource = typeof(Colors).GetProperties();

            // Start with Windows
            if (Properties.Settings.Default.StartWithWindows == true)
            {
                mnuStartWW.IsChecked = true;
            }
            else
            {
                mnuStartWW.IsChecked = false;
            }
            WriteLog.WriteTempFile($"    Start with Windows is {mnuStartWW.IsChecked}");

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
            WriteLog.WriteTempFile($"    Startup notification is {mnuStartNotify.IsChecked}");

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
                        mnuBlueIco.IsChecked = true;
                        break;
                    }
                case "blue2":
                    {
                        IconFromFile(blue2Icon);
                        mnuBlue2Ico.IsChecked = true;
                        break;
                    }
                case "black":
                    {
                        IconFromFile(blackIcon);
                        mnuBlackIco.IsChecked = true;
                        break;
                    }
                case "cyan":
                    {
                        IconFromFile(cyanIcon);
                        mnuCyanIco.IsChecked = true;
                        break;
                    }
                case "gray":
                    {
                        IconFromFile(grayIcon);
                        mnuGrayIco.IsChecked = true;
                        break;
                    }
                case "green":
                    {
                        IconFromFile(greenIcon);
                        mnuGreenIco.IsChecked = true;
                        break;
                    }
                case "orange":
                    {
                        IconFromFile(orangeIcon);
                        mnuOrangeIco.IsChecked = true;
                        break;
                    }
                case "red":
                    {
                        IconFromFile(redIcon);
                        mnuRedIco.IsChecked = true;
                        break;
                    }
                case "teal":
                    {
                        IconFromFile(tealIcon);
                        mnuTealIco.IsChecked = true;
                        break;
                    }
                case "white":
                    {
                        IconFromFile(whiteIcon);
                        mnuWhiteIco.IsChecked = true;
                        break;
                    }
                case "yellow":
                    {
                        IconFromFile(yellowIcon);
                        mnuYellowIco.IsChecked = true;
                        break;
                    }
                case "magenta":
                    {
                        IconFromFile(magentaIcon);
                        mnuMagentaIco.IsChecked = true;
                        break;
                    }
                default:
                    {
                        IconFromFile(whiteIcon);
                        mnuWhiteIco.IsChecked = true;
                        break;
                    }
            }
            WriteLog.WriteTempFile($"    Icon color is {iconFile}");

            // Hide main window on startup
            if (Properties.Settings.Default.HideOnStart == true)
            {
                mnuHideOnStart.IsChecked = true;
                Hide();
                WriteLog.WriteTempFile($"    Main window is hidden on startup");
            }
            else
            {
                mnuHideOnStart.IsChecked = false;
                Show();
                WriteLog.WriteTempFile($"    Main window is shown on startup");
            }

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
            WriteLog.WriteTempFile($"    ShadeAltRows is {altRows}");

            WriteLog.WriteTempFile("  Leaving ReadSettings");
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
            Title = string.Format($"{myExe} - {titleVer}");

            WriteLog.WriteTempFile($"    {myExe} version is {titleVer}");
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
                        WriteLog.WriteTempFile($"  Shortcut saved to {shortcutPath}");
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show($"Error creating shortcut\n{ex.Message}",
                                                "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        WriteLog.WriteTempFile($"* Error creating shortcut.");
                        WriteLog.WriteTempFile($"* {ex.Message}");
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
                        WriteLog.WriteTempFile($"* Error deleting shortcut.");
                        WriteLog.WriteTempFile($"* {ex.Message}");
                    }
                }
                WriteLog.WriteTempFile($"  Shortcut removed from {shortcutPath}");
            }
        }
        #endregion Startup Shortcut

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
                    WriteLog.WriteTempFile($"    Setting tray icon to {myIcon}");
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"{myIcon} is not a valid icon file.\n\n{ex.Message}", "ERROR",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    WriteLog.WriteTempFile($"* {myIcon} is not a valid icon file.");
                    WriteLog.WriteTempFile($"* {ex.Message}");
                }
            }
            else
            {
                WriteLog.WriteTempFile($"* {myIcon} was not found.");
            }
        }
        #endregion Get icon from Images folder

        #region Set Menu background color
        public void SetMenuBackground(Color selectedColor)
        {
            // Set the background color of the tray menu from https://www.dotnetcurry.com/wpf/1142/resources-wpf-static-dynamic-difference

            SolidColorBrush findsolidColorBrush = FindResource("brush1") as SolidColorBrush;
            findsolidColorBrush.Color = selectedColor;
            Resources.Remove("brush1");
            Resources.Add("brush1", findsolidColorBrush);
        }
        #endregion Set Menu background color

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
                WriteLog.WriteTempFile($"    Font size set to {trayMenu.FontSize}");
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
                WriteLog.WriteTempFile($"    Font size set to {trayMenu.FontSize}");
            }
        }

        #endregion Font size

        #region Backup XML data
        // Simple backup of the XML menu file
        private void BackupXMLFile()
        {
            string tStamp = string.Format("{0:yyyyMMdd}", DateTime.Now);
            SaveFileDialog dlgSave = new SaveFileDialog
            {
                Title = "Choose backup location",
                CheckPathExists = true,
                CheckFileExists = false,
                OverwritePrompt = true,
                FileName = $"TrayLauncher_Menu_{tStamp}_backup.xml",
                Filter = "XML (*.xml|*.XML| All files (*.*)|*.*"
            };
            if (dlgSave.ShowDialog() == true)
            {
                try
                {
                    // Third parameter is overwrite which we want since we have overwrite prompt above
                    File.Copy(xmlMenuFile, dlgSave.FileName, true);
                    WriteLog.WriteTempFile($"  Menu items backed up to {dlgSave.FileName} ");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Backup failed\n{ex.Message}", "ERROR",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    WriteLog.WriteTempFile($"* Backup failed.");
                    WriteLog.WriteTempFile($"* {ex.Message}");
                }
            }
        }
        #endregion Backup XML data

        #region Alternate row shading
        // Shade alternate rows of the datagrid in the main window
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