using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ZohoPeopleTimeLogger.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public BoolToVisibilityConverter()
        {
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }

        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            return boolValue ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityValue = (Visibility) value;

            if (visibilityValue == TrueValue)
            {
                return true;
            }

            if (visibilityValue == FalseValue)
            {
                return false;
            }

            throw new ArgumentException(string.Format("Cannot convert {0} to bool", visibilityValue), "value");
        }
    }
}