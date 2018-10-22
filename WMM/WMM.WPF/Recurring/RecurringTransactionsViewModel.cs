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
    public class RecurringTransactionsViewModel : TransactionListViewModelBase
    {
        private readonly bool _manageTemplates;
        private readonly DateTime _month;
        private string _newCategory;
        private double _newAmount;
        private AsyncRelayCommand _addCommand;
        private ObservableCollection<string> _categories;
        private string _selectedSign;
        private AsyncRelayCommand _applyTemplatesCommand;
        
        public RecurringTransactionsViewModel(IRepository repository, IWindowService windowService, DateTime month) 
            : base(repository, windowService, false)
        {
            _manageTemplates = false;
            _month = month;
            Categories = new ObservableCollection<string>();
        }
        public RecurringTransactionsViewModel(IRepository repository, IWindowService windowService)
            : base(repository, windowService, false)
        {
            _manageTemplates = true;
            Categories = new ObservableCollection<string>();
        }

        public async Task Initialize()
        {
            Categories = new ObservableCollection<string>(Repository.GetCategories());
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
            foreach (var template in await Repository.GetRecurringTemplates())
            {
                Transactions.Add(template);
            }
        }

        private async Task GetRecurringTransactions()
        {
            Transactions.Clear();
            foreach (var template in await Repository.GetRecurringTransactions(_month.FirstDayOfMonth(), _month.LastDayOfMonth()))
            {
                Transactions.Add(template);
            }
        }

        public AsyncRelayCommand AddCommand => _addCommand ?? (_addCommand = new AsyncRelayCommand(Add));
        private async Task Add()
        {
            var amount = SelectedSign == "-" ? NewAmount * -1.0 : NewAmount;

            var transaction = _manageTemplates
                ? await Repository.AddRecurringTemplate(NewCategory, amount, "")
                : await Repository.AddTransaction(_month.FirstDayOfMonth(), NewCategory, amount, "", true);
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
            await Repository.ApplyRecurringTemplates(_month);
            await GetRecurringTransactions();
            RaiseMultipleTransactionsModified();
        }
    }
}
