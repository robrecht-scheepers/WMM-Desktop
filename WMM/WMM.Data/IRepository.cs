﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WMM.Data
{
    public interface IRepository
    {
        Task Initialize();

        Task<Transaction> AddTransaction(DateTime date, string category, double amount, string comments, bool recurring = false);

        Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, string newCategory, double newAmount, string newComments);

        Task<Transaction> UpdateTransaction(Transaction transaction, string newCategory, double newAmount, string newComments);

        Task DeleteTransaction(Transaction transaction);

        Task<IEnumerable<Transaction>> GetTransactions();

        Task<IEnumerable<Transaction>> GetTransactions(DateTime dateFrom, DateTime dateTo, string category);

        Task<Transaction> AddRecurringTemplate(string category, double amount, string comments);
        
        Task<IEnumerable<Transaction>> GetRecurringTemplates();

        Task<Balance> GetRecurringTemplatesBalance();

        Task<IEnumerable<Transaction>> GetRecurringTransactions(DateTime dateFrom, DateTime dateTo);

        Task<Balance> GetRecurringTransactionsBalance(DateTime dateFrom, DateTime dateTo);

        Task ApplyRecurringTemplates(DateTime date);

        Task<Balance> GetBalance(DateTime dateFrom, DateTime dateTo);

        Task<Dictionary<string,Balance>> GetAreaBalances(DateTime dateFrom, DateTime dateTo);

        Task<Dictionary<string, Balance>> GetCategoryBalances(DateTime dateFrom, DateTime dateTo, string area);

        Task<Balance> GetBalanceForArea(DateTime dateFrom, DateTime dateTo, string area);

        Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category);

        
        IEnumerable<string> GetCategories();

        string GetAreaForCategory(string category);
    }
}
