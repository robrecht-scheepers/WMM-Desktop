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
        private readonly bool _manageTemplates;
        private readonly DateTime _month;
        private string _newCategory;
        private double _newAmount;
        private AsyncRelayCommand _addCommand;
        private ObservableCollection<string> _categories;
        private string _selectedSign;
        private AsyncRelayCommand _applyTemplatesCommand;

        public RecurringTransactionsViewModel(IRepository repository, DateTime month)
        {
            _repository = repository;
            _manageTemplates = false;
            _month = month;
            
            Categories = new ObservableCollection<string>();
            Transactions = new ObservableCollection<Transaction>();
        }

        public RecurringTransactionsViewModel(IRepository repository)
        {
            _repository = repository;
            _manageTemplates = true;

            Categories = new ObservableCollection<string>();
            Transactions = new ObservableCollection<Transaction>();
        }

        public async Task Initialize()
        {
            Categories = new ObservableCollection<string>(await _repository.GetCategories());
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
