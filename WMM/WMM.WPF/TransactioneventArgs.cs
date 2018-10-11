using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
