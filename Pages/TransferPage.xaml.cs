using BtcWalletLibrary.Interfaces;
using BtcWalletUI.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BtcWalletUI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransferPage : BasePage
    {
        public TransferPage()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is TransferViewModel viewModel)
            {
                await viewModel.GetBitFeeAsync();
            }
        }
    }
}