using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WMM.Data
{
    public interface IRepository
    {
        Task<Transaction> AddTransaction(DateTime date, string description, double amount, string comments);

        Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, string newDescription, double newAmount, string newComments);

        Task DeleteTransaction(Transaction transaction);

        Task<IEnumerable<Transaction>> GetTransactions(DateTime dateFrom, DateTime dateTo);

        Task<IEnumerable<Transaction>> GetTransactionsForDescription(DateTime dateFrom, DateTime dateTo, string description);

        Task<IEnumerable<Transaction>> GetTransactionsForCategory(DateTime dateFrom, DateTime dateTo, string category);

        Task<Balance> GetBalance(DateTime dateFrom, DateTime dateTo);

        Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category);

        Task<Balance> GetBalanceForDescription(DateTime dateFrom, DateTime dateTo, string description);
    }
}
