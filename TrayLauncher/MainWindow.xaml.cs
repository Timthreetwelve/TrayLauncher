// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.
//
// TrayLauncher - TrayLauncher is a customizable menu to launch applications, websites, documents,
// settings, and folders from the system tray.
//
// See App.xaml.cs for code that restricts app to one instance

#region using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
using Hardcodet.Wpf.TaskbarNotification;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using TKUtils;
using File = System.IO.File;
using Path = System.IO.Path;
#endregion using directives

namespace TrayLauncher
{
    public partial class MainWindow : Window
    {
        private string xmlMenuFile;
        private MenuList XmlData;

        #region Icon filename variables
        private readonly string blackIcon = @"Images\black.ico";
        private readonly string mediumblueIcon = @"Images\mediumblue.ico";
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
            UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);

            InitializeComponent();

            ReadSettings();

            ConstructMenu();
        }

        ////////////////////////////// XML file methods /////////////////////////////

        #region Get the menu XML file  <- determine path and see if file exists
        private string GetXmlFile()
        {
            try
            {
                // Append the filename
                string menuFile = Path.Combine(AppInfo.AppDirectory, "MenuItems.xml");

                // If the menu file doesn't exist, create a new one and seed it with one record
                if (!File.Exists(menuFile))
                {
                    WriteLog.WriteTempFile($"    Creating new XML menu file: {menuFile}");
                    File.Create(menuFile).Dispose();
                    CreateXMLStarter(menuFile);
                }

                // Save the path to setting
                UserSettings.Setting.XMLFile = menuFile;
                return menuFile;
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading or writing to menu file\n{ex.Message}",
                                     "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile("* Error reading or writing to menu file.");
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
                WriteLog.WriteTempFile("* Error reading or writing to menu file.");
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
                WriteLog.WriteTempFile("* Error reading or writing to menu file.");
                WriteLog.WriteTempFile($"* {ex.Message}");
            }
        }
        #endregion Refresh  <- reread, sort and renumber

        #region Create XML Starter  <- creates starter menu items and separators
        private void CreateXMLStarter(string menuFile)
        {
            // This will create a "starter" menu if none exists
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
                            new XElement("Type", "SH")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "200"),
                            new XElement("MenuHeader", "Command Prompt"),
                            new XElement("AppPath", @"C:\Windows\System32\Cmd.exe"),
                            new XElement("Arguments", "/k Echo Hello from TrayLauncher!"),
                            new XElement("ToolTip", "This is an example menu item with arguments"),
                            new XElement("Type", "")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "300"),
                            new XElement("MenuHeader", "Calculator"),
                            new XElement("AppPath", "Calc.exe"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "This is an app example"),
                            new XElement("Type", "")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "400"),
                            new XElement("MenuHeader", "Windows Folder"),
                            new XElement("AppPath", "Explorer.exe"),
                            new XElement("Arguments", "%WINDIR%"),
                            new XElement("ToolTip", "This is a folder example"),
                            new XElement("Type", "")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "500"),
                            new XElement("MenuHeader", "Separator"),
                            new XElement("AppPath", "Not applicable"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a separator"),
                            new XElement("Type", "SEP")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "600"),
                            new XElement("MenuHeader", "Search for Puppies"),
                            new XElement("AppPath", "www.bing.com/images/search?q=puppies"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "It works for websites too"),
                            new XElement("Type", "")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "700"),
                            new XElement("MenuHeader", "Separator"),
                            new XElement("AppPath", "Not applicable"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a separator"),
                            new XElement("Type", "SEP")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "800"),
                            new XElement("MenuHeader", "Taskbar Settings"),
                            new XElement("AppPath", "ms-settings:taskbar"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "This is a settings example"),
                            new XElement("Type", "")),
                        new XElement("TLMenuItem",
                            new XElement("Position", "900"),
                            new XElement("MenuHeader", "Separator"),
                            new XElement("AppPath", "Not applicable"),
                            new XElement("Arguments", ""),
                            new XElement("ToolTip", "Not visible for a separator"),
                            new XElement("Type", "SEP"))
                        ));
                xDoc.Save(menuFile);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error writing to menu file\n{ex.Message}",
                                     "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile("* Error writing to menu file.");
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
            int sepIndex = UserSettings.Setting.SeparatorColor;
            Color sepColor = ColorIndexToColor(sepIndex);
            Separator sep = new Separator
            {
                BorderBrush = new SolidColorBrush(sepColor),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            _ = trayMenu.Items.Add(sep);
            WriteLog.WriteTempFile("    Loaded: Default item - Separator");

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
                    "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile("* The menu XML file cannot be found.");
                Show();
            }
            else
            {
                try
                {
                    // Deserialize the XML file
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
                    WriteLog.WriteTempFile("* Unable to continue. Shutting down.");

                    // Bail out
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
            WriteLog.WriteTempFile($"    Loaded {sortedList.Count} items");
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
            const int itemOffset = 1;
            MenuItem prevMenuItem = null;

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
                if (item.ItemType == "SEP")
                {
                    Debug.WriteLine("Adding a separator");
                    int sepIndex = UserSettings.Setting.SeparatorColor;
                    Color sepColor = ColorIndexToColor(sepIndex);

                    Separator sep = new Separator
                    {
                        BorderBrush = new SolidColorBrush(sepColor),
                        BorderThickness = new Thickness(0, 1, 0, 0)
                    };
                    trayMenu.Items.Insert(itemCounter + itemOffset, sep);
                    WriteLog.WriteTempFile("    Loaded: <Separator>");
                }

                // If menu item is a Section Header
                else if (item.ItemType == "SH")
                {
                    Debug.WriteLine("Adding a section header");
                    mi.Header = item.Header;
                    mi.Tag = item.ItemType;
                    mi.IsHitTestVisible = false;
                    mi.Margin = new Thickness(-5, 0, 0, 0);
                    int HeaderColorIndex = UserSettings.Setting.SectionHeaderColor;
                    Color headerColor = ColorIndexToColor(HeaderColorIndex);
                    mi.Foreground = new SolidColorBrush(headerColor);
                    if (UserSettings.Setting.SectionHeaderBold)
                    {
                        mi.FontWeight = FontWeights.Bold;
                    }
                    trayMenu.Items.Insert(itemCounter + itemOffset, mi);
                    WriteLog.WriteTempFile($"    Loaded: <Section header> {item.Header}");
                }

                // Sub menu
                else if (item.ItemType == "SM")
                {
                    mi.Header = item.Header;
                    mi.Margin = new Thickness(5, 0, 0, 0);
                    prevMenuItem = mi;
                    trayMenu.Items.Insert(itemCounter + itemOffset, mi);
                    Debug.WriteLine($"Adding a Sub menu: {item.Header}");
                    WriteLog.WriteTempFile($"    Loaded: <Sub menu> {item.Header}");
                }
                else
                // normal & sub menu item
                {
                    mi.Header = item.Header;
                    if (!string.IsNullOrEmpty(item.ToolTip))
                    {
                        mi.ToolTip = $"{item.ToolTip}";
                    }
                    mi.Click += Mi_Click;
                    mi.Tag = item;
                    if (item.ItemType == "SMI")
                    {
                        mi.Margin = new Thickness(-10, 0, -5, 0);
                        prevMenuItem.Items.Add(mi);
                        itemCounter--;
                        WriteLog.WriteTempFile($"    Loaded: {prevMenuItem.Header} > {item.Header}");
                    }
                    else
                    {
                        mi.Margin = new Thickness(5, 0, 0, 0);
                        trayMenu.Items.Insert(itemCounter + itemOffset, mi);
                        WriteLog.WriteTempFile($"    Loaded: {item.Header}");
                    }
                    Debug.WriteLine($"Adding item {item.Header}");
                }
            }
            WriteLog.WriteTempFile("    Loading of menu items is complete");
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
                WriteLog.WriteTempFile("* Error reading or writing to menu file.");
                WriteLog.WriteTempFile($"* {ex.Message}");
            }
            WriteLog.WriteTempFile("  Leaving RemoveItem");
        }
        #endregion Remove an item

        ///////////////////////////// NotifyIcon events /////////////////////////////

        #region Tray icon events
        // Show window and bring to the front.
        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Topmost = true;
            Focus();
            Topmost = false;
        }
        #endregion Tray icon events

        ////////////////////////////// Tray menu events /////////////////////////////

        #region Tray Menu events
        // Set explicit shutdown flag and exit
        private void TbcmExit_Click(object sender, RoutedEventArgs e)
        {
            ExplicitShutdown();
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
        // Window closing event
        protected override void OnClosing(CancelEventArgs e)
        {
            // Is Windows shutting down?
            if (!App.WindowsLogoffOrShutdown)
            {
                // Intercept close from the X in the title bar or from the taskbar
                // in case user didn't really mean to exit. Could change this to minimize.
                if (!App.ExplicitClose)
                {
                    // Is option set to minimize to tray?
                    if (UserSettings.Setting.MinimizeToTrayOnExit)
                    {
                        Hide();
                        e.Cancel = true;
                    }
                    else
                    {
                        if (UserSettings.Setting.VerifyExit)
                        {
                            // Ask user
                            MessageBoxResult result = MessageBox.Show(
                                "Do you want to exit TrayLauncher?", "Exit TrayLauncher?",
                                MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.No)
                            {
                                e.Cancel = true;
                            }
                            else
                            {
                                //clean up notify icon (would otherwise stay after application finishes)
                                myNotifyIcon.Dispose();
                                base.OnClosing(e);
                            }
                        }
                        else
                        {
                            myNotifyIcon.Dispose();
                            base.OnClosing(e);
                        }
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
            else
            {
                WriteLog.WriteTempFile("TrayLauncher is closing due to user log off or Windows shutdown");
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
            UserSettings.Setting.WindowLeft = Left;
            UserSettings.Setting.WindowTop = Top;
            UserSettings.Setting.WindowHeight = Height;
            UserSettings.Setting.WindowWidth = Width;

            // save the property settings
            //Properties.Settings.Default.Save();

            UserSettings.SaveSettings();

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

        #region Tray Menu click handler  <- this guy does the launching
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
                    WriteLog.WriteTempFile("* AppPath is empty.");
                }
                else
                {
                    try
                    {
                        // Resolve environmental variables
                        launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(x.AppPath);
                        launch.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(x.Arguments);

                        // Next line does the magic.
                        _ = launch.Start();
                        string arg;
                        if (string.IsNullOrEmpty(x.Arguments))
                        {
                            arg = "*none*";
                        }
                        else
                        {
                            arg = x.Arguments;
                        }
                        WriteLog.WriteTempFile($"  Launching: {x.Header}," +
                                               $" Position: {x.Pos}," +
                                               $" AppPath: {x.AppPath}," +
                                               $" Arguments: {arg},");
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

        #region Main Menu events

        ////////////////////////////// File menu //////////////////////////////////

        #region File menu
        // Exit
        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            ExplicitShutdown();
        }

        // Backup the menu file
        private void MnuBackupXML_Click(object sender, RoutedEventArgs e)
        {
            BackupXMLFile();
        }

        // Restore the menu file
        private void MnuRestoreXML_Click(object sender, RoutedEventArgs e)
        {
            RestoreXMLFile();
        }
        #endregion

        ////////////////////////////// Configuration menu //////////////////////////

        #region Config menu
        // Disable menu items if nothing in datagrid is selected
        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (theDataGrid.SelectedIndex == -1)
            {
                mnuDelete.IsEnabled = false;
                mnuUpdate.IsEnabled = false;
                mnuTest.IsEnabled = false;
                mnuCopy.IsEnabled = false;
            }
            else
            {
                mnuDelete.IsEnabled = true;
                mnuUpdate.IsEnabled = true;
                mnuTest.IsEnabled = true;
                mnuCopy.IsEnabled = true;
            }
        }

        // Add
        private void MnuAdd_Click(object sender, RoutedEventArgs e)
        {
            int pos = -1;

            if (theDataGrid.SelectedItem != null)
            {
                TLMenuItem x = (TLMenuItem)theDataGrid.SelectedItem;
                pos = x.Pos;
            }

            AddItem add = new AddItem(pos)
            {
                Owner = Application.Current.MainWindow
            };
            add.ShowDialog();
            if (add.DialogResult == true)
            {
                SortXMLFile();
                trayMenu.Items.Clear();
                ConstructMenu();
                Debug.WriteLine("*** ShowDialog result is true");
            }
            else
            {
                Debug.WriteLine("+++ ShowDialog result is false");
            }
        }

        // Copy
        private void MnuCopy_Click(object sender, RoutedEventArgs e)
        {
            if (theDataGrid.SelectedItem != null)
            {
                TLMenuItem x = (TLMenuItem)theDataGrid.SelectedItem;
                string header = x.Header;
                string appPath = x.AppPath;
                string args = x.Arguments;
                string ttip = x.ToolTip;
                string iType = x.ItemType;
                int index = theDataGrid.SelectedIndex;
                int NewPos = x.Pos + 1;

                CopyItem cpy = new CopyItem(header, appPath, args, ttip, NewPos, iType, index)
                {
                    Owner = this
                };
                cpy.ShowDialog();

                if (cpy.DialogResult == true)
                {
                    SortXMLFile();
                    trayMenu.Items.Clear();
                    ConstructMenu();
                    Debug.WriteLine("*** ShowDialog result is true");
                }
                else
                {
                    Debug.WriteLine("+++ ShowDialog result is false");
                }
            }
        }

        // Delete
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
                if (update.DialogResult == true)
                {
                    SortXMLFile();
                    trayMenu.Items.Clear();
                    ConstructMenu();
                    Debug.WriteLine("*** ShowDialog result is true");
                }
                else
                {
                    Debug.WriteLine("+++ ShowDialog result is false");
                }
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

                    if (x.ItemType == "SM" || x.ItemType == "SEP" || x.ItemType == "SH")
                    {
                        return;
                    }
                    if (string.IsNullOrEmpty(x.AppPath))
                    {
                        _ = MessageBox.Show($"{x.Header}\nAppPath cannot be empty", "Launch Error",
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                        WriteLog.WriteTempFile($"* Failed to launch {x.Header} ");
                        WriteLog.WriteTempFile("* AppPath is empty.");
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
                            _ = MessageBox.Show($"Failed to launch \"{x.Header} - {x.AppPath}\"\n{ex.Message}",
                                                "Test Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            WriteLog.WriteTempFile("    Menu refreshed");
        }

        // Preferences
        private void MnuPreferences_Click(object sender, RoutedEventArgs e)
        {
            OpenSettingsWindow();
        }
        #endregion

        ////////////////////////////// Help menu //////////////////////////////////

        #region Help menu
        // Readme
        private void MnuReadme_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(@".\ReadMe.txt");
        }

        // Change log
        private void MnuChange_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(@".\ChangeLog.txt");
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

        // Show settings
        private void MnuShowSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsWindow();
        }

        // View menu file
        private void MnuViewMenu_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(xmlMenuFile);
        }

        // View temp/log file
        private void MnuViewLog_Click(object sender, RoutedEventArgs e)
        {
            string path = WriteLog.GetTempFile();
            TextFileViewer.ViewTextFile(path);
        }
        #endregion region

        #endregion Menu events

        ////////////////////////////// Keyboard events //////////////////////////////

        #region Keyboard events
        // Handle keyboard events
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // Ctrl+Comma = Preferences
            if (e.Key == Key.OemComma && (e.KeyboardDevice.Modifiers == ModifierKeys.Control))
            {
                OpenSettingsWindow();
            }

            // Del = Remove
            if (e.Key == Key.Delete && (e.KeyboardDevice.Modifiers == ModifierKeys.None) && theDataGrid.SelectedItem != null)
            {
                RemoveItem();
                trayMenu.Items.Clear();
                ConstructMenu();
            }

            //F1 = About
            if (e.Key == Key.F1 && (e.KeyboardDevice.Modifiers == ModifierKeys.None))
            {
                mnuAbout.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            }

            //Shift F1 = Show settings
            if (e.Key == Key.F1 && (e.KeyboardDevice.Modifiers == ModifierKeys.Shift))
            {
                ShowSettingsWindow();
            }

            // F3 = View menu file
            if (e.Key == Key.F3)
            {
                TextFileViewer.ViewTextFile(xmlMenuFile);
            }

            // F4 = View temp file
            if (e.Key == Key.F4)
            {
                string path = WriteLog.GetTempFile();
                TextFileViewer.ViewTextFile(path);
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
                UserSettings.Setting.ShadeAltRows = !UserSettings.Setting.ShadeAltRows;
            }
        }

        #endregion Keyboard events

        ////////////////////////////// Settings events //////////////////////////////

        #region Setting change
        private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
            var newValue = prop?.GetValue(sender, null);
            switch (e.PropertyName)
            {
                case "ShadeAltRows":
                    if ((bool)newValue)
                    {
                        AltRowShadingOn();
                    }
                    else
                    {
                        AltRowShadingOff();
                    }
                    break;

                case "FontSize":
                    theDataGrid.FontSize = (double)newValue;
                    trayMenu.FontSize = (double)newValue;
                    break;

                case "Icon":
                    SetTrayIcon(newValue.ToString());
                    break;

                case "ShowItemType":
                    theDataGrid.Columns[1].Visibility = ((bool)newValue) ? Visibility.Visible : Visibility.Collapsed;
                    break;

                case "KeepOnTop":
                    Topmost = (bool)newValue;
                    break;

                case "ForeColor":
                    Color textColor = ColorIndexToColor((int)newValue);
                    trayMenu.Foreground = new SolidColorBrush(textColor);
                    tbToolTip.Foreground = new SolidColorBrush(textColor);
                    break;

                case "BackColor":
                    Color backColor = ColorIndexToColor((int)newValue);
                    SetMenuBackground(backColor);
                    break;

                case "SectionHeaderBold":
                    foreach (var item in trayMenu.Items)
                    {
                        if (item.GetType() == typeof(MenuItem))
                        {
                            var x = (MenuItem)item;
                            if (x.Tag?.ToString() == "SH")
                            {
                                if ((bool)newValue)
                                {
                                    x.FontWeight = FontWeights.Bold;
                                }
                                else
                                {
                                    x.FontWeight = FontWeights.Normal;
                                }
                            }
                        }
                    }
                    break;

                case "SectionHeaderColor":
                    foreach (var item in trayMenu.Items)
                    {
                        if (item.GetType() == typeof(MenuItem))
                        {
                            var x = (MenuItem)item;
                            if (x.Tag?.ToString() == "SH")
                            {
                                Color headerColor = ColorIndexToColor((int)newValue);
                                x.Foreground = new SolidColorBrush(headerColor);
                            }
                        }
                    }
                    break;

                case "SeparatorColor":
                    foreach (var item in trayMenu.Items)
                    {
                        if (item.GetType() == typeof(Separator))
                        {
                            var x = (Separator)item;
                            Color sepColor = ColorIndexToColor((int)newValue);
                            x.BorderBrush = new SolidColorBrush(sepColor);
                        }
                    }
                    break;

                case "StartWithWindows":
                    if (!Debugger.IsAttached)
                    {
                        if ((bool)newValue)
                        {
                            StartupShortcut("Create");
                        }
                        else
                        {
                            StartupShortcut("Delete");
                        }
                    }
                    break;
            }
            Debug.WriteLine($"+++Setting change: {e.PropertyName} New Value: {newValue}");
            WriteLog.WriteTempFile($"    Changing {e.PropertyName} to {newValue}");
        }
        #endregion Setting change

        ////////////////////////////// Read Settings ///////////////////////////////

        #region Settings file
        private void ReadSettings()
        {
            // Settings change event
            UserSettings.Setting.PropertyChanged += UserSettingChanged;

            WriteLog.WriteTempFile(" ");
            WriteLog.WriteTempFile($"TrayLauncher {AppInfo.TitleVersion} is starting up");
            WriteLog.WriteTempFile("  Entering ReadSettings");
            WriteLog.WriteTempFile("    Application architecture: " +
                                   $"{RuntimeInformation.ProcessArchitecture} ");
            WriteLog.WriteTempFile($"    Slow Machine: {SystemParameters.IsSlowMachine}");
            WriteLog.WriteTempFile($"    Mouse is present: {SystemParameters.IsMousePresent}");

            if (!SystemParameters.IsMousePresent)
            {
                Console.WriteLine("* Systems parameters indicate no mouse is present!");
            }

            // First time?
            bool firstRun = UserSettings.Setting.FirstRun;
            WriteLog.WriteTempFile($"    FirstRun is {firstRun}");
            if (firstRun)
            {
                UserSettings.Setting.FirstRun = false;
            }
            else
            {
                // Window position
                Top = UserSettings.Setting.WindowTop;
                Left = UserSettings.Setting.WindowLeft;
                Width = UserSettings.Setting.WindowWidth;
                Height = UserSettings.Setting.WindowHeight;
            }

            // XML menu filename
            xmlMenuFile = GetXmlFile();
            WriteLog.WriteTempFile($"    Menu file is: {xmlMenuFile}");

            // Put version number in title bar
            WindowTitleVersion();

            // Max height for main window
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 20;
            WriteLog.WriteTempFile($"    Primary screen height is: {SystemParameters.PrimaryScreenHeight}");
            WriteLog.WriteTempFile($"    Primary screen width is: {SystemParameters.PrimaryScreenWidth}");
            WriteLog.WriteTempFile($"    Max MainWindow height is: {MaxHeight}");

            // Startup Notification
            if (UserSettings.Setting.StartNotification)
            {
                myNotifyIcon.ShowBalloonTip("TrayLauncher is Running",
                    "Right-click for launch menu", BalloonIcon.Info);
            }
            WriteLog.WriteTempFile($"    Startup notification is: {UserSettings.Setting.StartNotification}");

            // Background color
            int bkColorIndex = UserSettings.Setting.BackColor;
            Color bkColor = ColorIndexToColor(bkColorIndex);
            SetMenuBackground(bkColor);
            WriteLog.WriteTempFile($"    Background color is: {UserSettings.Setting.BackColor}");

            // Menu text color
            int textColorIndex = UserSettings.Setting.ForeColor;
            Color textColor = ColorIndexToColor(textColorIndex);
            trayMenu.Foreground = new SolidColorBrush(textColor);
            tbToolTip.Foreground = new SolidColorBrush(textColor);
            WriteLog.WriteTempFile($"    Menu text color is: {UserSettings.Setting.ForeColor}");

            // Datagrid font size
            theDataGrid.FontSize = UserSettings.Setting.FontSize;
            trayMenu.FontSize = UserSettings.Setting.FontSize;
            WriteLog.WriteTempFile($"    Font size is: {UserSettings.Setting.FontSize}");

            // Icon file
            string iconFile = UserSettings.Setting.Icon;
            SetTrayIcon(iconFile);

            // Hide main window on startup
            if (UserSettings.Setting.HideOnStart)
            {
                Hide();
                WriteLog.WriteTempFile("    Main window is hidden on startup");
            }

            // Alternate row shading
            if (UserSettings.Setting.ShadeAltRows)
            {
                AltRowShadingOn();
            }
            else
            {
                AltRowShadingOff();
            }
            WriteLog.WriteTempFile($"    Shade Alternate Rows is {UserSettings.Setting.ShadeAltRows}");

            // Show Item type column
            if (UserSettings.Setting.ShowItemType)
            {
                theDataGrid.Columns[1].Visibility = Visibility.Visible;
            }
            else
            {
                theDataGrid.Columns[1].Visibility = Visibility.Collapsed;
            }
            WriteLog.WriteTempFile($"    Show item type column is {UserSettings.Setting.ShowItemType}");

            // End of settings
            WriteLog.WriteTempFile("  Leaving ReadSettings");
        }

        #endregion Settings file

        ////////////////////////////// Helper Methods ///////////////////////////////

        #region Title version
        public void WindowTitleVersion()
        {
            // Set the windows title
            Title = string.Format($"{AppInfo.AppName} - {AppInfo.TitleVersion}");
        }
        #endregion Title version

        #region Startup Shortcut
        // This will create/delete a shortcut in the users Startup folder
        private static void StartupShortcut(string mode)
        {
            string startupfolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupfolder, AppInfo.AppProcessName + ".lnk");

            if (string.Equals(mode, "create", StringComparison.OrdinalIgnoreCase) && !File.Exists(shortcutPath))
            {
                try
                {
                    // WshShell requires a Reference and using statement
                    // Add Reference > COM > Windows Script Host Object Model &
                    // using IWshRuntimeLibrary;
                    WshShell shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                    shortcut.Description = "Shortcut for TrayLauncher";
                    shortcut.TargetPath = AppInfo.AppPath;
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
                    WriteLog.WriteTempFile("* Error creating shortcut.");
                    WriteLog.WriteTempFile($"* {ex.Message}");
                }
            }

            if (string.Equals(mode, "delete", StringComparison.OrdinalIgnoreCase) && File.Exists(shortcutPath))
            {
                try
                {
                    File.Delete(shortcutPath);
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Error deleting shortcut\n{ex.Message}",
                                            "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    WriteLog.WriteTempFile("* Error deleting shortcut.");
                    WriteLog.WriteTempFile($"* {ex.Message}");
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
                    WriteLog.WriteTempFile($"    Tray icon file is {myIcon}");
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
                _ = MessageBox.Show($"{myIcon} was not found.", "Icon File Missing",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile($"* {myIcon} was not found.");
            }
        }
        #endregion Get icon from Images folder

        #region Set tray icon color
        // Set Tray Icon
        private void SetTrayIcon(string iconFile)
        {
            switch (iconFile.ToLower())
            {
                case "lightskyblue":
                    {
                        IconFromFile(blueIcon);
                        break;
                    }
                case "dodgerblue":
                    {
                        IconFromFile(mediumblueIcon);
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
                case "lightgray":
                    {
                        IconFromFile(grayIcon);
                        break;
                    }
                case "lawngreen":
                    {
                        IconFromFile(greenIcon);
                        break;
                    }
                case "darkorange":
                    {
                        IconFromFile(orangeIcon);
                        break;
                    }
                case "red":
                    {
                        IconFromFile(redIcon);
                        break;
                    }
                case "teal":
                    {
                        IconFromFile(tealIcon);
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
                case "magenta":
                    {
                        IconFromFile(magentaIcon);
                        break;
                    }
                default:
                    {
                        IconFromFile(whiteIcon);
                        break;
                    }
            }
            Debug.WriteLine($"*** SetTrayIcon: Icon color is {iconFile}");
        }
        #endregion

        #region Set Menu background color
        public void SetMenuBackground(Color selectedColor)
        {
            // Set the background color of the tray menu from
            // https://www.dotnetcurry.com/wpf/1142/resources-wpf-static-dynamic-difference

            SolidColorBrush findsolidColorBrush = FindResource("brush1") as SolidColorBrush;
            findsolidColorBrush.Color = selectedColor;
            Resources.Remove("brush1");
            Resources.Add("brush1", findsolidColorBrush);
        }
        #endregion Set Menu background color

        #region Font size

        // Increase / Decrease font size - change datagrid row height so things don't look weird
        private void FontSmaller()
        {
            if (trayMenu.FontSize > 12)
            {
                trayMenu.FontSize--;
                theDataGrid.FontSize--;
                theDataGrid.RowHeight -= 2;
                UserSettings.Setting.FontSize = trayMenu.FontSize;
                Debug.WriteLine($"Font size is {trayMenu.FontSize}");
                WriteLog.WriteTempFile($"    Font size set to {trayMenu.FontSize}");
            }
        }

        private void FontLarger()
        {
            if (trayMenu.FontSize < 18)
            {
                trayMenu.FontSize++;
                theDataGrid.FontSize++;
                theDataGrid.RowHeight += 2;
                UserSettings.Setting.FontSize = trayMenu.FontSize;
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
                    _ = MessageBox.Show($"Backup failed\n{ex.Message}", "ERROR",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    WriteLog.WriteTempFile("* Backup failed.");
                    WriteLog.WriteTempFile($"* {ex.Message}");
                }
            }
        }
        #endregion Backup XML data

        #region Restore XML data

        private void RestoreXMLFile()
        {
            OpenFileDialog dlgOpen = new OpenFileDialog
            {
                Title = "Choose file to restore",
                CheckFileExists = true,
                Filter = "XML (*.xml|*.XML| All files (*.*)|*.*"
            };

            if (dlgOpen.ShowDialog() == true)
            {
                try
                {
                    File.Copy(dlgOpen.FileName, xmlMenuFile, true);
                    WriteLog.WriteTempFile($"  Menu file restored from {dlgOpen.FileName} ");
                    SortXMLFile();
                    trayMenu.Items.Clear();
                    ConstructMenu();
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Restore failed\n{ex.Message}", "ERROR",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    WriteLog.WriteTempFile($"* Menu restore from {dlgOpen.FileName} failed.");
                    WriteLog.WriteTempFile($"* {ex.Message}");
                }
            }
        }
        #endregion

        #region Alternate row shading
        // Shade alternate rows of the datagrid in the main window
        private void AltRowShadingOff()
        {
            theDataGrid.AlternationCount = 0;
            theDataGrid.RowBackground = new SolidColorBrush(Colors.White);
            theDataGrid.AlternatingRowBackground = new SolidColorBrush(Colors.White);
            theDataGrid.Items.Refresh();
        }
        private void AltRowShadingOn()
        {
            theDataGrid.AlternationCount = 1;
            theDataGrid.RowBackground = new SolidColorBrush(Colors.White);
            theDataGrid.AlternatingRowBackground = new SolidColorBrush(Colors.WhiteSmoke);
            theDataGrid.Items.Refresh();
        }
        #endregion Alternate row shading

        #region Show settings

        // Show settings in a custom window
        private static void ShowSettingsWindow()
        {
            ShowSettings showSettings = new ShowSettings
            {
                Owner = Application.Current.MainWindow
            };
            showSettings.Show();
        }
        #endregion

        #region Color index to color name
        // Convert color index to name
        public static Color ColorIndexToColor(int c)
        {
            var colors = typeof(Colors).GetProperties();
            return (Color)ColorConverter.ConvertFromString(colors[c].Name);
        }
        #endregion

        #region Double click delay
        // double-click delay time has moved to NativeMethods.cs
        #endregion

        #region Settings window
        private void OpenSettingsWindow()
        {
            Settings settings = new Settings
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Topmost = true
            };
            settings.Show();
        }
        #endregion

        #region Explicit Shutdown
        private void ExplicitShutdown()
        {
            App.ExplicitClose = true;
            Application.Current.Shutdown();
        }
        #endregion
    }
}