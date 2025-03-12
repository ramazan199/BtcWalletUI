using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BtcWalletLibrary.Events.Arguments;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using BtcWalletLibrary.Services;
using BtcWalletUI.Models;
using BtcWalletUI.Utils;
using BtcWalletUI.Utils.Mappers;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms;
using Balance = BtcWalletUI.Models.Balance;
using Transaction = BtcWalletUI.Models.Transaction;

namespace BtcWalletUI.ViewModels
{
    public class TxHistoryViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly ITransactionMapper _transactionMapper;
        private readonly IBalanceService _balanceService;
        private readonly ITxHistoryService _txHistoryService;

        // Direct backing fields for properties
        private RangeObservableCollection<Transaction> _transactions = [];
        private Balance _balance;
        private string _balanceUnconfirmed;
        private bool _isDataLoading;
        private bool _ifListEmpty;
        private bool _isWalletRecovering;
        private bool _isWeAreQueryingBChain;

        public RangeObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        public Balance Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public string BalanceUnconfirmed
        {
            get => _balanceUnconfirmed;
            set => SetProperty(ref _balanceUnconfirmed, value);
        }

        public bool IsDataLoading
        {
            get => _isDataLoading;
            set => SetProperty(ref _isDataLoading, value);
        }

        public bool IfListEmpty
        {
            get => _ifListEmpty;
            set => SetProperty(ref _ifListEmpty, value);
        }

        public bool IsWalletRecovering
        {
            get => _isWalletRecovering;
            set => SetProperty(ref _isWalletRecovering, value);
        }

        public bool IsQueryingBlockChain
        {
            get => _isWeAreQueryingBChain;
            set => SetProperty(ref _isWeAreQueryingBChain, value);
        }

        // Commands
        public ICommand RefreshCommand { get; private set; }
        public ICommand NavigateToTransferPageCommand { get; private set; }
        public ICommand NavigateShareAddressPageCommand { get; private set; }
        public IAsyncCommand TxListViewItemTapCommand { get; private set; }

        public TxHistoryViewModel(
            IBalanceService balanceService,
            ITxHistoryService txHistoryService,
            IEventDispatcher eventDispatcher,
            INavigationService navigationService,
            ITransactionMapper transactionMapper)
        {
            _balanceService = balanceService;
            _txHistoryService = txHistoryService;
            _navigationService = navigationService;
            _transactionMapper = transactionMapper;
            InitializeCommands();
            SubscribeToEvents(eventDispatcher);
            InitializeData();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new Command(async () => await RefreshCommandExecuted());
            NavigateToTransferPageCommand = new Command(async () => await NavigateToTransferPageCmdExecuted());
            NavigateShareAddressPageCommand = new Command(async () => await NavigateShareAddressPageCmdExecuted());
            TxListViewItemTapCommand = new AsyncDelegateCommand<ListView>(TxListViewItemTapCmdExecuted);
        }

        private void SubscribeToEvents(IEventDispatcher eventDispatcher)
        {
            eventDispatcher.Subscribe<TransactionAddedEventArgs>(OnTransactionAdded);
            eventDispatcher.Subscribe<TxInputAddrMarkedAsUserAddrEventArgs>(OnTxInputAddrMarkedAsUserAddr);
            eventDispatcher.Subscribe<TxOutputAddrMarkedAsUserAddrEventArgs>(OnTxOutputAddrMarkedAsUserAddr);
            eventDispatcher.Subscribe<TransactionDateUpdatedEventArgs>(OnTransactionDateUpdated);
            eventDispatcher.Subscribe<TransactionConfirmedEventArgs>(OnTransactionConfirmed);
            eventDispatcher.Subscribe<TransactionBroadcastedEventArgs>(OnTransactionBroadcasted);
        }

        

        private void InitializeData()
        {
            Balance = new Balance { Confirmed = 0, Unconfirmed = 0 };
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            AssignTxFromStorageToTransactions();
            UpdateBalance();
        }

        private void AssignTxFromStorageToTransactions()
        {
            var mappedTxs = MapTxsFromStorageToTransaction();
            SortTransactions(mappedTxs);
            Transactions.AddRange(mappedTxs);
        }

        private List<Transaction> MapTxsFromStorageToTransaction()
        {
            return [.. _txHistoryService.Transactions.Select(_transactionMapper.TxForStorageToTx)];
        }

