using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Resources;

namespace WMM.WPF.Transactions
{
    public class TransactionListViewModelBase : ObservableObject
    {
        private ObservableCollection<Transaction> _transactions;
        private AsyncRelayCommand<Transaction> _deleteTransactionCommand;
        private RelayCommand<Transaction> _editTransactionCommand;
        protected readonly IRepository Repository;
        protected readonly IWindowService WindowService;
        private RelayCommand<Transaction> _useAsTemplateCommand;

        public TransactionListViewModelBase(IRepository repository, IWindowService windowService, bool showDate)
        {
            Repository = repository;
            WindowService = windowService;
            ShowDate = showDate;
            Transactions = new ObservableCollection<Transaction>();

            Repository.TransactionDeleted += RepositoryOnTransactionDeleted;
            Repository.TransactionUpdated += RepositoryOnTransactionUpdated;
            Repository.TransactionAdded += RepositoryOnTransactionAdded;
        }

        public bool ShowDate { get; }

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set => SetValue(ref _transactions, value);
        }

        public AsyncRelayCommand<Transaction> DeleteTransactionCommand => _deleteTransactionCommand ?? (_deleteTransactionCommand = new AsyncRelayCommand<Transaction>(DeleteTransaction));
        private async Task DeleteTransaction(Transaction transaction)
        {
            if(!WindowService.AskConfirmation(Captions.ConfirmDeleteTransaction))
                return;
            await Repository.DeleteTransaction(transaction);
        }

        public RelayCommand<Transaction> EditTransactionCommand =>
            _editTransactionCommand ?? (_editTransactionCommand = new RelayCommand<Transaction>(EditTransaction));

        private void EditTransaction(Transaction transaction)
        {
            var editTransactionViewModel = new EditTransactionViewModel(transaction, Repository, ShowDate);
            WindowService.OpenDialogWindow(editTransactionViewModel);
        }

        public RelayCommand<Transaction> UseAsTemplateCommand =>
            _useAsTemplateCommand ?? (_useAsTemplateCommand = new RelayCommand<Transaction>(UseTransactionAsTemplate));
        private void UseTransactionAsTemplate(Transaction transaction)
        {
            UseAsTemplateRequested?.Invoke(this, new TransactionEventArgs(transaction));

        }
        public event TransactionEventHandler UseAsTemplateRequested;

        
        protected virtual void RepositoryOnTransactionAdded(object sender, TransactionEventArgs args)
        {
            
        }

        protected virtual void RepositoryOnTransactionUpdated(object sender, TransactionUpdateEventArgs args)
        {
            var transaction = Transactions.FirstOrDefault(x => x.Id == args.OldTransaction.Id);
            if (transaction == null)
                return;

            var index = Transactions.IndexOf(transaction);
            Transactions.Remove(transaction);
            Transactions.Insert(index, args.NewTransaction);
        }

        protected virtual void RepositoryOnTransactionDeleted(object sender, TransactionEventArgs args)
        {
            var transaction = Transactions.FirstOrDefault(x => x.Id == args.Transaction.Id);
            if (transaction == null)
                return;

            Transactions.Remove(transaction);
        }
    }
}
