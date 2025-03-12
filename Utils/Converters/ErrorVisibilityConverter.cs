using System;
using System.Globalization;
using Xamarin.Forms;

namespace BtcWalletUI.Utils.Converters
{
    // Converters
    public class ErrorVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            !string.IsNullOrEmpty(value as string);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}