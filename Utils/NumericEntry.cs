using DryIoc;
using System.Linq;
using Xamarin.Forms;

namespace BtcWalletUI.Utils
{
    public class NumericEntry : Entry
    {
        public NumericEntry()
        {
            TextChanged += (sender, e) =>
            {
                if (sender is Entry entry)
                {
                    if (entry.Text == string.Empty) return;
                    if (decimal.TryParse(entry.Text, out _)) return;
                    entry.Text = e.OldTextValue;
                }
            };
        }
    }
}
