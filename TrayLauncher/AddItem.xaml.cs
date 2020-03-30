// TrayLauncher - A customizable tray menu to launch applications, websites and folders.
#region using directives

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.Serialization;
using TKUtils;

#endregion using directives

namespace TrayLauncher
{
    // Logic for the AddItem dialog
    public partial class AddItem : Window
    {
        private const string specItemsXML = "SpecialItems.xml";
        private string xmlMenuFile;
        private int addCount = 0;
        private string itemType = string.Empty;
        private readonly List<Shortcut> cboxItems = new List<Shortcut>();

        public AddItem(int pos)
        {
            InitializeComponent();

            ReadSettings();

            LoadComboBox();

            CheckPos(pos);
        }

        #region Read Settings

        private void ReadSettings()
        {
            WriteLog.WriteTempFile("  Entering AddItem");
            lblStatus.Text = "Ready";
            lblStatus.Foreground = Brushes.SlateGray;
            xmlMenuFile = Properties.Settings.Default.XMLfile;
            FontSize = Properties.Settings.Default.FontSize;
            if (FontSize > 16)
            {
                FontSize = 16;
            }
            if (FontSize < 12)
            {
                FontSize = 12;
            }
            _ = tbAddHeader.Focus();
        }

        #endregion Read Settings

        #region Load ComboBox

        public void LoadComboBox()
        {
            try
            {
                // Location of XML file
                string sourceXML = specItemsXML;
                string currentFolder = Assembly.GetExecutingAssembly().Location;
                string inputXML = Path.Combine(Path.GetDirectoryName(currentFolder), sourceXML);

                // Deserialize the XML file
                XmlSerializer deserializer = new XmlSerializer(typeof(SpecialMenuItems));
                StreamReader reader = new StreamReader(inputXML);
                SpecialMenuItems special = (SpecialMenuItems)deserializer.Deserialize(reader);
                reader.Close();

                // Add a placeholder to the top of the combo box
                Shortcut ph = new Shortcut
                {
                    Name = "Select a menu item"
                };
                cboxItems.Add(ph);

                // Add items to combo box
                Debug.WriteLine($"  Adding items to ComboBox");
                foreach (Shortcut item in special.shortcuts)
                {
                    cboxItems.Add(item);
                    Debug.WriteLine($"   {item.Name} = {item.Path}");
                }
                cmbSpecial.ItemsSource = cboxItems;

                // set combo box to the first item
                cmbSpecial.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading or writing to special items file\n{ex.Message}",
                    "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile($"* Error reading or writing to special items file.");
                WriteLog.WriteTempFile($"* {ex.Message}");
            }
        }

        #endregion Load ComboBox

        #region Buttons

        // Add a menu item
        public void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Make sure that the required text boxes have been filled
            if (string.IsNullOrEmpty(tbAddHeader.Text))
            {
                SystemSounds.Asterisk.Play();
                _ = tbAddHeader.Focus();
                tbAddHeader.Background = Brushes.LemonChiffon;
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Text = "Menu Item Text can't be blank";
                return;
            }

            if (string.IsNullOrEmpty(tbAddAppPath.Text) && (string.IsNullOrEmpty(itemType) || itemType == "SMI"))
            {
                SystemSounds.Asterisk.Play();
                _ = tbAddAppPath.Focus();
                tbAddAppPath.Background = Brushes.LemonChiffon;
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Text = "Application Path can't be blank";
                return;
            }

            if (string.IsNullOrEmpty(tbAddPosition.Text))
            {
                SystemSounds.Asterisk.Play();
                _ = tbAddPosition.Focus();
                tbAddPosition.Background = Brushes.LemonChiffon;
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Text = "Position can't be blank";
                return;
            }

            string comment = BuildComment();
            string pos = tbAddPosition.Text;
            string header = tbAddHeader.Text;
            string apppath = tbAddAppPath.Text;
            string args = tbAddArguments.Text;
            string ttip = tbAddToolTip.Text;

