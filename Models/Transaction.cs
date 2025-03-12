using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace BtcWalletUI.Models
{
    public sealed class Transaction : INotifyPropertyChanged
    {
        private DateTime _date;
        private bool _confirmed;

        // Constructor
        public Transaction(
            string transactionId,
            DateTime date,
            ObservableCollection<TransactionOutput> outputs,
            ObservableCollection<TransactionInput> inputs,
            bool confirmed = false)
        {
            TransactionId = transactionId ?? throw new ArgumentNullException(nameof(transactionId));
            _date = date;
            _confirmed = confirmed;
            Inputs = [];
            Inputs.CollectionChanged += Inputs_CollectionChanged;
            foreach (var input in inputs)
            {
                Inputs.Add(input);
                input.PropertyChanged += InputOutput_PropertyChanged;
            }

            Outputs = [];
            Outputs.CollectionChanged += Outputs_CollectionChanged;
            foreach (var output in outputs)
            {
                Outputs.Add(output);
                output.PropertyChanged += InputOutput_PropertyChanged;
            }
        }

        public string TransactionId { get; }

        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date == value) return;
                _date = value;
                OnPropertyChanged(nameof(Date)); // Notify UI of change
            }
        }

        public ObservableCollection<TransactionOutput> Outputs { get; private set; }

        public ObservableCollection<TransactionInput> Inputs { get; private set; }

        public bool Confirmed
        {
            get => _confirmed;
            set
            {
                if (_confirmed == value) return;
                _confirmed = value;
                OnPropertyChanged(nameof(Confirmed)); // Notify UI of change
            }
        }

        public decimal Amount => GetUserOutputsSum() - GetUserInputsSum(); // Sent or received amount

        public bool IsOutgoing => Amount < 0; // If Amount is negative, it means that user inputs are greater than user outputs

        public void Confirm()
        {
            Confirmed = true;
        }

 

        private decimal GetUserInputsSum()
        {
            return (decimal)Inputs.Where(input => input.IsUsersAddress).Sum(input => input.Amount);
        }

        private decimal GetUserOutputsSum()
        {
            return (decimal)Outputs.Where(output => output.IsUsersAddress).Sum(output => output.Amount);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        //input output changes
        private void Inputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (TransactionInput newItem in e.NewItems)
                {
                    newItem.PropertyChanged += InputOutput_PropertyChanged;
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (TransactionInput oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= InputOutput_PropertyChanged;
                }
            }
            OnPropertyChanged(nameof(Inputs));
            OnPropertyChanged(nameof(Amount));
            OnPropertyChanged(nameof(IsOutgoing));
        }
        private void Outputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (TransactionOutput newItem in e.NewItems)
                {
                    newItem.PropertyChanged += InputOutput_PropertyChanged;
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (TransactionOutput oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= InputOutput_PropertyChanged;
                }
            }
            OnPropertyChanged(nameof(Outputs));
            OnPropertyChanged(nameof(Amount));
            OnPropertyChanged(nameof(IsOutgoing));
        }

        private void InputOutput_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TransactionInput.IsUsersAddress) &&
                e.PropertyName != nameof(TransactionOutput.IsUsersAddress) &&
                e.PropertyName != nameof(TransactionInput.Amount) &&
                e.PropertyName != nameof(TransactionOutput.Amount)) return;
            OnPropertyChanged(nameof(Amount));  // Crucial: Notify Amount has changed
            OnPropertyChanged(nameof(IsOutgoing)); //Crucial: Notify IsOutgoing has changed
        }
    }
}
