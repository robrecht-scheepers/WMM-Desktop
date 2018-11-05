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
    public class EditTransactionViewModel : ObservableObject
    {
        private readonly Transaction _transaction;
        private readonly IRepository _repository;
        private DateTime _date;
        private string _category;
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
            Categories = new ObservableCollection<string>( _repository.GetCategories());
        }

        public ObservableCollection<string> Categories { get; }

        public bool EditDate => _editDate;

        public DateTime Date
        {
            get => _date;
            set => SetValue(ref _date, value);
        }

        public string Category
        {
            get => _category;
            set => SetValue(ref _category, value);
        }

        public double Amount
        {
            get => _amount;
            set => SetValue(ref _amount, value);
        }

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
                   Math.Abs(Amount - _transaction.Amount) >= 0.01;
        }
        private async Task SaveChanges()
        {
            var amount = SelectedSign == "-" ? Amount * -1.0 : Amount;

            var newTransaction = _editDate
                ? await _repository.UpdateTransaction(_transaction, Category, amount, Comments)
                : await _repository.UpdateTransaction(_transaction, Date, Category, amount, Comments);
            RaiseTransactionUpdated(_transaction, newTransaction);
        }

        public event TransactionUpdateEventHandler TransactionChanged;
        private void RaiseTransactionUpdated(Transaction oldTransaction, Transaction newTransaction)
        {
            TransactionChanged?.Invoke(this, new TransactionUpdateEventArgs(oldTransaction, newTransaction));
        }
    }
}
