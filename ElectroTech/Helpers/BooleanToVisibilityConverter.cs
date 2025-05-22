using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ElectroTech.Helpers
{
    /// <summary>
    /// Converts a boolean value to a Visibility value.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = false;
            if (value is bool)
            {
                boolValue = (bool)value;
            }
            else if (value is Nullable<bool>)
            {
                Nullable<bool> nullable = (Nullable<bool>)value;
                boolValue = nullable.GetValueOrDefault();
            }
            if (parameter != null && parameter.ToString() == "Inverse")
            {
                boolValue = !boolValue;
            }
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility value to a boolean value.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            bool result = visibility == Visibility.Visible;
            if (parameter != null && parameter.ToString() == "Inverse")
            {
                result = !result;
            }
            return result;
        }
    }

    /// <summary>
    /// Converts a boolean value to a Visibility value (inverse).
    /// </summary>
    public class InverseBooleanToVisibilityConverter : BooleanToVisibilityConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value (inverse).
        /// </summary>
        public new object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return base.Convert(value, targetType, "Inverse", culture);
        }

        /// <summary>
        /// Converts a Visibility value to a boolean value (inverse).
        /// </summary>
        public new object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return base.ConvertBack(value, targetType, "Inverse", culture);
        }
    }
}