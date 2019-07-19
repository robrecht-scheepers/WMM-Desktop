using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WMM.Data
{
    public interface IRepository
    {
        Task Initialize();

        Task<Transaction> AddTransaction(DateTime date, Category category, double amount, string comments, bool recurring = false);

        Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, Category newCategory, double newAmount, string newComments);

        Task<Transaction> UpdateTransaction(Transaction transaction, Category newCategory, double newAmount, string newComments);

        Task DeleteTransaction(Transaction transaction);

        event TransactionEventHandler TransactionAdded;
        event TransactionEventHandler TransactionDeleted;
        event TransactionUpdateEventHandler TransactionUpdated;
        event EventHandler TransactionBulkUpdated;

        Task<IEnumerable<Transaction>> GetTransactions();

        Task<IEnumerable<Transaction>> GetTransactions(DateTime dateFrom, DateTime dateTo, Category category);

        Task<IEnumerable<Transaction>> GetTransactions(SearchConfiguration searchConfiguration);

        Task<Transaction> AddRecurringTemplate(Category category, double amount, string comments);
        
        Task<IEnumerable<Transaction>> GetRecurringTemplates();

        Task<Balance> GetRecurringTemplatesBalance();

        Task<IEnumerable<Transaction>> GetRecurringTransactions(DateTime dateFrom, DateTime dateTo);

        Task<Balance> GetRecurringTransactionsBalance(DateTime dateFrom, DateTime dateTo);

        Task ApplyRecurringTemplates(DateTime date);

        Task<Balance> GetBalance(DateTime dateFrom, DateTime dateTo);

        Task<Dictionary<string,Balance>> GetAreaBalances(DateTime dateFrom, DateTime dateTo);

        Task<Dictionary<string, Balance>> GetCategoryBalances(DateTime dateFrom, DateTime dateTo, string area);

        Task<Balance> GetBalanceForArea(DateTime dateFrom, DateTime dateTo, string area);

        Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, Category category);
        
        IEnumerable<string> GetCategoryNames();

        IEnumerable<string> GetAreas();

        List<Category> GetCategories();

        Task AddArea(string area);

        Task AddCategory(string area, string category, ForecastType forecastType);

        Task EditCategory(string oldCategory, string newArea, string newCategory, ForecastType newForecastType);

        Task DeleteCategory(string category, string fallback = null);

        event EventHandler CategoriesUpdated;
    }
}
