using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF.Recurring
{
    public class RecurringTransactionsViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private readonly bool _manageTemplates;
        private readonly DateTime _month;
        private string _newCategory;
        private double _newAmount;
        private AsyncRelayCommand _addCommand;
        private ObservableCollection<string> _categories;
        private string _selectedSign;
        private AsyncRelayCommand _applyTemplatesCommand;
        private AsyncRelayCommand<Transaction> _deleteTransactionCommand;
        private RelayCommand<Transaction> _editTransactionCommand;

        public RecurringTransactionsViewModel(IRepository repository, IWindowService windowService, DateTime month)
        {
            _repository = repository;
            _windowService = windowService;
            _manageTemplates = false;
            _month = month;
            
            Categories = new ObservableCollection<string>();
            Transactions = new ObservableCollection<Transaction>();
        }
        public RecurringTransactionsViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            _manageTemplates = true;

            Categories = new ObservableCollection<string>();
            Transactions = new ObservableCollection<Transaction>();
        }

        public async Task Initialize()
        {
            Categories = new ObservableCollection<string>(_repository.GetCategories());
            NewCategory = Categories.FirstOrDefault();
            NewAmount = 0.0;
            SelectedSign = "-";

            if (_manageTemplates)
            {
                await GetRecurringTemplates();
            }
            else
            {
                await GetRecurringTransactions();
            }
        }

        public string Title => _manageTemplates
            ? "Vorlagen für monatliche Kosten und Einkunften verwalten"
            : $"Monatliche Kosten und Einkunften für {_month:Y} verwalten";

        public ObservableCollection<Transaction> Transactions { get; }

        public string NewCategory
        {
            get => _newCategory;
            set => SetValue(ref _newCategory, value);
        }

        public double NewAmount
        {
            get => _newAmount;
            set => SetValue(ref _newAmount, value);
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            private set => SetValue(ref _categories, value);
        }

        public List<string> Signs => new List<string> { "+", "-" };

        public string SelectedSign
        {
            get => _selectedSign;
            set => SetValue(ref _selectedSign, value);
        }

        private async Task GetRecurringTemplates()
        {
            Transactions.Clear();
            foreach (var template in await _repository.GetRecurringTemplates())
            {
                Transactions.Add(template);
            }
        }

        private async Task GetRecurringTransactions()
        {
            Transactions.Clear();
            foreach (var template in await _repository.GetRecurringTransactions(_month.FirstDayOfMonth(), _month.LastDayOfMonth()))
            {
                Transactions.Add(template);
            }
        }

        public AsyncRelayCommand AddCommand => _addCommand ?? (_addCommand = new AsyncRelayCommand(Add));
        private async Task Add()
        {
            var amount = SelectedSign == "-" ? NewAmount * -1.0 : NewAmount;

            var transaction = _manageTemplates
                ? await _repository.AddRecurringTemplate(NewCategory, amount, "")
                : await _repository.AddTransaction(_month.FirstDayOfMonth(), NewCategory, amount, "", true);
            Transactions.Add(transaction);
            RaiseTransactionModified(transaction);
        }

        public AsyncRelayCommand ApplyTemplatesCommand =>
            _applyTemplatesCommand ?? (_applyTemplatesCommand = new AsyncRelayCommand(ApplyTemplates, CanExecuteApplyTemplates));

        private bool CanExecuteApplyTemplates()
        {
            return !_manageTemplates && !Transactions.Any();
        }
        private async Task ApplyTemplates()
        {
            await _repository.ApplyRecurringTemplates(_month);
            await GetRecurringTransactions();
            RaiseMultipleTransactionsModified();
        }

        public AsyncRelayCommand<Transaction> DeleteTransactionCommand => _deleteTransactionCommand ?? (_deleteTransactionCommand = new AsyncRelayCommand<Transaction>(DeleteTransaction));
        private async Task DeleteTransaction(Transaction transaction)
        {
            await _repository.DeleteTransaction(transaction);
            Transactions.Remove(transaction);
            RaiseTransactionModified(transaction);
        }

        public RelayCommand<Transaction> EditTransactionCommand =>
            _editTransactionCommand ?? (_editTransactionCommand = new RelayCommand<Transaction>(EditTransaction));

        private void EditTransaction(Transaction transaction)
        {
            var editTransactionViewModel = new EditTransactionViewModel(transaction, _repository, false);
            editTransactionViewModel.TransactionChanged += (sender, args) =>
            {
                var index = Transactions.IndexOf(args.OldTransaction);
                Transactions.Remove(args.OldTransaction);
                Transactions.Insert(index, args.NewTransaction);
                RaiseTransactionModified(args.OldTransaction);
                RaiseTransactionModified(args.NewTransaction);
            };
            _windowService.OpenDialogWindow(editTransactionViewModel);
        }


        public event TransactionEventHandler TransactionModified;
        private void RaiseTransactionModified(Transaction transaction)
        {
            TransactionModified?.Invoke(this, new TransactionEventArgs(transaction));
        }

        public event TransactionEventHandler MultipleTransactionsModified;
        private void RaiseMultipleTransactionsModified()
        {
            MultipleTransactionsModified?.Invoke(this, new TransactionEventArgs(null));
        }
    }
}
