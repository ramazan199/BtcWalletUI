using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using NBitcoin;


namespace BtcWalletUI.Utils.Mappers
{
    public interface ICoinMapper
    {
        UnspentCoin UtxoToUnspentCoin(UtxoDetailsElectrumx utxo);
    }

    public class CoinMapper(ITxMapper transactionMapper) : ICoinMapper
    {
        public UnspentCoin UtxoToUnspentCoin(UtxoDetailsElectrumx utxo)
        {
            var coin = transactionMapper.UtxoToCoin(utxo);
            var unspentCoin = new UnspentCoin
            {
                Amount = coin.Amount.ToDecimal(MoneyUnit.BTC),
                Confirmed = coin.Confirmed,
                Address = utxo.Address,
                TransactionId = coin.Outpoint.Hash.ToString()
            };
            return unspentCoin;
        }
    }
}
