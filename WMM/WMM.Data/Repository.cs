using System;
using System.Collections.Generic;
using System.Text;

namespace WMM.Data
{
    public class Repository : IRepository
    {
        public Transaction AddTransaction(DateTime date, string description, double amount, string comments)
        {
            throw new NotImplementedException();
        }

        public Transaction UpdateTransaction(Transaction transaction, DateTime newDate, string newDescription, double newAmount,
            string newComments)
        {
            throw new NotImplementedException();
        }

        public void DeleteTransaction(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Transaction> GetTransactions(DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Transaction> GetTransactionsForDescription(DateTime dateFrom, DateTime dateTo, string description)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Transaction> GetTransactionsForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            throw new NotImplementedException();
        }

        public Balance GetBalance(DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public Balance GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            throw new NotImplementedException();
        }

        public Balance GetBalanceForDescription(DateTime dateFrom, DateTime dateTo, string description)
        {
            throw new NotImplementedException();
        }
    }
}
