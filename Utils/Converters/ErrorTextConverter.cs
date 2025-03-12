using System;
using System.Globalization;
using Xamarin.Forms;

namespace BtcWalletUI.Utils.Converters
{
    public class ErrorTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value as string ?? string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}