﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Resources;
using WMM.WPF.Transactions;

namespace WMM.WPF.Recurring
{
    public class RecurringTransactionsViewModel : TransactionListViewModelBase
    {
        private readonly DateTime _month;
        private Category _newCategory;
        private double _newAmount;
        private AsyncRelayCommand _addCommand;
        private ObservableCollection<Category> _categories;
        private string _selectedSign;
        private AsyncRelayCommand _applyTemplatesCommand;
        private Balance _totalRecurringBalance;
        private string _newComments;

        public RecurringTransactionsViewModel(IRepository repository, IWindowService windowService, DateTime month) 
            : base(repository, windowService, false, false)
        {
            _month = month;
            ManageTemplates = false;

            Categories = new ObservableCollection<Category>();
            Repository.CategoriesUpdated += (s, a) => InitalizeCategories();
        }
        public RecurringTransactionsViewModel(IRepository repository, IWindowService windowService)
            : base(repository, windowService, false, false)
        {
            ManageTemplates = true;
            Categories = new ObservableCollection<Category>();
            Repository.CategoriesUpdated += (s, a) => InitalizeCategories();
        }

        public async Task Initialize()
        {
            InitalizeCategories();
            NewCategory = Categories.FirstOrDefault();
            NewAmount = 0.0;
            SelectedSign = "-";

            await GetItems();
        }

        private void InitalizeCategories()
        {
            Categories = new ObservableCollection<Category>(Repository.GetCategories().OrderBy(x => x.Name));
        }

        public string Title => ManageTemplates
            ? Captions.RecurringTitle
            : string.Format(Captions.TitleRecurringMonth, _month.ToString("Y"));

        public bool ManageTemplates { get; }


        public Category NewCategory
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

        public ObservableCollection<Category> Categories
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

        private async Task GetItems()
        {
            var transactions = (ManageTemplates
                    ? await Repository.GetRecurringTemplates()
                    : await Repository.GetRecurringTransactions(_month.FirstDayOfMonth(), _month.LastDayOfMonth())
                ).OrderBy(t => t.Category.Name);

            Transactions.Clear();
            foreach (var t in transactions)
            {
                Transactions.Add(t);
            }

            CalculateBalance();
        }

        private void CalculateBalance()
        {
            TotalRecurringBalance = new Balance(
                Transactions.Select(x => x.Amount).Where(x => x > 0).Sum(),
                Transactions.Select(x => x.Amount).Where(x => x < 0).Sum());
        }

        public AsyncRelayCommand AddCommand => _addCommand ?? (_addCommand = new AsyncRelayCommand(Add));
        private async Task Add()
        {
            var amount = SelectedSign == "-" ? NewAmount * -1.0 : NewAmount;

            if (ManageTemplates)
                await Repository.AddRecurringTemplate(NewCategory, amount, NewComments);
            else
                await Repository.AddTransaction(_month.FirstDayOfMonth(), NewCategory, amount, NewComments, true);

            await GetItems();

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
            await GetItems();
        }

        protected override void RepositoryOnTransactionDeleted(object sender, TransactionEventArgs args)
        {
            base.RepositoryOnTransactionDeleted(sender, args);
            CalculateBalance();
        }

        protected override void RepositoryOnTransactionUpdated(object sender, TransactionUpdateEventArgs args)
        {
            base.RepositoryOnTransactionUpdated(sender, args);
            CalculateBalance();
        }
    }
}
