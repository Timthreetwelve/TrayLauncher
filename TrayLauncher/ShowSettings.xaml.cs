using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace TrayLauncher
{
    /// <summary>
    /// Interaction logic for ShowSettings.xaml
    /// </summary>
    public partial class ShowSettings : Window
    {
        public ShowSettings()
        {
            InitializeComponent();

            Refresh();
        }

        private void Refresh()
        {
            List<MySettings> settings = ReadSettings();

            GetDoubleClick(settings);

            //dg1.Items.Clear();

            dg1.ItemsSource = settings.OrderBy(x => x.Name);
        }

        private static void GetDoubleClick(List<MySettings> settings)
        {
            MySettings dd = new MySettings
            {
                Name = "Double-click Delay",
                Value = $"{GetDoubleClickTime().ToString()} ms"
            };
            settings.Add(dd);
        }

        private static List<MySettings> ReadSettings()
        {
            List<MySettings> settings = new List<MySettings>();
            foreach (SettingsPropertyValue property in Properties.Settings.Default.PropertyValues)
            {
                MySettings show = new MySettings();
                switch (property.Name)
                {
                    case "BackColor":
                        {
                            // adjust for removal of Transparent
                            if ((int)property.PropertyValue < 133)
                            {
                                show.Value = $"Index {property.PropertyValue} = " +
                                $"{ColorIndexToName((int)property.PropertyValue)}";
                            }
                            else
                            {
                                show.Value = $"Index {property.PropertyValue} = " +
                                $"{ColorIndexToName((int)property.PropertyValue + 1)}";
                            }
                            break;
                        }
                    case "ForeColor":
                        {
                            show.Value = $"Index {property.PropertyValue} = " +
                                $"{ColorIndexToName((int)property.PropertyValue)}";
                            break;
                        }
                    case "SectionHeaderColor":
                        {
                            show.Value = $"Index {property.PropertyValue} = " +
                                $"{ColorIndexToName((int)property.PropertyValue)}";
                            break;
                        }
                    case "SeparatorColor":
                        {
                            show.Value = $"Index {property.PropertyValue} = " +
                                $"{ColorIndexToName((int)property.PropertyValue)}";
                            break;
                        }
                    default:
                        {
                            show.Value = property.PropertyValue.ToString();
                            break;
                        }
                }
                show.Name = property.Name;
                settings.Add(show);
            }

            return settings;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static string ColorIndexToName(int c)
        {
            var colors = typeof(Colors).GetProperties();
            return colors[c].Name;
        }

        // Get double-click delay time in milliseconds
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetDoubleClickTime();

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                Refresh();
            }

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }
        }
    }
}
