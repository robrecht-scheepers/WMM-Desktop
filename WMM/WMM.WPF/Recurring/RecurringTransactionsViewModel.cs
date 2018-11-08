using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Transactions;

namespace WMM.WPF.Recurring
{
    public class RecurringTransactionsViewModel : TransactionListViewModelBase
    {
        private readonly DateTime _month;
        private string _newCategory;
        private double _newAmount;
        private AsyncRelayCommand _addCommand;
        private ObservableCollection<string> _categories;
        private string _selectedSign;
        private AsyncRelayCommand _applyTemplatesCommand;
        private Balance _totalRecurringBalance;
        private string _newComments;

        public RecurringTransactionsViewModel(IRepository repository, IWindowService windowService, DateTime month) 
            : base(repository, windowService, false)
        {
            ManageTemplates = false;
            _month = month;
            Categories = new ObservableCollection<string>();
        }
        public RecurringTransactionsViewModel(IRepository repository, IWindowService windowService)
            : base(repository, windowService, false)
        {
            ManageTemplates = true;
            Categories = new ObservableCollection<string>();
        }

        public async Task Initialize()
        {
            Categories = new ObservableCollection<string>(Repository.GetCategories().OrderBy(x => x));
            NewCategory = Categories.FirstOrDefault();
            NewAmount = 0.0;
            SelectedSign = "-";

            if (ManageTemplates)
            {
                await GetRecurringTemplates();
            }
            else
            {
                await GetRecurringTransactions();
            }
        }

        public string Title => ManageTemplates
            ? "Vorlagen für monatliche Kosten und Einkunften verwalten"
            : $"Monatliche Kosten und Einkunften für {_month:Y} verwalten";

        public bool ManageTemplates { get; }


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

        public string NewComments
        {
            get => _newComments;
            set => SetValue(ref _newComments, value);
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

        public Balance TotalRecurringBalance
        {
            get => _totalRecurringBalance;
            set => SetValue(ref _totalRecurringBalance, value);
        }

        private async Task GetRecurringTemplates()
        {
            Transactions.Clear();
            foreach (var template in await Repository.GetRecurringTemplates())
            {
                Transactions.Add(template);
            }
            TotalRecurringBalance = await Repository.GetRecurringTemplatesBalance();
        }

        private async Task GetRecurringTransactions()
        {
            Transactions.Clear();
            foreach (var template in await Repository.GetRecurringTransactions(_month.FirstDayOfMonth(), _month.LastDayOfMonth()))
            {
                Transactions.Add(template);
            }
            TotalRecurringBalance = await Repository.GetRecurringTransactionsBalance(_month.FirstDayOfMonth(), _month.LastDayOfMonth());
        }

        public AsyncRelayCommand AddCommand => _addCommand ?? (_addCommand = new AsyncRelayCommand(Add));
        private async Task Add()
        {
            var amount = SelectedSign == "-" ? NewAmount * -1.0 : NewAmount;

            var transaction = ManageTemplates
                ? await Repository.AddRecurringTemplate(NewCategory, amount, NewComments)
                : await Repository.AddTransaction(_month.FirstDayOfMonth(), NewCategory, amount, NewComments, true);
            Transactions.Add(transaction);
            RaiseTransactionModified(transaction);

            NewAmount = 0.0;
            NewComments = "";
        }
        
        public AsyncRelayCommand ApplyTemplatesCommand =>
            _applyTemplatesCommand ?? (_applyTemplatesCommand = new AsyncRelayCommand(ApplyTemplates, CanExecuteApplyTemplates));

        private bool CanExecuteApplyTemplates()
        {
            return !ManageTemplates && !Transactions.Any();
        }
        private async Task ApplyTemplates()
        {
            await Repository.ApplyRecurringTemplates(_month);
            await GetRecurringTransactions();
            RaiseMultipleTransactionsModified();
        }
    }
}
