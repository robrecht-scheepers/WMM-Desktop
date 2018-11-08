using System;
using WMM.Data;

namespace WMM.WPF.Transactions
{
    public class TransactionEventArgs
    {
        public TransactionEventArgs(Transaction transaction)
        {
            Transaction = transaction;
        }

        public Transaction Transaction { get; }
    }

    public class TransactionUpdateEventArgs
    {
        public TransactionUpdateEventArgs(Transaction oldTransaction, Transaction newTransaction)
        {
            OldTransaction = oldTransaction;
            NewTransaction = newTransaction;
        }

        public Transaction OldTransaction { get; }
        public Transaction NewTransaction { get; }
    }

    public class DetailTransactionsRequestEventArgs
    {
        public DetailTransactionsRequestEventArgs(DateTime dateFrom, DateTime dateTo, string category)
        {
            DateFrom = dateFrom;
            DateTo = dateTo;
            Category = category;
        }

        public DateTime DateFrom { get; }
        public DateTime DateTo { get; }
        public string Category { get; }
    }

    public delegate void TransactionEventHandler(object sender, TransactionEventArgs args);
    public delegate void TransactionUpdateEventHandler(object sender, TransactionUpdateEventArgs args);
    public delegate void DetailTransactionsRequestEventHandler(object sender, DetailTransactionsRequestEventArgs args);
}
