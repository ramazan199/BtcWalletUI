using System.Collections.ObjectModel;
using System.Linq;
using TransactionInput = BtcWalletUI.Models.TransactionInput;
using TransactionOutput = BtcWalletUI.Models.TransactionOutput;

namespace BtcWalletUI.Utils.Mappers
{
    public interface ITransactionMapper
    {
        Models.Transaction TxForStorageToTx(BtcWalletLibrary.Models.Transaction transactionForStorage);
    }

    public class TransactionMapper : ITransactionMapper
    {
        public Models.Transaction TxForStorageToTx(BtcWalletLibrary.Models.Transaction transactionForStorage)
        {
            var transactionOutputs = new ObservableCollection<TransactionOutput>(transactionForStorage.Outputs.Select(output => new TransactionOutput { Address = output.Address, IsUsersAddress = output.IsUsersAddress, Amount = output.Amount }).ToList());
            var transactionInputs = new ObservableCollection<TransactionInput>(transactionForStorage.Inputs.Select(input => new TransactionInput { Address = input.Address, IsUsersAddress = input.IsUsersAddress, Amount = input.Amount }).ToList());
            return new Models.Transaction(
                transactionForStorage.TransactionId,
                transactionForStorage.Date,
                transactionOutputs,
                transactionInputs,
                transactionForStorage.Confirmed);
        }
    }
}
