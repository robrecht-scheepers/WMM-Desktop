using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF
{
    public class TransactionListViewModelBase : ObservableObject
    {
        private ObservableCollection<Transaction> _transactions;
        private AsyncRelayCommand<Transaction> _deleteTransactionCommand;
        private RelayCommand<Transaction> _editTransactionCommand;
        protected readonly IRepository Repository;
        protected readonly IWindowService WindowService;
        private readonly bool _showDate;

        public TransactionListViewModelBase(IRepository repository, IWindowService windowService, bool showDate)
        {
            Repository = repository;
            WindowService = windowService;
            _showDate = showDate;
            Transactions = new ObservableCollection<Transaction>();
        }

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set => SetValue(ref _transactions, value);
        }

        public AsyncRelayCommand<Transaction> DeleteTransactionCommand => _deleteTransactionCommand ?? (_deleteTransactionCommand = new AsyncRelayCommand<Transaction>(DeleteTransaction));
        private async Task DeleteTransaction(Transaction transaction)
        {
            await Repository.DeleteTransaction(transaction);
            Transactions.Remove(transaction);
            RaiseTransactionModified(transaction);
        }

        public RelayCommand<Transaction> EditTransactionCommand =>
            _editTransactionCommand ?? (_editTransactionCommand = new RelayCommand<Transaction>(EditTransaction));

        private void EditTransaction(Transaction transaction)
        {
            var editTransactionViewModel = new EditTransactionViewModel(transaction, Repository, _showDate);
            editTransactionViewModel.TransactionChanged += (sender, args) =>
            {
                var index = Transactions.IndexOf(args.OldTransaction);
                Transactions.Remove(args.OldTransaction);
                Transactions.Insert(index, args.NewTransaction);
                RaiseTransactionModified(args.OldTransaction);
                RaiseTransactionModified(args.NewTransaction);
            };
            WindowService.OpenDialogWindow(editTransactionViewModel);
        }

        public event TransactionEventHandler TransactionModified;
        protected void RaiseTransactionModified(Transaction transaction)
        {
            TransactionModified?.Invoke(this, new TransactionEventArgs(transaction));
        }

        public event TransactionEventHandler MultipleTransactionsModified;
        protected void RaiseMultipleTransactionsModified()
        {
            MultipleTransactionsModified?.Invoke(this, new TransactionEventArgs(null));
        }

        public void Show(IEnumerable<Transaction> transactions)
        {
            Transactions = new ObservableCollection<Transaction>(transactions);
        }
    }
}
