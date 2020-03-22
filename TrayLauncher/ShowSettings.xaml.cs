using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

            GetSystemParameters(settings);

            dg1.ItemsSource = settings.OrderBy(x => x.Name);
        }

        private static void GetDoubleClick(List<MySettings> settings)
        {
            MySettings dd = new MySettings
            {
                Name = "Double-click Delay",
                Value = $"{NativeMethods.GetDoubleClickTime().ToString()} ms"
            };
            settings.Add(dd);
        }

        private static void GetSystemParameters(List<MySettings> settings)
        {
            MySettings sp1 = new MySettings
            {
                Name = "Mouse Present",
                Value = $"{SystemParameters.IsMousePresent.ToString()}"
            };
            settings.Add(sp1);

            MySettings sp2 = new MySettings
            {
                Name = "Menu Delay",
                Value = $"{SystemParameters.MenuShowDelay.ToString()} ms"
            };
            settings.Add(sp2);

            MySettings sp3 = new MySettings
            {
                Name = "Main Window Max Height",
                Value = $"{(SystemParameters.PrimaryScreenHeight -20).ToString()}"
            };
            settings.Add(sp3);

            MySettings sp4 = new MySettings
            {
                Name = "Primary Screen Height",
                Value = $"{SystemParameters.PrimaryScreenHeight.ToString()}"
            };
            settings.Add(sp4);

            MySettings sp5 = new MySettings
            {
                Name = "Primary Screen Width",
                Value = $"{SystemParameters.PrimaryScreenWidth.ToString()}"
            };
            settings.Add(sp5);
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
                            show.Value = $"Index {property.PropertyValue} = " +
                                $"{ColorIndexToName((int)property.PropertyValue)}";
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
