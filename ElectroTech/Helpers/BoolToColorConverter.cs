using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ElectroTech.Helpers
{
    /// <summary>
    /// Convertidor que transforma un valor booleano a un color.
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Si requiere reposición (true), devolver rojo; si no, verde
                return boolValue ? new SolidColorBrush(Color.FromRgb(211, 47, 47)) : new SolidColorBrush(Color.FromRgb(56, 142, 60));
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}