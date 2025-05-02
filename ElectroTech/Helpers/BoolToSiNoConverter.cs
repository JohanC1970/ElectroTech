using System;
using System.Globalization;
using System.Windows.Data;

namespace ElectroTech.Helpers
{
    /// <summary>
    /// Convertidor que transforma un valor booleano a "Sí" o "No".
    /// </summary>
    public class BoolToSiNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Sí" : "No";
            }

            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = value as string;
            if (stringValue != null)
            {
                return stringValue.Equals("Sí", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}