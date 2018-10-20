using WMM.Data;

namespace WMM.WPF
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

    public delegate void TransactionEventHandler(object sender, TransactionEventArgs args);
    public delegate void TransactionUpdateEventHandler(object sender, TransactionUpdateEventArgs args);
}
