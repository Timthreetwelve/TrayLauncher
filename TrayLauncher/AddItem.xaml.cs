//
// TrayLauncher - A customizable tray menu to launch applications, websites and folders.
//
#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Windows.Controls;
using TKUtils;
using System.Windows.Media.Imaging;
using System.Reflection;

#endregion Using directives

namespace TrayLauncher
{
    // Logic for the AddItem dialog
    public partial class AddItem : Window
    {
        #region Icon filename variables
        private const string blueIcon = "pack://application:,,,/Images/blue.ico";
        private const string blackIcon = "pack://application:,,,/Images/black.ico";
        private const string cyanIcon = "pack://application:,,,/Images/cyan.ico";
        private const string greenIcon = "pack://application:,,,/Images/green.ico";
        private const string orangeIcon = "pack://application:,,,/Images/orange.ico";
        private const string redIcon = "pack://application:,,,/Images/red.ico";
        private const string whiteIcon = "pack://application:,,,/Images/white.ico";
        private const string yellowIcon = "pack://application:,,,/Images/yellow.ico";
        #endregion

        SpecialMenuItems special;
        private const string specItemsXML = "SpecialItems.xml";
        private string xmlMenuFile;
        private int addCount = 0;

        public AddItem()
        {
            InitializeComponent();

            ReadSettings();

            LoadComboBox();
        }

        #region Load ComboBox
        public void LoadComboBox()
        {
            try
            {
                string sourceXML = specItemsXML;
                string currentFolder = Assembly.GetExecutingAssembly().Location;
                string inputXML = Path.Combine(Path.GetDirectoryName(currentFolder), sourceXML);

                XmlSerializer deserializer = new XmlSerializer(typeof(SpecialMenuItems));
                StreamReader reader = new StreamReader(inputXML);
                special = (SpecialMenuItems)deserializer.Deserialize(reader);
                reader.Close();

                List<string> cboxItems = new List<string>();
                cboxItems.Add("Select a menu item");
                foreach (Shortcut item in special.shortcuts)
                {
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        Debug.WriteLine($"   {item.Name} = {item.Path}");
                        cboxItems.Add(item.Name.ToString());
                    }
                }
                cmbSpecial.ItemsSource = cboxItems;
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading or writing to special items file\n{ex.Message}",
                    "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteLog.WriteTempFile($"Error reading or writing to special items file.");
                WriteLog.WriteTempFile(ex.Message);
            }
        }
        #endregion

        #region Read Settings
        private void ReadSettings()
        {
            lblStatus.Text = "Ready";
            lblStatus.Foreground = Brushes.SlateGray;

            xmlMenuFile = Properties.Settings.Default.XMLfile;
            FontSize = Properties.Settings.Default.FontSize;

            // Icon file
            //string iconFile = Properties.Settings.Default.Icon;
            //switch (iconFile.ToLower())
            //{
            //    case "blue":
            //        {
            //            IconFromFile(blueIcon);
            //            break;
            //        }
            //    case "black":
            //        {
            //            IconFromFile(blackIcon);
            //            break;
            //        }
            //    case "cyan":
            //        {
            //            IconFromFile(cyanIcon);
            //            break;
            //        }
            //    case "green":
            //        {
            //            IconFromFile(greenIcon);
            //            break;
            //        }
            //    case "red":
            //        {
            //            IconFromFile(redIcon);
            //            break;
            //        }
            //    case "white":
            //        {
            //            IconFromFile(whiteIcon);
            //            break;
            //        }
            //    case "yellow":
            //        {
            //            IconFromFile(yellowIcon);
            //            break;
            //        }
            //    default:
            //        {
            //            IconFromFile(orangeIcon);
            //            break;
            //        }
            //}
        }
        #endregion Read Settings

        #region Buttons
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

            if (string.IsNullOrEmpty(tbAddAppPath.Text))
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
                lblStatus.Text = "Position can't be blank.";
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
                    new XElement("ToolTip", ttip)));
            xDoc.Save(xmlMenuFile);

            ReadyForNext();
            WriteLog.WriteTempFile($"  Added menu item: Header: {header}, " +
                                   $"Position: {pos}, " +
                                   $"AppPath: {apppath}, " +
                                   $"Arguments: {args}, " +
                                   $"Tooltip: {ttip} ");
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
            Close();
        }

        #endregion Buttons

        #region Text box events

        // Make sure size text box only accepts numbers
        private void TbAddPosition_PreviewTextInput_1(object sender,
            System.Windows.Input.TextCompositionEventArgs e)
        {
            // Only digits
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        #endregion Text box events

        #region Helper Methods

        private void ReadyForNext()
        {
            tbAddHeader.Text = string.Empty;
            tbAddAppPath.Text = string.Empty;
            tbAddPosition.Text = string.Empty;
            tbAddArguments.Text = string.Empty;
            tbAddToolTip.Text = string.Empty;
            lblStatus.Text = string.Empty;

            tbAddHeader.Background = Brushes.White;
            tbAddAppPath.Background = Brushes.White;
            tbAddPosition.Background = Brushes.White;
            lblStatus.Foreground = Brushes.SlateGray;
            lblStatus.FontWeight = FontWeights.Normal;

            _ = tbAddHeader.Focus();
        }

        private string BuildComment()
        {
            // Build comment
            DateTime dt = DateTime.Now;
            return $"Added {dt}";
        }

        //public void IconFromFile(string iconFile)
        //{
        //    Uri iconUri = new Uri(iconFile, UriKind.RelativeOrAbsolute);
        //    this.Icon = BitmapFrame.Create(iconUri);
        //}
        #endregion Helpers

        #region ComboBox events
        private void CmbSpecial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSpecial.SelectedIndex == 0)
            {
                tbAddHeader.Text = string.Empty;
                tbAddAppPath.Text = string.Empty;
                tbAddArguments.Text = string.Empty;
            }
            else
            {
                foreach (var item in special.shortcuts)
                {
                    if (item.Name == cmbSpecial.SelectedItem.ToString())
                    {
                        tbAddHeader.Text = item.Name;
                        tbAddAppPath.Text = item.Path;
                        tbAddArguments.Text = item.Args;
                    }
                }
            }
        }
        #endregion
    }
}
