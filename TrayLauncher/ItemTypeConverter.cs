// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System;
using System.Globalization;
using System.Windows.Data;

namespace TrayLauncher
{
    public class ItemTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case "SH":
                    return "Section Header";
                case "SM":
                    return "Sub Menu";
                case "SMI":
                    return "Sub Menu Item";
                case "SEP":
                    return "Separator";
                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
