using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF.Transactions
{
    public class AddTransactionsViewModel : TransactionListViewModelBase
    {
        private DateTime _newTransactionDate;
        private string _newTransactionCategory;
        private double _newTransactionAmount;
        private AsyncRelayCommand _addTransactionCommand;
        private ObservableCollection<string> _categories;
        private string _selectedSign;
        private string _newTransactionComment;

        public AddTransactionsViewModel(IRepository repository, IWindowService windowService)
            :base(repository,windowService, true)
        {
            Categories = new ObservableCollection<string>();
        }

        public Task Initialize()
        {
            Categories = new ObservableCollection<string>( Repository.GetCategoryNames().OrderBy(x => x));
            NewTransactionCategory = Categories.FirstOrDefault();
            NewTransactionDate = DateTime.Today;
            NewTransactionAmount = 0.0; 
            SelectedSign = "-";

            // currently this method runs synchronously as categories are loaded synchronously,
            // but I don't want to mix up the async initialize pattern
            return Task.CompletedTask; 
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

        public string NewTransactionComment
        {
            get => _newTransactionComment;
            set => SetValue(ref _newTransactionComment, value);
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

            var transaction = await Repository.AddTransaction(NewTransactionDate, NewTransactionCategory, amount, NewTransactionComment);

            if(!Transactions.Any()) // workaround for bug in DataGrid when adding an element to an empty list
                Transactions = new ObservableCollection<Transaction>(new List<Transaction>{transaction});
            else
                Transactions.Insert(0,transaction);

            RaiseTransactionModified(transaction);

            NewTransactionAmount = 0.00;
            NewTransactionComment = "";
        }

        public void UseTransactionAsTemplate(Transaction transaction)
        {
            NewTransactionDate = DateTime.Today;
            NewTransactionCategory = transaction.Category;
            NewTransactionAmount = System.Math.Abs(transaction.Amount);
            SelectedSign = (transaction.Amount <= 0) ? "-" : "+";
            NewTransactionComment = transaction.Comments;
        }
    }
}
