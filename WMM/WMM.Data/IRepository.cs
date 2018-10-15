using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WMM.Data
{
    public interface IRepository
    {
        Task Initialize();

        Task<Transaction> AddTransaction(DateTime date, string category, double amount, string comments, bool recurring = false);

        Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, string newCategory, double newAmount, string newComments);

        Task DeleteTransaction(Transaction transaction);

        Task<Transaction> AddRecurringTemplate(string category, double amount, string comments);
        
        Task<IEnumerable<Transaction>> GetRecurringTemplates();

        Task<IEnumerable<Transaction>> GetRecurringTransactions(DateTime dateFrom, DateTime dateTo);
        
        Task ApplyRecurringTemplates(DateTime date);

        Task<Balance> GetBalance(DateTime dateFrom, DateTime dateTo);

        Task<Dictionary<string,Balance>> GetAreaBalances(DateTime dateFrom, DateTime dateTo);

        Task<Dictionary<string, Balance>> GetCategoryBalances(DateTime dateFrom, DateTime dateTo, string area);

        Task<Balance> GetBalanceForArea(DateTime dateFrom, DateTime dateTo, string area);

        Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category);

        
        Task<IEnumerable<string>> GetCategories();

        Task<string> GetAreaForCategory(string category);
    }
}
