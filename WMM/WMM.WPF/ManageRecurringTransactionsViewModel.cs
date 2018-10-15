using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.MVVM;

namespace WMM.WPF
{
    public class ManageRecurringTransactionsViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private string _newTransactionCategory;
        private double _newTransactionAmount;
        private AsyncRelayCommand _addTransactionCommand;
        private ObservableCollection<string> _categories;
        private string _selectedSign;

        public ManageRecurringTransactionsViewModel(IRepository repository)
        {
            _repository = repository;
            Categories = new ObservableCollection<string>();
            FixedTransactions = new ObservableCollection<Transaction>();
        }

        public async Task Initialize()
        {
            Categories = new ObservableCollection<string>(await _repository.GetCategories());
            NewTransactionCategory = Categories.FirstOrDefault();
            NewTransactionAmount = 0.0;
            SelectedSign = "-";

            await GetRecurringTransactionTemplates();
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

        public List<string> Signs => new List<string> { "+", "-" };

        public string SelectedSign
        {
            get => _selectedSign;
            set => SetValue(ref _selectedSign, value);
        }

        private async Task GetRecurringTransactionTemplates()
        {

        }

        public AsyncRelayCommand AddTransactionCommand => _addTransactionCommand ?? (_addTransactionCommand = new AsyncRelayCommand(AddTransaction));
        private async Task AddTransaction()
        {
            var amount = SelectedSign == "-" ? NewTransactionAmount * -1.0 : NewTransactionAmount;

            var transaction = await _repository.AddTransaction(DateTime.MinValue, NewTransactionCategory, amount, null, true);
            AddedTransactions.Add(transaction);
        }

        public ObservableCollection<Transaction> FixedTransactions { get; }
    }
}
