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

    public delegate void TransactionEventHandler(object sender, TransactionEventArgs args);
}
