using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using BtcWalletLibrary.Services;
using BtcWalletUI.Models;
using BtcWalletUI.Resources;
using NBitcoin;
using Xamarin.Forms;
using ICoinMapper = BtcWalletUI.Utils.Mappers.ICoinMapper;
using Transaction = NBitcoin.Transaction;

namespace BtcWalletUI.ViewModels
{
    public class UnspentCoinViewModel : BaseViewModel
    {
        public ObservableCollection<UnspentCoin> UnspentCoins { get; private set; }
        public ObservableCollection<UnspentCoin> SelectedUnspentCoins { get; set; }


        public bool IsDataLoading { get; set; }
        public bool IfListEmpty { get; set; }

        private readonly ICommonService _btcCommonService;
        private readonly ITransferService _btcTransferService;
        private readonly IBalanceService _balanceService;
        private readonly ICoinMapper _coinMapper;
        public ICommand ConfirmCommand;
        public ICommand BackComannand;


        public UnspentCoinViewModel(ICommonService btcCommonService, ITransferService btcTransferService, IBalanceService btcBalanceService, ICoinMapper coinMapper)
        {
            Title = Dictionary.UnspentTransactions;
            ConfirmCommand = new Command(OnConfirmClicked);
            _btcCommonService = btcCommonService;
            _btcTransferService = btcTransferService;
            _balanceService = btcBalanceService;
            _coinMapper = coinMapper;

            InitUnspentCoins();

        }

        private void OnConfirmClicked()
        {
            MessagingCenter.Send(this, "CoinsSelceted", SelectedUnspentCoins);
        }

        private void InitUnspentCoins()
        {
            UnspentCoins = new ObservableCollection<UnspentCoin>(_balanceService.Utxos.Select(u => _coinMapper.UtxoToUnspentCoin(u)));
        }
    }
}
