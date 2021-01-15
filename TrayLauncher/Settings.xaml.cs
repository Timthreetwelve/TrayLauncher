// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
#endregion Using directives

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
        #endregion Disable if debug

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
            List<string> iconColors = new List<string>
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
            cmbTrayIcon.ItemsSource = iconColors;

            cmbTrayIcon.SelectedIndex = iconColors.FindIndex(c => c.Equals(UserSettings.Setting.Icon, StringComparison.OrdinalIgnoreCase));
        }
        #endregion Load Icon colors

        #region Window Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserSettings.SaveSettings();
        }
        #endregion Window Events

        #region Color themes
        private void RbLight_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.BackColor = 47;
            UserSettings.Setting.ForeColor = 7;
            UserSettings.Setting.SectionHeaderColor = 7;
            UserSettings.Setting.SeparatorColor = 50;
        }

        private void RbDark_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.BackColor = 40;
            UserSettings.Setting.ForeColor = 67;
            UserSettings.Setting.SectionHeaderColor = 7;
            UserSettings.Setting.SeparatorColor = 125;
        }

        private void RbBlues_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.BackColor = 72;
            UserSettings.Setting.ForeColor = 9;
            UserSettings.Setting.SectionHeaderColor = 115;
            UserSettings.Setting.SeparatorColor = 74;
        }

        private void RbBanana_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.BackColor = 62;
            UserSettings.Setting.ForeColor = 40;
            UserSettings.Setting.SectionHeaderColor = 85;
            UserSettings.Setting.SeparatorColor = 24;
        }

        private void RbCherry_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.BackColor = 69;
            UserSettings.Setting.ForeColor = 113;
            UserSettings.Setting.SectionHeaderColor = 38;
            UserSettings.Setting.SeparatorColor = 89;
        }

        private void RbSpring_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.BackColor = 103;
            UserSettings.Setting.ForeColor = 44;
            UserSettings.Setting.SectionHeaderColor = 77;
            UserSettings.Setting.SeparatorColor = 51;
        }
        #endregion Color themes
    }
}