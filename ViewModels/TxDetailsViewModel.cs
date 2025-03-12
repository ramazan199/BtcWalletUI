using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using BtcWalletLibrary.Interfaces;
using NBitcoin;
using Prism.Navigation;
using Xamarin.Forms;
using Transaction = BtcWalletUI.Models.Transaction;
using Prism.Mvvm;
using System;
using BtcWalletUI.Models;
using System.Threading.Tasks;
using System.Linq;

namespace BtcWalletUI.ViewModels
{
    public class TxDetailsViewModel : BaseViewModel, INavigationAware
    {
        private Transaction _transaction;
        private readonly ITxHistoryService _btcTxHistoryService;
        private readonly ICommonService _commonService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<VinOut> Vins { get; } = [];
        public ObservableCollection<VinOut> Vouts { get; } = [];
        public string TransactionId { get; private set; }
        public string ConfirmedBalance { get; private set; }
        public ICommand NavigateBackCommand { get; }

        public TxDetailsViewModel(
            ITxHistoryService btcTxHistoryService,
            ICommonService commonService,
            INavigationService navigationService)
        {
            _btcTxHistoryService = btcTxHistoryService;
            _commonService = commonService;
            _navigationService = navigationService;

            NavigateBackCommand = new Command(async () => await NavigateBackCmdExecuted());
        }

        private async Task NavigateBackCmdExecuted()
        {
            await _navigationService.GoBackAsync();
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue<Transaction>("transaction", out var transaction))
            {
                _transaction = transaction;
                TransactionId = _transaction.TransactionId;
                GetTransactionDetails();
            }
        }

        public void GetTransactionDetails()
        {

            Vins.Clear();
            Vouts.Clear();

            var transactionForStorage = _btcTxHistoryService.Transactions
                .FirstOrDefault(x => x.TransactionId == _transaction.TransactionId);


            var parsedTransaction = NBitcoin.Transaction.Parse(
                transactionForStorage.TransactionHex,
                _commonService.BitcoinNetwork
            );

            foreach (var input in transactionForStorage.Inputs)
            {
                Vins.Add(new VinOut
                {
                    Address = input.Address,
                    Amount = input.Amount.ToString(CultureInfo.InvariantCulture),
                    IsVin = true
                });
            }

            foreach (var output in parsedTransaction.Outputs)
            {
                Vouts.Add(new VinOut
                {
                    Address = output.ScriptPubKey.GetDestinationAddress(_commonService.BitcoinNetwork)?.ToString() ?? "Unknown Address",
                    Amount = output.Value.ToDecimal(MoneyUnit.BTC).ToString("0.#######################"),
                    IsVin = false
                });
            }


        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            Vins.Clear();
            Vouts.Clear();
        }
    }
}