        private void SortTransactions(List<Transaction> txs)
        {
            txs.Sort((a, b) => a.Date.CompareTo(b.Date));
        }

        public async Task FetchTransactionsAsync()
        {
            IsQueryingBlockChain = true;
            IsDataLoading = true;

            try
            {
                var result = await Task.Run(async () => await _txHistoryService.SyncTransactionsAsync());
                if (result.HasNetworkErrors)
                {

                    await Application.Current.MainPage.DisplayAlert("Network Error", "Network issues occured while fetching transactions. Some tranactions may be missing from the list. Please press refresh, later.", "OK");

                }
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert("Unexpected Error", "An unexpected error occurred. Please press refresh.", "OK");
            }
            finally
            {
                IsQueryingBlockChain = false;
                IsDataLoading = false;
            }
        }

        public void UpdateBalance()
        {
            Balance ??= new Balance();
            Balance.Confirmed = _balanceService.TotalConfirmedBalance;
            Balance.Unconfirmed = _balanceService.TotalUncnfirmedBalance;
        }

        private void  OnTransactionBroadcasted(object _, TransactionBroadcastedEventArgs e)
        {

            if (e?.TransactionForStorage == null) return;
            var transaction = _transactionMapper.TxForStorageToTx(e.TransactionForStorage);
            Device.BeginInvokeOnMainThread(() =>
            {
                Transactions.Insert(0, transaction);
                UpdateBalance();

            });
        }
        private void OnTransactionAdded(object sender, TransactionAddedEventArgs e)
        {
            if (e?.BitcoinTransaction == null) return;
            var transaction = _transactionMapper.TxForStorageToTx(e.BitcoinTransaction);
            Device.BeginInvokeOnMainThread(() =>
            {
                InsertByDateOrder(transaction);
            });
        }

        private void InsertByDateOrder(Transaction transaction)
        {
            for (int i = 0; i < Transactions.Count; i++)
            {
                if (transaction.Date > Transactions[i].Date)
                {
                    Transactions.Insert(i, transaction);
                    return;
                }

            }
            Transactions.Add(transaction);
        }

        private void OnTransactionConfirmed(object sender, TransactionConfirmedEventArgs e)
        {
            var transaction = GetTransactionByTxId(e.TxId);
            if (transaction == null) return;

            transaction.Confirm();
        }

        private void OnTransactionDateUpdated(object sender, TransactionDateUpdatedEventArgs e)
        {
            var transaction = GetTransactionByTxId(e.TxId);
            if (transaction == null) return;

            transaction.Date = e.Date;
            Transactions.Remove(transaction);
            InsertByDateOrder(transaction);
        }


        private void OnTxOutputAddrMarkedAsUserAddr(object sender, TxOutputAddrMarkedAsUserAddrEventArgs e)
        {
            var transaction = GetTransactionByTxId(e.TxId);
            if (transaction == null) return;

            var output = transaction.Outputs.FirstOrDefault(output => output.Address == e.Output.Address);
            if (output == null) return;

            output.IsUsersAddress = true;
        }



        private void OnTxInputAddrMarkedAsUserAddr(object sender, TxInputAddrMarkedAsUserAddrEventArgs e)
        {
            var transaction = GetTransactionByTxId(e.TxId);
            if (transaction == null) return;

            var input = transaction.Inputs.FirstOrDefault(input => input.Address == e.Input.Address);
            if (input == null) return;

            input.IsUsersAddress = true;
        }

        private Transaction GetTransactionByTxId(String txId)
        {
            return Transactions.FirstOrDefault(tx => tx.TransactionId == txId);
        }


        private async Task NavigateToTransferPageCmdExecuted()
        {
            await _navigationService.NavigateAsync("TransferPage");
        }

        private async Task NavigateShareAddressPageCmdExecuted()
        {
            await _navigationService.NavigateAsync("AddressSharingPage");
        }

        private async Task TxListViewItemTapCmdExecuted(ListView listView)
        {
            if (SelectedTransaction == null) return;

            var parameters = new NavigationParameters
            {
                { "transaction", SelectedTransaction }
            };

            await _navigationService.NavigateAsync("TxDetailsPopup", parameters);

            listView.SelectedItem = null;
        }

        private async Task RefreshCommandExecuted()
        {
            await FetchTransactionsAsync();
            UpdateBalance();
        }
    }
}


