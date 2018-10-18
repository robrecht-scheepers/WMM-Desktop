using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.MVVM;

namespace WMM.WPF
{
    public class AddTransactionsViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private DateTime _newTransactionDate;
        private string _newTransactionCategory;
        private double _newTransactionAmount;
        private AsyncRelayCommand _addTransactionCommand;
        private ObservableCollection<string> _categories;
        private string _selectedSign;

        public AddTransactionsViewModel(IRepository repository)
        {
            _repository = repository;
            Categories = new ObservableCollection<string>();
            AddedTransactions = new ObservableCollection<Transaction>();
        }

        public async Task Initialize()
        {
            Categories = new ObservableCollection<string>(await _repository.GetCategories());
            NewTransactionCategory = Categories.FirstOrDefault();
            NewTransactionDate = DateTime.Today;
            NewTransactionAmount = 115.29; // should be 0, now with value for faster testing
            SelectedSign = "-";
        }

        public DateTime NewTransactionDate
        {
            get => _newTransactionDate;
            set => SetValue(ref _newTransactionDate, value);
        }

        public string NewTransactionCategory
        {
            get => _newTransactionCategory;
            set => SetValue(ref _newTransactionCategory, value);
        }

        public double NewTransactionAmount
        {
            get => _newTransactionAmount;
            set => SetValue(ref _newTransactionAmount, value);
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            private set => SetValue(ref _categories, value);
        }

        public List<string> Signs => new List<string> {"+", "-"};

        public string SelectedSign
        {
            get => _selectedSign;
            set => SetValue(ref _selectedSign, value);
        }

        public AsyncRelayCommand AddTransactionCommand => _addTransactionCommand ?? (_addTransactionCommand = new AsyncRelayCommand(AddTransaction));
        private async Task AddTransaction()
        {
            var amount = SelectedSign == "-" ? NewTransactionAmount * -1.0 : NewTransactionAmount;

            var transaction = await _repository.AddTransaction(NewTransactionDate, NewTransactionCategory, amount, null);

            AddedTransactions.Add(transaction);
            RaiseTransactionAdded(transaction);
        }

        public ObservableCollection<Transaction> AddedTransactions { get; }

        public event TransactionEventHandler TransactionAdded;

        private void RaiseTransactionAdded(Transaction transaction)
        {
            TransactionAdded?.Invoke(this, new TransactionEventArgs(transaction));
        }
    }
}