            // Add to XML file
            XDocument xDoc = XDocument.Load(xmlMenuFile);
            xDoc.Elements("MenuList").First().Add(
                new XElement("TLMenuItem",
                    new XComment(comment),
                    new XElement("Position", pos),
                    new XElement("MenuHeader", header),
                    new XElement("AppPath", apppath),
                    new XElement("Arguments", args),
                    new XElement("ToolTip", ttip),
                    new XElement("Type", itemType)));
            xDoc.Save(xmlMenuFile);

            ReadyForNext(pos);

            WriteLog.WriteTempFile($"    Added menu item: Header: {header}, " +
                                   $"Position: {pos}, " +
                                   $"AppPath: {apppath}, " +
                                   $"Arguments: {args}, " +
                                   $"Tooltip: {ttip}, " +
                                   $"Type: {itemType}");
            addCount++;
            if (addCount == 1)
            {
                lblStatus.Text = "1 item added";
            }
            else
            {
                lblStatus.Text = $"{addCount} items added";
            }
        }

        // Close window when Exit button clicked
        private void BtnExitAdd_Click(object sender, RoutedEventArgs e)
        {
            switch (addCount)
            {
                case 0:
                    DialogResult = false;
                    break;

                default:
                    DialogResult = true;
                    break;
            }
            WriteLog.WriteTempFile($"  Leaving AddItem, {addCount} items added");
            Close();
        }

        #endregion Buttons

        #region Text box events

        // Change single underscore character to two in menu header
        private void TbAddHeader_LostFocus(object sender, RoutedEventArgs e)
        {
            HandleUnderscore((TextBox)sender);
        }

