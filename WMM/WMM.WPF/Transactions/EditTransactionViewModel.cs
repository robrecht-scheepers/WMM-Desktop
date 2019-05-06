using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.MVVM;

namespace WMM.WPF.Transactions
{
    public class EditTransactionViewModel : ObservableObject
    {
        private readonly Transaction _transaction;
        private readonly IRepository _repository;
        private DateTime _date;
        private Category _category;
        private double _amount;
        private AsyncRelayCommand _saveChangesCommand;
        private readonly bool _editDate;
        private string _comments;
        private string _selectedSign;

        public EditTransactionViewModel(Transaction transaction, IRepository repository, bool editDate = true)
        {
            _transaction = transaction;
            _repository = repository;
            _editDate = editDate;

            Date = _transaction.Date;
            Category = _transaction.Category;
            SelectedSign = _transaction.Amount > 0 ? "+" : "-";
            Amount = Math.Abs(_transaction.Amount);
            Comments = _transaction.Comments;
            Categories = new ObservableCollection<Category>( _repository.GetCategories().OrderBy(x => x.Name));
        }

        public ObservableCollection<Category> Categories { get; }

        public bool EditDate => _editDate;

        public DateTime Date
        {
            get => _date;
            set => SetValue(ref _date, value);
        }

        public Category Category
        {
            get => _category;
            set => SetValue(ref _category, value);
        }

        public double Amount
        {
            get => _amount;
            set => SetValue(ref _amount, value);
        }

        private double SignedAmount => SelectedSign == "-" ? Amount * -1.0 : Amount;

        public string Comments
        {
            get => _comments;
            set => SetValue(ref _comments , value);
        }

        public List<string> Signs => new List<string> { "+", "-" };

        public string SelectedSign
        {
            get => _selectedSign;
            set => SetValue(ref _selectedSign, value);
        }

        public AsyncRelayCommand SaveChangesCommand => _saveChangesCommand ?? (_saveChangesCommand = new AsyncRelayCommand(SaveChanges,CanExecuteSaveChanges));
        private bool CanExecuteSaveChanges()
        {
            return Category != _transaction.Category || Comments != _transaction.Comments ||
                   (Date > DateTime.MinValue && Date != _transaction.Date) ||
                   Math.Abs(SignedAmount- _transaction.Amount) >= 0.01;
        }
        private async Task SaveChanges()
        {
            var newTransaction = _editDate
                ? await _repository.UpdateTransaction(_transaction, Date, Category, SignedAmount, Comments)
                : await _repository.UpdateTransaction(_transaction, Category, SignedAmount, Comments);
            RaiseTransactionUpdated(_transaction, newTransaction);
        }

        public event TransactionUpdateEventHandler TransactionChanged;
        private void RaiseTransactionUpdated(Transaction oldTransaction, Transaction newTransaction)
        {
            TransactionChanged?.Invoke(this, new TransactionUpdateEventArgs(oldTransaction, newTransaction));
        }
    }
}
