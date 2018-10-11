using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WMM.Data
{
    public interface IRepository
    {
        Task<Transaction> AddTransaction(DateTime date, string category, double amount, string comments);

        Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, string newCategory, double newAmount, string newComments);

        Task DeleteTransaction(Transaction transaction);

        Task<IEnumerable<Transaction>> GetTransactions(DateTime dateFrom, DateTime dateTo);

        Task<IEnumerable<Transaction>> GetTransactionsForCategory(DateTime dateFrom, DateTime dateTo, string category);

        Task<IEnumerable<Transaction>> GetTransactionsForArea(DateTime dateFrom, DateTime dateTo, string area);

        Task<Balance> GetBalance(DateTime dateFrom, DateTime dateTo);

        Task<Dictionary<string,Balance>> GetAreaBalances(DateTime dateFrom, DateTime dateTo);

        Task<Dictionary<string, Balance>> GetCategoryBalances(DateTime dateFrom, DateTime dateTo, string area);

        Task<Balance> GetBalanceForArea(DateTime dateFrom, DateTime dateTo, string area);

        Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category);

        Task Initialize();

        Task<IEnumerable<string>> GetCategories();

        Task<string> GetArea(string category);
    }
}
