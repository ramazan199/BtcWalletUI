namespace BtcWalletUI.Utils.Converters
{
    using System;
    using System.Globalization;
    using Xamarin.Forms;

    public class UnixDateConverter : IValueConverter
    {
        // Unix epoch (January 1, 1970 UTC)
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                // Compare in UTC to avoid timezone issues
                DateTime utcDate = date.ToUniversalTime();

                if (utcDate <= UnixEpoch)
                    return "N/A"; // Return "N/A" for Unix epoch
                else
                    return utcDate.ToString((string)parameter ?? "MM/dd/yyyy HH:mm"); // Use provided format
            }
            return "N/A"; // Fallback for invalid data
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
