// TrayLauncher - A customizable tray menu to launch applications, websites and folders.
#region using directives

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
    // Interaction logic for CopyItem.xaml
    public partial class CopyItem : Window
    {
        private const string specItemsXML = "SpecialItems.xml";
        private string xmlMenuFile;
        private string itemType;
        private readonly List<Shortcut> cboxItems = new List<Shortcut>();
        private readonly int index;

        public CopyItem(string header, string path, string args, string ttip, int pos, string itype, int idx)
        {
            InitializeComponent();

            ReadSettings();

            LoadComboBox();

            PreFillTextboxes(header, path, args, ttip, pos);

            CheckPos(pos);

            CheckRadioButton(itype);

            itemType = itype;
            index = idx;
        }

        #region Check Position

        private void CheckPos(int pos)
        {
            // Deserialize the XML file
            XmlSerializer deserializer = new XmlSerializer(typeof(MenuList));
            StreamReader reader = new StreamReader(xmlMenuFile);
            MenuList items = (MenuList)deserializer.Deserialize(reader);
            reader.Close();

            foreach (TLMenuItem item in items.menuList)
            {
                if (item.Pos == pos)
                {
                    lblStatus.Text = $"Position {pos} already in use";
                    lblStatus.Foreground = Brushes.Red;
                    tbCopyPosition.Background = Brushes.LemonChiffon;
                }
            }
        }

        #endregion Check Position

        #region Read Settings

        private void ReadSettings()
        {
            WriteLog.WriteTempFile("  Entering CopyItem");
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
            _ = tbCopyHeader.Focus();
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

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            // Make sure that the required text boxes have been filled
            if (string.IsNullOrEmpty(tbCopyHeader.Text))
            {
                SystemSounds.Asterisk.Play();
                _ = tbCopyHeader.Focus();
                tbCopyHeader.Background = Brushes.LemonChiffon;
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Text = "Menu Item Text can't be blank";
                return;
            }

            if (string.IsNullOrEmpty(tbCopyAppPath.Text) && (string.IsNullOrEmpty(itemType) || itemType == "SMI"))
            {
                SystemSounds.Asterisk.Play();
                _ = tbCopyAppPath.Focus();
                tbCopyAppPath.Background = Brushes.LemonChiffon;
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Text = "Application Path can't be blank";
                return;
            }

            if (string.IsNullOrEmpty(tbCopyPosition.Text))
            {
                SystemSounds.Asterisk.Play();
                _ = tbCopyPosition.Focus();
                tbCopyPosition.Background = Brushes.LemonChiffon;
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Text = "Position can't be blank";
                return;
            }

            try
            {
                string comment = BuildComment();

                XDocument xDoc = XDocument.Load(xmlMenuFile);

                // Then add the Copied item
                xDoc.Elements("MenuList").First().Add(
                    new XElement("TLMenuItem",
                        new XComment(comment),
                        new XElement("Position", tbCopyPosition.Text),
                        new XElement("MenuHeader", tbCopyHeader.Text),
                        new XElement("AppPath", tbCopyAppPath.Text),
                        new XElement("Arguments", tbCopyArguments.Text),
                        new XElement("ToolTip", tbCopyToolTip.Text),
                        new XElement("Type", itemType)));
                xDoc.Save(xmlMenuFile);

                WriteLog.WriteTempFile($"    Copied menu item at index {index} ");
                WriteLog.WriteTempFile($"    Updated menu item: Header: {tbCopyHeader.Text}, " +
                                       $"Position: {tbCopyPosition.Text}, " +
                                       $"AppPath: {tbCopyAppPath.Text}, " +
                                       $"Arguments: {tbCopyArguments.Text}, " +
                                       $"Tooltip: {tbCopyToolTip.Text}, " +
                                       $"Type: {itemType} ");
                WriteLog.WriteTempFile("  Leaving CopyItem");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading or writing to menu file\n{ex.Message}",
                                     "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile($"* Error reading or writing to menu file.");
                WriteLog.WriteTempFile($"* {ex.Message}");
            }
        }

        private void BtnExitCopy_Click(object sender, RoutedEventArgs e)
        {
            WriteLog.WriteTempFile("  Leaving CopyItem");
            DialogResult = false;
            Close();
        }

        #endregion Buttons

        #region Text Box events

        private void TbCopyHeader_LostFocus(object sender, RoutedEventArgs e)
        {
            HandleUnderscore((TextBox)sender);
        }

        private void TbCopyPosition_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only digits
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void TbCopyPosition_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnCopy.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        #endregion Text Box events

        #region ComboBox events

        // Combo box selection changed
        private void CmbSpecial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckRadioButton("");
            if (cmbSpecial.SelectedIndex == 0)
            {
                tbCopyHeader.Text = string.Empty;
                tbCopyAppPath.Text = string.Empty;
                tbCopyArguments.Text = string.Empty;
            }
            else
            {
                Shortcut x = (Shortcut)cmbSpecial.SelectedItem;

                foreach (Shortcut item in cboxItems)
                {
                    if (item.Name == x.Name)
                    {
                        tbCopyHeader.Text = item.Name;
                        tbCopyAppPath.Text = item.Path;
                        tbCopyArguments.Text = item.Args;
                        itemType = item.ItemType;
                        // If item type isn't blank, check the appropriate radio button
                        if (!string.IsNullOrEmpty(itemType))
                        {
                            CheckRadioButton(itemType.ToString());
                        }
                        else
                        {
                            CheckRadioButton("");
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
            tbCopyArguments.Background = Brushes.WhiteSmoke;
            tbCopyToolTip.Background = Brushes.WhiteSmoke;
            tbCopyArguments.IsEnabled = false;
            tbCopyToolTip.IsEnabled = false;
        }

        private void TextBoxNormal()
        {
            tbCopyArguments.Background = Brushes.White;
            tbCopyToolTip.Background = Brushes.White;
            tbCopyArguments.IsEnabled = true;
            tbCopyToolTip.IsEnabled = true;
        }

        #endregion Radio buttons

        #region Helper methods

        private string BuildComment()
        {
            // Build comment
            DateTime dt = DateTime.Now;
            return $"Updated {dt}";
        }

        private void PreFillTextboxes(string he, string pa, string ar, string tt, int po)
        {
            tbCopyHeader.Text = he;
            tbCopyAppPath.Text = pa;
            tbCopyArguments.Text = ar;
            tbCopyToolTip.Text = tt;
            tbCopyPosition.Text = po.ToString();
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

        #endregion Helper methods
    }
}