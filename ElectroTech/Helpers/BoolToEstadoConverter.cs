using System;
using System.Globalization;
using System.Windows.Data;

namespace ElectroTech.Helpers
{
    /// <summary>
    /// Convertidor que transforma un valor booleano a un texto de estado.
    /// </summary>
    public class BoolToEstadoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Bajo Stock" : "OK";
            }

            return "Desconocido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = value as string;
            if (stringValue != null)
            {
                return stringValue.Equals("Bajo Stock", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}