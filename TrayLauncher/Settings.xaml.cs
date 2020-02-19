using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace TrayLauncher
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            LoadComboBoxes();

            DebugMode();
        }

        #region Disable if debug
        private void DebugMode()
        {
            // Disable start with windows when in debug mode
            if (System.Diagnostics.Debugger.IsAttached)
            {
                cbStartWW.IsEnabled = false;
            }
        }
        #endregion

        #region Load combo boxes
        private void LoadComboBoxes()
        {
            CmbBackground.ItemsSource = typeof(Colors).GetProperties();

            CmbForeground.ItemsSource = typeof(Colors).GetProperties();

            CmbSeparator.ItemsSource = typeof(Colors).GetProperties();

            CmbHeader.ItemsSource = typeof(Colors).GetProperties();

            LoadIconColors();
        }
        #endregion Load combo boxes

        #region Load Icon colors
        private void LoadIconColors()
        {
            List<string> iconList = new List<string>
            {
                "Black",
                "Cyan",
                "DarkOrange",
                "DodgerBlue",
                "LawnGreen",
                "LightGray",
                "LightSkyBlue",
                "Magenta",
                "Red",
                "Teal",
                "White",
                "Yellow"
            };
            cmbTrayIcon.ItemsSource = iconList;
        }
        #endregion Load Icon colors

        #region Window Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
        #endregion Window Events

        #region Color themes
        private void RbLight_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BackColor = 47;
            Properties.Settings.Default.ForeColor = 7;
            Properties.Settings.Default.SectionHeaderColor = 7;
            Properties.Settings.Default.SeparatorColor = 50;
        }

        private void RbDark_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BackColor = 40;
            Properties.Settings.Default.ForeColor = 67;
            Properties.Settings.Default.SectionHeaderColor = 7;
            Properties.Settings.Default.SeparatorColor = 125;
        }

        private void RbBlues_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BackColor = 72;
            Properties.Settings.Default.ForeColor = 9;
            Properties.Settings.Default.SectionHeaderColor = 115;
            Properties.Settings.Default.SeparatorColor = 74;
        }

        private void RbBanana_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BackColor = 62;
            Properties.Settings.Default.ForeColor = 40;
            Properties.Settings.Default.SectionHeaderColor = 85;
            Properties.Settings.Default.SeparatorColor = 24;
        }


        private void RbCherry_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BackColor = 69;
            Properties.Settings.Default.ForeColor = 113;
            Properties.Settings.Default.SectionHeaderColor = 38;
            Properties.Settings.Default.SeparatorColor = 89;
        }

        private void RbSpring_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BackColor = 103;
            Properties.Settings.Default.ForeColor = 44;
            Properties.Settings.Default.SectionHeaderColor = 77;
            Properties.Settings.Default.SeparatorColor = 51;
        }
        #endregion
    }
}