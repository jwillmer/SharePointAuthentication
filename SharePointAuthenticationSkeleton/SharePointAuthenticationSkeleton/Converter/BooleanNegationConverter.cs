using System;
using Windows.UI.Xaml.Data;

namespace SharePointAuthenticationSkeleton.Converter
{
    /// <summary>
    /// Wertkonverter, der TRUE in FALSE übersetzt und umgekehrt.
    /// </summary>
    public sealed class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }
    }
}
