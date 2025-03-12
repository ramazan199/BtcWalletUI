using Xamarin.Forms;

namespace BtcWalletUI.Pages
{
    public class BasePage : ContentPage
    {
        protected BasePage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}