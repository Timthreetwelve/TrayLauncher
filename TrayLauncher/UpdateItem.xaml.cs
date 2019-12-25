//
// TrayLauncher - A customizable tray menu to launch applications, websites and folders.
//
#region using directives
using System;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using TKUtils;
#endregion

namespace TrayLauncher
{
    /// <summary>
    /// Interaction logic for UpdateItem.xaml
    /// </summary>
    public partial class UpdateItem : Window
    {
        private string xmlMenuFile;
        private readonly int index;

        public UpdateItem(string header, string path, string args, string ttip, int pos, int idx)
        {
            InitializeComponent();

            ReadSettings();

            PreFillTextboxes(header, path, args, ttip, pos);

            index = idx;
        }



        #region Read Settings
        private void ReadSettings()
        {
            WriteLog.WriteTempFile("  Entering UpdateItem");
            xmlMenuFile = Properties.Settings.Default.XMLfile;
            FontSize = Properties.Settings.Default.FontSize;
        }
        #endregion Read Settings

        #region Buttons
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Make sure that the required text boxes have been filled
            if (string.IsNullOrEmpty(tbUpdateHeader.Text))
            {
                SystemSounds.Asterisk.Play();
                _ = tbUpdateHeader.Focus();
                tbUpdateHeader.Background = Brushes.LemonChiffon;
                lblStatus.Foreground = Brushes.Red;
                lblStatus.FontWeight = FontWeights.Bold;
                lblStatus.Text = "Menu Item Text can't be blank";
                return;
            }

            if (string.IsNullOrEmpty(tbUpdateAppPath.Text))
            {
                SystemSounds.Asterisk.Play();
                _ = tbUpdateAppPath.Focus();
                tbUpdateAppPath.Background = Brushes.LemonChiffon;
                lblStatus.Foreground = Brushes.Red;
                lblStatus.FontWeight = FontWeights.Bold;
                lblStatus.Text = "Application Path can't be blank";
                return;
            }

            if (string.IsNullOrEmpty(tbUpdatePosition.Text))
            {
                SystemSounds.Asterisk.Play();
                _ = tbUpdatePosition.Focus();
                tbUpdatePosition.Background = Brushes.LemonChiffon;
                lblStatus.Foreground = Brushes.Red;
                lblStatus.FontWeight = FontWeights.Bold;
                lblStatus.Text = "Position can't be blank";
                return;
            }

            try
            {
                string comment = BuildComment();

                XDocument xDoc = XDocument.Load(xmlMenuFile);

                // First remove the item
                var item = xDoc.Descendants("TLMenuItem").ElementAt(index);
                item.Remove();

                // Then add the updated item
                xDoc.Elements("MenuList").First().Add(
                    new XElement("TLMenuItem",
                        new XComment(comment),
                        new XElement("Position", tbUpdatePosition.Text),
                        new XElement("MenuHeader", tbUpdateHeader.Text),
                        new XElement("AppPath", tbUpdateAppPath.Text),
                        new XElement("Arguments", tbUpdateArguments.Text),
                        new XElement("ToolTip", tbUpdateToolTip.Text)));
                xDoc.Save(xmlMenuFile);

                WriteLog.WriteTempFile($"    Updating menu item at index {index} ");
                WriteLog.WriteTempFile($"    Updated menu item: Header: {tbUpdateHeader.Text}, " +
                                       $"Position: {tbUpdatePosition.Text}, " +
                                       $"AppPath: {tbUpdateAppPath.Text}, " +
                                       $"Arguments: {tbUpdateArguments.Text}, " +
                                       $"Tooltip: {tbUpdateToolTip.Text} ");
                WriteLog.WriteTempFile("  Leaving UpdateItem");
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

        private void BtnExitUpdate_Click(object sender, RoutedEventArgs e)
        {
            WriteLog.WriteTempFile("  Leaving UpdateItem");
            Close();
        }
        #endregion

        #region Text Box events
        private void TbUpdatePosition_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only digits
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }
        #endregion

        #region Helper methods
        private string BuildComment()
        {
            // Build comment
            DateTime dt = DateTime.Now;
            return $"Updated {dt}";
        }

        private void PreFillTextboxes(string he, string pa, string ar, string tt, int po)
        {
            tbUpdateHeader.Text = he;
            tbUpdateAppPath.Text = pa;
            tbUpdateArguments.Text = ar;
            tbUpdateToolTip.Text = tt;
            tbUpdatePosition.Text = po.ToString();
        }

        #endregion
    }
}
