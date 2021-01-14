// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
#endregion Using directives

namespace TrayLauncher
{
    public partial class ShowSettings : Window
    {
        private static readonly SortedDictionary<string, string> mySettings =
            new SortedDictionary<string, string>();

        public ShowSettings()
        {
            InitializeComponent();
            GetSettings();
            GetSystemParameters();
            dg1.ItemsSource = mySettings;
        }

        #region Get System Parameters
        private static void GetSystemParameters()
        {
            mySettings.Add("System - Double-click Delay", $"{ NativeMethods.GetDoubleClickTime()} ms");
            mySettings.Add("System - Menu Delay", $"{SystemParameters.MenuShowDelay} ms");
            mySettings.Add("System - Primary Screen Height", SystemParameters.PrimaryScreenHeight.ToString());
            mySettings.Add("System - Primary Screen Width", SystemParameters.PrimaryScreenWidth.ToString());
        }
        #endregion Get System Parameters

        #region Get Settings
        private static void GetSettings()
        {
            mySettings.Clear();
            foreach (KeyValuePair<string, object> property in UserSettings.ListSettings())
            {
                string key;
                string value;
                switch (property.Key)
                {
                    case "BackColor":
                    case "ForeColor":
                    case "SectionHeaderColor":
                    case "SeparatorColor":
                        key = property.Key;
                        value = $"Index {property.Value} = " +
                           $"{ColorIndexToName((int)property.Value)}";
                        break;
                    case "XMLFile":
                        key = "Menu Items File";
                        value = property.Value.ToString();
                        break;
                    default:
                        key = property.Key;
                        value = property.Value.ToString();
                        break;
                }
                mySettings.Add(key, value);
            }
        }
        #endregion Get Settings

        #region Button Events
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion Button Events

        #region Keyboard Events
        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }
        }
        #endregion Keyboard Events

        #region Color Index to Color Name
        public static string ColorIndexToName(int c)
        {
            PropertyInfo[] colors = typeof(Colors).GetProperties();
            return colors[c].Name;
        }
        #endregion Color Index to Color Name
    }
}
