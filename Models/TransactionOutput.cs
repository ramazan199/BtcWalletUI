using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BtcWalletUI.Models
{
    public sealed class TransactionOutput :  INotifyPropertyChanged
    {
        private string _address;
        private bool _isUsersAddress;
        private double _amount;

        public string Address
        {
            get => _address;
            set
            {
                if (_address == value) return;
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public bool IsUsersAddress
        {
            get => _isUsersAddress;
            set
            {
                if (_isUsersAddress == value) return;
                _isUsersAddress = value;
                OnPropertyChanged(nameof(IsUsersAddress));
            }
        }

        public double Amount
        {
            get => _amount;
            set
            {
                if (_amount == value) return;
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }


        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
