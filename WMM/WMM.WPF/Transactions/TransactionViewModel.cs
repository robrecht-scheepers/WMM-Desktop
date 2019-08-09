using System;
using System.Collections.Generic;
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
        private bool _editMode;

        public TransactionViewModel(Transaction transaction, IRepository repository)
        {
            _repository = repository;
            Transaction = transaction;
        }

        public Transaction Transaction
        {
            get => _transaction;
            set => SetValue(ref _transaction, value);
        }

        public bool EditMode
        {
            get => _editMode;
            set => SetValue(ref _editMode, value);
        }
    }
}
