using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WMM.Data
{
    public class DummyRepository : IRepository
    {
        private const string Account = "DummyAccount";

        private static readonly List<Transaction> DummyTransactions = new List<Transaction>
        {
            new Transaction(Guid.NewGuid(), "Aldi", new DateTime(2018,10,1), -25.08, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Edeka", new DateTime(2018,10,1), -129.46, "Für Besuch", new DateTime(2018,10,2,13,15,06), Account, new DateTime(2018,10,2,13,32,12), Account, false),
            new Transaction(Guid.NewGuid(), "Tanken", new DateTime(2018,10,1), -25.0, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Schuhen kinder", new DateTime(2018,10,4), -69.99, "Winterschuhen", new DateTime(2018,10,4,15,5,12), Account, new DateTime(2018,10,4,15,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Elterngeld", new DateTime(2018,10,15), 350.00, "", new DateTime(2018,10,15,12,6,12), Account, new DateTime(2018,10,15,12,6,12), Account, false),
        };

        private static readonly Dictionary<string,List<string>> Categories = new Dictionary<string, List<string>>
        {
            {"Haushalt", new List<string>{"Aldi", "Edeka"}},
            {"Auto", new List<string>{"Werkstatt", "Tanken"}},
            {"Kinder", new List<string>{"Klamotten kinder", "Spielzeug"}},
            {"Einnahmen", new List<string>{"Gehalt", "Kindergeld", "Elterngeld"}},
        };

        public Task<Transaction> AddTransaction(DateTime date, string description, double amount, string comments)
        {
            var now = DateTime.Now;
            var transaction = new Transaction(Guid.NewGuid(), description, date, amount, comments, now, Account, now, Account, false);
            DummyTransactions.Add(transaction);
            return Task.FromResult(transaction);
        }

        public Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, string newDescription, double newAmount,
            string newComments)
        {
            var now = DateTime.Now;
            var updatedTransaction = new Transaction(transaction.Id, newDescription, newDate, newAmount, newComments, transaction.CreatedTime, transaction.CreatedAccount, now, Account, false);
            DummyTransactions.Remove(transaction);
            DummyTransactions.Add(updatedTransaction);
            return Task.FromResult(updatedTransaction);
        }

        public Task DeleteTransaction(Transaction transaction)
        {
            DummyTransactions.Remove(transaction);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Transaction>> GetTransactions(DateTime dateFrom, DateTime dateTo)
        {
            return Task.FromResult(DummyTransactions.Where(x => x.Date >= dateFrom && x.Date <= dateTo));
        }

        public Task<IEnumerable<Transaction>> GetTransactionsForDescription(DateTime dateFrom, DateTime dateTo, string description)
        {
            return Task.FromResult(DummyTransactions.Where(x => x.Date >= dateFrom && x.Date <= dateTo && x.Description == description));
        }

        public Task<IEnumerable<Transaction>> GetTransactionsForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            return Categories.ContainsKey(category)
                ? Task.FromResult(DummyTransactions.Where(x => x.Date >= dateFrom && x.Date <= dateTo && Categories[category].Contains(x.Description)))
                : Task.FromResult((IEnumerable<Transaction>) new List<Transaction>());
        }

        public Task<Balance> GetBalance(DateTime dateFrom, DateTime dateTo)
        {
            return Task.FromResult(CalculateBalance(GetTransactions(dateFrom, dateTo).Result.ToList()));
        }

        public Task<Dictionary<string, Balance>> GetCategoryBalances(DateTime dateFrom, DateTime dateTo)
        {
            var categoryBalances = new Dictionary<string,Balance>();
            var transactions = GetTransactions(dateFrom, dateTo).Result.ToList();
            foreach (var category in Categories.Keys)
            {
                if(!transactions.Any(x => Categories[category].Contains(x.Description))) continue;

                categoryBalances[category] = GetBalanceForCategory(dateFrom, dateTo, category).Result;
            }

            return Task.FromResult(categoryBalances);
        }

        public Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            return Task.FromResult(CalculateBalance(GetTransactionsForCategory(dateFrom, dateTo, category).Result.ToList()));
        }

        public Task<Balance> GetBalanceForDescription(DateTime dateFrom, DateTime dateTo, string description)
        {
            return Task.FromResult(CalculateBalance(GetTransactionsForDescription(dateFrom, dateTo, description).Result.ToList()));
        }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        private Balance CalculateBalance(List<Transaction> transactions)
        {
            return new Balance(
                transactions.Where(x => x.Amount > 0).Select(x => x.Amount).Sum(),
                transactions.Where(x => x.Amount < 0).Select(x => x.Amount).Sum());
        }
    }
}
