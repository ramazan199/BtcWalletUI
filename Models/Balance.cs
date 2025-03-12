using System.ComponentModel;

namespace BtcWalletUI.Models
{
    public class Balance : INotifyPropertyChanged
    {
        private double _confirmed;
        private double _unconfirmed;

        
        public double Confirmed
        {
            get => _confirmed;
            set
            {
                if (_confirmed == value) return;
                _confirmed = value;
                OnPropertyChanged(nameof(Confirmed));
                OnPropertyChanged(nameof(Total)); // Notify that Total has changed
            }
        }

        public double Unconfirmed
        {
            get => _unconfirmed;
            set
            {
                if (_unconfirmed == value) return;
                _unconfirmed = value;
                OnPropertyChanged(nameof(Unconfirmed));
                OnPropertyChanged(nameof(Total)); // Notify that Total has changed
            }
        }

        public double Total
        {
            get => Confirmed + Unconfirmed;
        }

        // Implementing the PropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;

        // Notify listeners when a property changes
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
