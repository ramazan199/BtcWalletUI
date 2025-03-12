using BtcWalletUI.Pages;
using BtcWalletUI.Utils.Mappers;
using BtcWalletUI.ViewModels;
using Prism.Ioc;
using ICoinMapper = BtcWalletUI.Utils.Mappers.ICoinMapper;

namespace BtcWalletUI
{
    public static class BtcWalletUiInit
    {
        public static void RegisterUiComponents(this IContainerRegistry containerRegistry)
        {
            RegisterMappers(containerRegistry);
            RegisterPages(containerRegistry);
        }

        private static void RegisterMappers(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ITransactionMapper, TransactionMapper>();
            containerRegistry.RegisterSingleton<ICoinMapper, CoinMapper>();
        }

        private static void RegisterPages(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<TxHistoryPage, TxHistoryViewModel>();
            containerRegistry.RegisterForNavigation<TxDetailsPopup, TxDetailsViewModel>();
            containerRegistry.RegisterForNavigation<CoinSelectionPopup, CoinSelectionViewModel>();
            containerRegistry.RegisterForNavigation<TransferPage, TransferViewModel>();
            containerRegistry.RegisterForNavigation<AddressSharingPage, AddressViewModel>();
        }
    }
}