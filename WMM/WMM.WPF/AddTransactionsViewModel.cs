using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF
{
    public class AddTransactionsViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private DateTime _newTransactionDate;
        private string _newTransactionCategory;
        private double _newTransactionAmount;
        private AsyncRelayCommand _addTransactionCommand;
        private ObservableCollection<string> _categories;
        private string _selectedSign;
        private AsyncRelayCommand<Transaction> _deleteTransactionCommand;
        private RelayCommand<Transaction> _editTransactionCommand;

        public AddTransactionsViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            Categories = new ObservableCollection<string>();
            AddedTransactions = new ObservableCollection<Transaction>();
        }

        public async Task Initialize()
        {
            Categories = new ObservableCollection<string>( _repository.GetCategories().OrderBy(x => x));
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

            AddedTransactions.Insert(0,transaction);
            RaiseTransactionChanged(transaction);
        }

        public AsyncRelayCommand<Transaction> DeleteTransactionCommand => _deleteTransactionCommand ?? (_deleteTransactionCommand = new AsyncRelayCommand<Transaction>(DeleteTransaction));
        private async Task DeleteTransaction(Transaction transaction)
        {
            await _repository.DeleteTransaction(transaction);
            AddedTransactions.Remove(transaction);
            RaiseTransactionChanged(transaction);
        }

        public RelayCommand<Transaction> EditTransactionCommand =>
            _editTransactionCommand ?? (_editTransactionCommand = new RelayCommand<Transaction>(EditTransaction));

        private void EditTransaction(Transaction transaction)
        {
            var editTransactionViewModel = new EditTransactionViewModel(transaction, _repository);
            editTransactionViewModel.TransactionChanged += (sender, args) =>
            {
                var index = AddedTransactions.IndexOf(args.OldTransaction);
                AddedTransactions.Remove(args.OldTransaction);
                AddedTransactions.Insert(index, args.NewTransaction);
                RaiseTransactionChanged(args.OldTransaction);
                RaiseTransactionChanged(args.NewTransaction);
            };
            _windowService.OpenDialogWindow(editTransactionViewModel);
        }


        public ObservableCollection<Transaction> AddedTransactions { get; }

        public event TransactionEventHandler TransactionChanged;

        private void RaiseTransactionChanged(Transaction transaction)
        {
            TransactionChanged?.Invoke(this, new TransactionEventArgs(transaction));
        }
    }
}
