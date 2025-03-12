using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BtcWalletLibrary.Events.Arguments;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using NBitcoin;
using Prism.Navigation;
using Xamarin.Forms;
using Transaction = NBitcoin.Transaction;

namespace BtcWalletUI.ViewModels
{
    public class TransferViewModel : BaseViewModel, INotifyDataErrorInfo, INavigatedAware
    {
        private readonly INavigationService _navigationService;
        private readonly ITransferService _transferService;
        private readonly ITxBuilderService _txBuilderService;
        private readonly ITxFeeService _txFeeService;
        private readonly ITxValidator _txValidator;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Dictionary<string, List<string>> _validationErrors = [];
        private Transaction _prebuiltTransaction;

        private string _address;
        private decimal _amount;
        private decimal? _customFee;
        private bool _customFeeSelectionMode;
        private bool _manualCoinSelectionMode;
        private List<UnspentCoin> _selectedUnspentCoins;
        private string _broadcastTxBroadcastTxSuccessMessage;
        private string _broadcastTxBroadcastTxErrorMessage;
        private string _buildTxSuccessMessage;
        private string _buildTxErrorMessage;
        private bool _isLoading;
        private bool isTxsFetching = true;




        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public ICommand ManualCoinSelectionCommand { get; }
        public ICommand ManualFeeSelectionCommand { get; }
        public ICommand SendCommand { get; }
        public bool IsTxsFetching { get => isTxsFetching; set => SetProperty(ref isTxsFetching, value); }
        public string Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value))
                {
                    ValidateAddress();
                    OnSendCommandCanExecuteChanged();
                }
            }
        }
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (SetProperty(ref _amount, value))
                {
                    ValidateAmount();
                    OnSendCommandCanExecuteChanged();
                }
            }
        }
        public decimal? CustomFee
        {
            get => _customFee;
            set
            {
                if (SetProperty(ref _customFee, value))
                {
                    if (_customFee.HasValue) ValidateCustomFee();
                    OnSendCommandCanExecuteChanged();
                }
            }
        }
        public bool CustomFeeSelectionMode
        {
            get => _customFeeSelectionMode;
            set => SetProperty(ref _customFeeSelectionMode, value);
        }
        public bool ManualCoinSelectionMode
        {
            get => _manualCoinSelectionMode;
            set => SetProperty(ref _manualCoinSelectionMode, value);
        }
        public string BroadcastTxSuccessMessage
        {
            get => _broadcastTxBroadcastTxSuccessMessage;
            set => SetProperty(ref _broadcastTxBroadcastTxSuccessMessage, value);
        }
        public string BroadcastTxErrorMessage
        {
            get => _broadcastTxBroadcastTxErrorMessage;
            set => SetProperty(ref _broadcastTxBroadcastTxErrorMessage, value);
        }
        public string BuildTxSuccessMessage
        {
            get => _buildTxSuccessMessage;
            set => SetProperty(ref _buildTxSuccessMessage, value);
        }
        public string BuildTxErrorMessage
        {
            get => _buildTxErrorMessage;
            set => SetProperty(ref _buildTxErrorMessage, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasErrors => _validationErrors.Count > 0;

        public string AddressError => string.Join("\n", _validationErrors.ContainsKey(nameof(Address))
            ? _validationErrors[nameof(Address)]
            : Enumerable.Empty<string>());
        public string AmountError => string.Join("\n", _validationErrors.ContainsKey(nameof(Amount))
            ? _validationErrors[nameof(Amount)]
            : Enumerable.Empty<string>());

        public string CustomFeeError => string.Join("\n", _validationErrors.ContainsKey(nameof(CustomFee))
            ? _validationErrors[nameof(CustomFee)]
            : Enumerable.Empty<string>());


        public TransferViewModel(ITransferService transferService, ITxFeeService txFeeService, INavigationService navigationService, ITxBuilderService txBuilderService, ITxValidator txValidator, IEventDispatcher eventDispatcher)
        {
            _navigationService = navigationService;
            _transferService = transferService;
            _txFeeService = txFeeService;

            _txBuilderService = txBuilderService;
            _txValidator = txValidator;
            _eventDispatcher = eventDispatcher;

            _eventDispatcher.Subscribe<FetchingStartedEventArgs>(OnTxFetchingSarted);
            _eventDispatcher.Subscribe<FetchingCompletedEventArgs>(OnTxFetchingComplete);

            ManualCoinSelectionCommand = new Command(async () => await ManualCoinSelectionCmdExecuted());
            ManualFeeSelectionCommand = new Command(ManualFeeSelectionCmdExecuted);
            SendCommand = new Command(
                async () => await SendButton_Clicked(),
                () => TryToPrebuildTransaction()
            );
        }

        private void OnTxFetchingComplete(object _, FetchingCompletedEventArgs __)
        {
            IsTxsFetching = false;
        }

        private void OnTxFetchingSarted(object _, FetchingStartedEventArgs __)
        {
            IsTxsFetching = true;
        }

        public async Task GetBitFeeAsync()
        {
            await _txFeeService.GetRecommendedBitFeeAsync();
        }

        // Command Handlers
        private async Task ManualCoinSelectionCmdExecuted() => await _navigationService.NavigateAsync("CoinSelectionPopup");

        private void ManualFeeSelectionCmdExecuted(object obj) => CustomFeeSelectionMode = true;

        private async Task SendButton_Clicked()
        {
            try
            {
                IsLoading = true;
                BuildTxSuccessMessage = string.Empty;
                BroadcastTxSuccessMessage = string.Empty;
                BroadcastTxErrorMessage = string.Empty;

                var transactionResult = await _transferService.BroadcastTransactionAsync(_prebuiltTransaction);
                var txId = transactionResult.TransactionId;
                var broadcasted = transactionResult.Success;

                if (broadcasted)
                {
                    BroadcastTxSuccessMessage = $"Transaction Broadcasted Successfully! TX ID: {txId}";
                }
                else
                {
                    BroadcastTxErrorMessage = "Transaction Broadcast Failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                BroadcastTxErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                OnSendCommandCanExecuteChanged();
                IsLoading = false;
            }
        }

        // Validation Methods
        private void ValidateAddress()
        {
            var errors = new List<string>();
            if (!_txValidator.ValidateAddress(_address, out var errorCode))
            {
                errors.Add(errorCode.ToString());
            }

            SetErrors(nameof(Address), errors);
        }

        private void ValidateAmount()
        {
            var errors = new List<string>();
            if (!_txValidator.ValidateAmount(_amount, out var errorCode))
            {
                errors.Add(errorCode.ToString());
            }

            SetErrors(nameof(Amount), errors);
        }

        private void ValidateCustomFee()
        {
            var errors = new List<string>();
            if (!_txValidator.ValidateCustomFee((decimal)_customFee, out var errorCode))
            {
                errors.Add(errorCode.ToString());
            }

            SetErrors(nameof(CustomFee), errors);
        }

        private bool TryToPrebuildTransaction()
        {
            if (HasErrors) return false;
            if (Address == null || Address.Length == 0) return false;
            if (Amount == 0) return false;
            if (CustomFeeSelectionMode && (CustomFee == null || CustomFee == 0)) return false;

            if (_txBuilderService.TryBuildTx(
                new Money(_amount, MoneyUnit.BTC),
                Address,
                out _prebuiltTransaction,
                out var _,
                out var _,
                out var txBuildErrorCode,
                ManualCoinSelectionMode ? _selectedUnspentCoins : null,
                CustomFeeSelectionMode ? new Money(CustomFee.Value, MoneyUnit.BTC) : null
            ))
            {
                BuildTxErrorMessage = string.Empty;
                BuildTxSuccessMessage = "transaction build is successfull";
                return true;
            }
            else
            {
                BuildTxSuccessMessage = string.Empty;
                BuildTxErrorMessage = "There were errors building transaction, error code:" + txBuildErrorCode.ToString();
                return false;
            }
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey("selectedCoins"))
            {
                _selectedUnspentCoins = parameters.GetValue<List<UnspentCoin>>("selectedCoins");
                OnSendCommandCanExecuteChanged();
            }
        }

        // Helper Methods
        private void SetErrors(string propertyName, List<string> errors)
        {
            var hasErrors = errors.Count > 0;
            var propertyInDict = _validationErrors.ContainsKey(propertyName);

            //case when no errors and property is not in the dictionary
            if (!hasErrors && !propertyInDict)
            {
                return;
            }

            //case when no errors and property is in the dictionary
            if (!hasErrors && propertyInDict)
            {
                _validationErrors.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
                return;
            }

            //case when there are errors and property is not in the dictionary
            if (hasErrors && !propertyInDict)
            {
                _validationErrors.Add(propertyName, errors);
                RaiseErrorsChanged(propertyName);
                return;
            }

            //case when there are errors and property is in the dictionary
            if (hasErrors && propertyInDict)
            {
                //case if its same errors as it was before
                if (_validationErrors[propertyName].SequenceEqual(errors))
                {
                    return;
                }
                _validationErrors[propertyName] = errors;
                RaiseErrorsChanged(propertyName);
            }
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs($"{propertyName}Error"));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasErrors)));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return !_validationErrors.TryGetValue(propertyName, out var errors) ? [] : errors;
        }

        private void OnSendCommandCanExecuteChanged() =>
            ((Command)SendCommand).ChangeCanExecute();

        protected virtual void OnErrorsChanged(string propertyName) =>
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }
    }
}