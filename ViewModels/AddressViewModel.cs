using System.Threading.Tasks;
using System.Windows.Input;
using BtcWalletLibrary.Interfaces;
using Prism.Commands;
using Xamarin.Essentials;
using Xamarin.Forms;
namespace BtcWalletUI.ViewModels
{
    public class AddressViewModel : BaseViewModel
    {
        private readonly IAddressService _addressService;
        private uint _newAddrIdxIncrement;
        private string _address;

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public ICommand CopyCommand { get; }
        public ICommand GenerateNewAddressCommand { get; }

        public AddressViewModel(IAddressService addressService)
        {
            _addressService = addressService;
            Address = addressService.DeriveNewMainAddr().ToString();
            _newAddrIdxIncrement++;

            CopyCommand = new DelegateCommand(async () => await CopyCommandExecuted());
            GenerateNewAddressCommand = new DelegateCommand(GenerateNewAddressCommandExecuted);
        }

        private async Task CopyCommandExecuted()
        {
            await Clipboard.SetTextAsync(Address);
            await Application.Current.MainPage.DisplayAlert("Success", "Copied to clipboard", "OK");
        }

        private void GenerateNewAddressCommandExecuted()
        {
            _newAddrIdxIncrement++;
            var newAddress = _addressService.DeriveMainAddr(
                (uint)(_addressService.LastMainAddrIdx + _newAddrIdxIncrement)
            ).ToString();

            Address = newAddress;  
        }
    }
}