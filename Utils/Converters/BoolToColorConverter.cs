using System;
using System.Globalization;
using Xamarin.Forms;

namespace BtcWalletUI.Utils.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOutgoing)
            {
                return isOutgoing ? Color.Red : Color.Green; // Red for outgoing, Green for incoming
            }
            return Color.Transparent; // Default case
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
