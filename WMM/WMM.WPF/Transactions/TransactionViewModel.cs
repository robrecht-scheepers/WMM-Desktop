using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.MVVM;

namespace WMM.WPF.Transactions
{
    public class TransactionViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private Transaction _transaction;
        private bool _isEditMode;
        private ObservableCollection<Category> _categories;
        private Category _editCategory;
        private DateTime _editDate;
        private double _editAmount;
        private string _editComments;
        private string _editSign;
        private AsyncRelayCommand _saveChangesCommand;

        public TransactionViewModel(Transaction transaction, IRepository repository)
        {
            _repository = repository;
            Transaction = transaction;
            IsEditMode = false;
        }

        public Transaction Transaction
        {
            get => _transaction;
            set => SetValue(ref _transaction, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetValue(ref _isEditMode, value, EditModeChanged);
        }

        private void EditModeChanged()
        {
            if(!IsEditMode)
                return;

            // initialize edit fields
            EditCategory = Transaction.Category;
            EditSign = Transaction.Amount > 0 ? "+" : "-";
            EditAmount = Math.Abs(Transaction.Amount);
            EditComments = Transaction.Comments;
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetValue(ref _categories , value);
        }

        public Category EditCategory
        {
            get => _editCategory;
            set => SetValue(ref _editCategory, value);
        }

        public DateTime EditDate
        {
            get => _editDate;
            set => SetValue(ref _editDate, value);
        }

        public double EditAmount
        {
            get => _editAmount;
            set => SetValue(ref _editAmount, value);
        }

        private double SignedEditAmount => EditSign == "-" ? EditAmount * -1.0 : EditAmount;

        public string EditComments
        {
            get => _editComments;
            set => SetValue(ref _editComments, value);
        }

        public List<string> Signs => new List<string> { "+", "-" };

        public string EditSign
        {
            get => _editSign;
            set => SetValue(ref _editSign, value);
        }

        public AsyncRelayCommand SaveChangesCommand => _saveChangesCommand ?? (_saveChangesCommand = new AsyncRelayCommand(SaveChanges));

        private async Task SaveChanges()
        {
            if(!IsEditMode)
                return;

            Transaction = await _repository.UpdateTransaction(Transaction, EditCategory, SignedEditAmount, EditComments);
            IsEditMode = false;
        }
    }
}