        // Make sure size text box only accepts numbers
        private void TbAddPosition_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only digits
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        // Enter will add item
        private void TbAddPosition_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnAdd.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            }
        }

        // File picker
        private void BtnOpenDlg_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog
            {
                Title = "Choose Application or Document",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "All files|*.*"
            };
            var result = dlgOpen.ShowDialog();
            if (result == true)
            {
                tbAddAppPath.Text = dlgOpen.FileName;
            }
        }

        #endregion Text box events

        #region ComboBox events

        // Combo box selection changed
        private void CmbSpecial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckRadioButton("");
            if (cmbSpecial.SelectedIndex == 0)
            {
                tbAddHeader.Text = string.Empty;
                tbAddAppPath.Text = string.Empty;
                tbAddArguments.Text = string.Empty;
            }
            else
            {
                Shortcut x = (Shortcut)cmbSpecial.SelectedItem;

                foreach (Shortcut item in cboxItems)
                {
                    if (item.Name == x.Name)
                    {
                        tbAddHeader.Text = item.Name;
                        tbAddAppPath.Text = item.Path;
                        tbAddArguments.Text = item.Args;
                        itemType = item.ItemType;
                        // If item type isn't blank, check the appropriate radio button
                        if (!string.IsNullOrEmpty(itemType))
                        {
                            CheckRadioButton(itemType.ToString());
                        }
                        break;
                    }
                }
            }
        }

        #endregion ComboBox events

        #region Radio buttons

        // Check radio button based on itemType
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton x = sender as RadioButton;

            switch (x.Name)
            {
                case "btnNormal":
                    itemType = "";
                    TextBoxNormal();
                    break;

                case "btnSep":
                    itemType = "SEP";
                    TextBoxDisable();
                    break;

                case "btnSH":
                    itemType = "SH";
                    TextBoxDisable();
                    break;

                case "btnSM":
                    itemType = "SM";
                    TextBoxDisable();
                    break;

                case "btnSMI":
                    itemType = "SMI";
                    TextBoxNormal();
                    break;

                default:
                    break;
            }
        }

        private void TextBoxDisable()
        {
            tbAddArguments.Background = Brushes.WhiteSmoke;
            tbAddToolTip.Background = Brushes.WhiteSmoke;
            tbAddArguments.IsEnabled = false;
            tbAddToolTip.IsEnabled = false;
            btnOpenDlg.IsEnabled = false;
        }

        private void TextBoxNormal()
        {
            tbAddArguments.Background = Brushes.White;
            tbAddToolTip.Background = Brushes.White;
            tbAddArguments.IsEnabled = true;
            tbAddToolTip.IsEnabled = true;
            btnOpenDlg.IsEnabled = true;
        }

        #endregion Radio buttons

        #region Helper Methods

        // Blank text boxes and set color values to be ready for next item
        private void ReadyForNext(string p)
        {
            int newPos = int.Parse(p) + 1;
            tbAddPosition.Text = newPos.ToString();
            tbAddHeader.Text = string.Empty;
            tbAddAppPath.Text = string.Empty;
            tbAddArguments.Text = string.Empty;
            tbAddToolTip.Text = string.Empty;
            itemType = string.Empty;
            lblStatus.Text = string.Empty;

            tbAddHeader.Background = Brushes.White;
            tbAddAppPath.Background = Brushes.White;
            tbAddPosition.Background = Brushes.White;
            lblStatus.Foreground = Brushes.SlateGray;
            lblStatus.FontWeight = FontWeights.Normal;

            cmbSpecial.SelectedIndex = 0;
            _ = tbAddHeader.Focus();
        }

        private string BuildComment()
        {
            // Build comment
            DateTime dt = DateTime.Now;
            return $"Added {dt}";
        }

        // Method to check specified radio button
        private void CheckRadioButton(string itype)
        {
            switch (itype)
            {
                case "SEP":
                    btnSep.IsChecked = true;
                    break;

                case "SH":
                    btnSH.IsChecked = true;
                    break;

                case "SM":
                    btnSM.IsChecked = true;
                    break;

                case "SMI":
                    btnSMI.IsChecked = true;
                    break;

                default:
                    btnNormal.IsChecked = true;
                    break;
            }
        }

        private void CheckPos(int pos)
        {
            int newPos = pos + 1;

            tbAddPosition.Text = newPos.ToString();

            // Deserialize the XML file
            XmlSerializer deserializer = new XmlSerializer(typeof(MenuList));
            StreamReader reader = new StreamReader(xmlMenuFile);
            MenuList items = (MenuList)deserializer.Deserialize(reader);
            reader.Close();

            foreach (TLMenuItem item in items.menuList)
            {
                if (item.Pos == newPos)
                {
                    lblStatus.Text = $"Position {newPos} already in use";
                    lblStatus.Foreground = Brushes.Red;
                    tbAddPosition.Background = Brushes.LemonChiffon;
                }
            }
        }

        // Change a single underscore to two but leave existing consecutive underscores alone
        private void HandleUnderscore(TextBox box)
        {
            if (box.Text.Contains("_"))
            {
                char[] array = box.Text.ToCharArray();
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < array.Length; i++)
                {
                    sb.Append(array[i]);

                    if ((i < array.Length - 1) && (i > 0))  // Not First or last character
                    {
                        if (array[i] == '_' && array[i + 1] != '_' && array[i - 1] != '_')
                        {
                            sb.Append("_");
                            Debug.WriteLine($"Not first or last: {array[i - 1]} {array[i]} {array[i + 1]}");
                        }
                    }
                    else if ((i == array.Length - 1) && (i > 0)) // Last character of many
                    {
                        if (array[i] == '_' && array[i - 1] != '_')
                        {
                            sb.Append("_");
                            Debug.WriteLine($"last of many: {array[i - 1]} {array[i]}");
                        }
                    }
                    else if ((i != array.Length - 1) && (i == 0)) // First character of many
                    {
                        if (array[i] == '_' && array[i + 1] != '_')
                        {
                            sb.Append("_");
                            Debug.WriteLine($"first of many: {array[i]} {array[i + 1]} ");
                        }
                    }
                    else if (array.Length == 1) // Only character
                    {
                        sb.Append("_");
                        Debug.WriteLine($"Only: {array[i]}");
                    }
                    else
                    {
                        Debug.WriteLine($"No action: {array[i]}");
                    }
                }
                box.Text = sb.ToString();
            }
        }
        #endregion Helper Methods
    }
}