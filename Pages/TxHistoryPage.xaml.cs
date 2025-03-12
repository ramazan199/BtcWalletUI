using BtcWalletUI.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BtcWalletUI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TxHistoryPage : ContentPage
    {

        public TxHistoryPage()
        {
            InitializeComponent();
        }
        private bool _isFirstTxFetching = true;

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_isFirstTxFetching) return;
            if (BindingContext is not TxHistoryViewModel viewModel) return;
            _isFirstTxFetching = false;
            await viewModel.FetchTransactionsAsync();
            viewModel.UpdateBalance();
        }
    }
}