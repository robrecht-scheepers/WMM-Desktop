using System;
using System.Collections.Generic;
using System.Text;

namespace WMM.Data
{
    public interface IRepository
    {
        Transaction AddTransaction(DateTime date, string description, double amount, string comments);

        Transaction UpdateTransaction(Transaction transaction, DateTime newDate, string newDescription, double newAmount, string newComments);

        void DeleteTransaction(Transaction transaction);

        IEnumerable<Transaction> GetTransactions(DateTime dateFrom, DateTime dateTo);

        IEnumerable<Transaction> GetTransactionsForDescription(DateTime dateFrom, DateTime dateTo, string description);

        IEnumerable<Transaction> GetTransactionsForCategory(DateTime dateFrom, DateTime dateTo, string category);

        Balance GetBalance(DateTime dateFrom, DateTime dateTo);

        Balance GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category);

        Balance GetBalanceForDescription(DateTime dateFrom, DateTime dateTo, string description);
    }
}
