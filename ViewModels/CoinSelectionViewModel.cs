using BtcWalletLibrary.Interfaces;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using UnspentCoin = BtcWalletLibrary.Models.UnspentCoin;
using ICoinMapper = BtcWalletUI.Utils.Mappers.ICoinMapper;

namespace BtcWalletUI.ViewModels
{
    class CoinSelectionViewModel : BaseViewModel
    {
        private List<UnspentCoin> _selectedUnspentCoins;
        private readonly IBalanceService _balanceService;
        private readonly ICoinMapper _coinMapper;

        public ICommand CoinSelectionChangedCommand { get; }
        public ICommand CoinSelectionConfirmCommand { get; }
        public ICommand CoinSelectionCancelCommand { get; }

        public ObservableCollection<UnspentCoin> UnspentCoins { get; private set; }
        public bool IsUnspentCoinListEmpty { get => UnspentCoins.Count == 0; }


        private readonly INavigationService _navigationService;

        public CoinSelectionViewModel(INavigationService navigationService, IBalanceService balanceService, ICoinMapper coinMapper)
        {
            CoinSelectionChangedCommand = new Command<CollectionView>(CoinSelectionChangedCmdExecuted);
            CoinSelectionConfirmCommand = new Command<EventArgs>(async (e) => await CoinSelectionConfirmCmdExecutedAsync(e));
            CoinSelectionCancelCommand = new Command<EventArgs>(async (e) => await CoinSelectionCancelCmdExecutedAsync(e));
            _navigationService = navigationService;
            _balanceService = balanceService;
            _coinMapper = coinMapper;

            InitUnspentCoins();
        }

        private void InitUnspentCoins()
        {
            UnspentCoins =
                [.. _balanceService.Utxos
                    .Select(u => _coinMapper.UtxoToUnspentCoin(u)).ToList()];
        }

        private void CoinSelectionChangedCmdExecuted(CollectionView collectionView)
        {
            if (collectionView != null)
            {
                _selectedUnspentCoins = [.. collectionView.SelectedItems.Cast<UnspentCoin>()];
            }
        }

        private async Task CoinSelectionConfirmCmdExecutedAsync(EventArgs _)
        {
            var navigationParams = new NavigationParameters
            {
                { "selectedCoins", _selectedUnspentCoins }
            };
            await _navigationService.GoBackAsync(navigationParams);
        }

        private async Task CoinSelectionCancelCmdExecutedAsync(EventArgs e)
        {
            _selectedUnspentCoins = null;
            var navigationParams = new NavigationParameters
            {
                { "selectedCoins", _selectedUnspentCoins }
            };
            await _navigationService.GoBackAsync(navigationParams);
        }
    }
}
