using System;
using System.Globalization;
using Xamarin.Forms;

namespace BtcWalletUI.Utils.Converters
{
    public class ListToHeightConverter : IValueConverter
    {
        public double RowHeight { get; set; } = 50; // Adjust as needed

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count * RowHeight;
            }
            return RowHeight; // Default height if count is not available
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
