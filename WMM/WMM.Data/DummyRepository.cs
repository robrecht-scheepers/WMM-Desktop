using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMM.Data
{
    public class DummyRepository : IRepository
    {
        private const string Account = "DummyAccount";

        private static readonly List<Transaction> DummyTransactions = new List<Transaction>
        {
            new Transaction(Guid.NewGuid(), new DateTime(2018,10,1), "Supermarkt", -25.08, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,10,1), "Tanken", -25.0, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,10,4), "Kinderkleidung", -69.99, "Winterschuhen", new DateTime(2018,10,4,15,5,12), Account, new DateTime(2018,10,4,15,5,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,10,15), "Elterngeld", 350.00, "", new DateTime(2018,10,15,12,6,12), Account, new DateTime(2018,10,15,12,6,12), Account, false, true),
            new Transaction(Guid.NewGuid(), new DateTime(2018,9,1), "Supermarkt", -35.08, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,9,1), "Supermarkt", -129.46, "Für Besuch", new DateTime(2018,10,2,13,15,06), Account, new DateTime(2018,10,2,13,32,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,9,1), "Tanken", -25.0, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,9,4), "Kinderkleidung", -69.99, "Winterschuhen", new DateTime(2018,10,4,15,5,12), Account, new DateTime(2018,10,4,15,5,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,9,15), "Elterngeld", 350.00, "", new DateTime(2018,10,15,12,6,12), Account, new DateTime(2018,10,15,12,6,12), Account, false, true),
            new Transaction(Guid.NewGuid(), new DateTime(2018,8,1), "Supermarkt", -25.08, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,8,1), "Supermarkt", -129.46, "Für Besuch", new DateTime(2018,10,2,13,15,06), Account, new DateTime(2018,10,2,13,32,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,8,1), "Tanken", -25.0, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,8,4), "Kinderkleidung", -69.99, "Winterschuhen", new DateTime(2018,10,4,15,5,12), Account, new DateTime(2018,10,4,15,5,12), Account, false, false),
            new Transaction(Guid.NewGuid(), new DateTime(2018,8,15), "Elterngeld", 350.00, "", new DateTime(2018,10,15,12,6,12), Account, new DateTime(2018,10,15,12,6,12), Account, false, true),
        };

        private static readonly Dictionary<string,List<string>> Categories = new Dictionary<string, List<string>>
        {
            {"Haushalt", new List<string>{"Supermarkt", "Drogerie", "Essen unterwegs"}},
            {"Auto", new List<string>{"Werkstatt", "Tanken", "Parking", "PKW steuer", "PKW versicherung"}},
            {"Kinder", new List<string>{"Kinderkleidung", "Spielzeug", "Pflege", "Kindergeld", "Kita"}},
            {"Medisch", new List<string>{"Arzt", "Apotheke", "Barmenia premie", "Barmenia rückzahlung"}},
            {"Freizeit", new List<string>{"Urlaub", "Restaurant & Cafe", "Party", "Bücher & Media"}},
            {"Haus", new List<string>{"Abzahlung" ,"Nebenkosten", "Baumarkt", "Ausstattung"}},
            {"Gehalt", new List<string>{"Gehalt", "Elterngeld"}},
            {"Versicherung", new List<string>{"versicherung"}},
        };

        public Task<Transaction> AddTransaction(DateTime date, string category, double amount, string comments, bool recurring = false)
        {
            var now = DateTime.Now;
            var transaction = new Transaction(Guid.NewGuid(), date, category, amount, comments, now, Account, now, Account, false, recurring);
            DummyTransactions.Add(transaction);
            return Task.FromResult(transaction);
        }

        public Task<Transaction> AddRecurringTemplate(string category, double amount, string comments)
        {
            return AddTransaction(DateTime.MinValue, category, amount, comments, true);
        }

        public Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, string newCategory, double newAmount,
            string newComments)
        {
            var now = DateTime.Now;
            var updatedTransaction = new Transaction(transaction.Id, newDate, newCategory, newAmount, newComments, transaction.CreatedTime, transaction.CreatedAccount, now, Account, transaction.Deleted, transaction.Recurring);
            DummyTransactions.Remove(transaction);
            DummyTransactions.Add(updatedTransaction);
            return Task.FromResult(updatedTransaction);
        }

        public Task DeleteTransaction(Transaction transaction)
        {
            DummyTransactions.Remove(transaction);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Transaction>> GetRecurringTemplates()
        {
            return Task.FromResult(DummyTransactions.Where(x => x.Recurring));
        }

        public Task<IEnumerable<Transaction>> GetRecurringTransactions(DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PeriodHasRecurringTransactions(DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public Task ApplyRecurringTemplates(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<Balance> GetBalance(DateTime dateFrom, DateTime dateTo)
        {
            return Task.FromResult(CalculateBalance(GetTransactions(dateFrom, dateTo).Result.ToList()));
        }

        public Task<Dictionary<string, Balance>> GetAreaBalances(DateTime dateFrom, DateTime dateTo)
        {
            var balances = new Dictionary<string,Balance>();
            var transactions = GetTransactions(dateFrom, dateTo).Result.ToList();
            foreach (var area in Categories.Keys)
            {
                balances[area] = CalculateBalance(transactions.Where(x => Categories[area].Contains(x.Category)).ToList());
            }

            return Task.FromResult(balances);
        }

        public Task<Dictionary<string, Balance>> GetCategoryBalances(DateTime dateFrom, DateTime dateTo, string area)
        {
            var balances = new Dictionary<string, Balance>();
            var transactions = GetTransactions(dateFrom, dateTo).Result.ToList();
            foreach (var category in Categories[area])
            {
                if (!transactions.Any(x => x.Category == category)) continue;

                balances[category] = CalculateBalance(transactions.Where(x => x.Category == category).ToList());
            }

            return Task.FromResult(balances);
        }

        public Task<Balance> GetBalanceForArea(DateTime dateFrom, DateTime dateTo, string area)
        {
            return Task.FromResult(CalculateBalance(GetTransactionsForArea(dateFrom, dateTo, area).Result.ToList()));
        }

        public Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            return Task.FromResult(CalculateBalance(GetTransactionsForCategory(dateFrom, dateTo, category).Result.ToList()));
        }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetCategories()
        {
            return Task.FromResult(Categories.SelectMany(x => x.Value));
        }

        public Task<string> GetAreaForCategory(string category)
        {
            var result = Categories.Values.Any(x => x.Contains(category))
                ? Categories.First(x => x.Value.Contains(category)).Key
                : null;
            return Task.FromResult(result);
        }

        private Balance CalculateBalance(List<Transaction> transactions)
        {
            return new Balance(
                transactions.Where(x => x.Amount > 0).Select(x => x.Amount).Sum(),
                transactions.Where(x => x.Amount < 0).Select(x => x.Amount).Sum());
        }

        private Task<IEnumerable<Transaction>> GetTransactions(DateTime dateFrom, DateTime dateTo)
        {
            return Task.FromResult(DummyTransactions.Where(x => x.Date >= dateFrom && x.Date <= dateTo));
        }

        private Task<IEnumerable<Transaction>> GetTransactionsForCategory(DateTime dateFrom, DateTime dateTo, string Category)
        {
            return Task.FromResult(DummyTransactions.Where(x => x.Date >= dateFrom && x.Date <= dateTo && x.Category == Category));
        }

        private Task<IEnumerable<Transaction>> GetTransactionsForArea(DateTime dateFrom, DateTime dateTo, string area)
        {
            return Categories.ContainsKey(area)
                ? Task.FromResult(DummyTransactions.Where(
                    x => x.Date >= dateFrom && x.Date <= dateTo && Categories[area].Contains(x.Category)))
                : Task.FromResult((IEnumerable<Transaction>)new List<Transaction>());
        }


        
    }
}
