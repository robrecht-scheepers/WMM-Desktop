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
            new Transaction(Guid.NewGuid(), "Supermarkt", new DateTime(2018,10,1), -25.08, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Supermarkt", new DateTime(2018,10,1), -129.46, "Für Besuch", new DateTime(2018,10,2,13,15,06), Account, new DateTime(2018,10,2,13,32,12), Account, false),
            new Transaction(Guid.NewGuid(), "Tanken", new DateTime(2018,10,1), -25.0, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Kinderkleidung", new DateTime(2018,10,4), -69.99, "Winterschuhen", new DateTime(2018,10,4,15,5,12), Account, new DateTime(2018,10,4,15,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Elterngeld", new DateTime(2018,10,15), 350.00, "", new DateTime(2018,10,15,12,6,12), Account, new DateTime(2018,10,15,12,6,12), Account, false),
            new Transaction(Guid.NewGuid(), "Supermarkt", new DateTime(2018,9,1), -35.08, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Supermarkt", new DateTime(2018,9,1), -129.46, "Für Besuch", new DateTime(2018,10,2,13,15,06), Account, new DateTime(2018,10,2,13,32,12), Account, false),
            new Transaction(Guid.NewGuid(), "Tanken", new DateTime(2018,9,1), -25.0, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Kinderkleidung", new DateTime(2018,9,4), -69.99, "Winterschuhen", new DateTime(2018,10,4,15,5,12), Account, new DateTime(2018,10,4,15,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Elterngeld", new DateTime(2018,9,15), 350.00, "", new DateTime(2018,10,15,12,6,12), Account, new DateTime(2018,10,15,12,6,12), Account, false),
            new Transaction(Guid.NewGuid(), "Supermarkt", new DateTime(2018,8,1), -25.08, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Supermarkt", new DateTime(2018,8,1), -129.46, "Für Besuch", new DateTime(2018,10,2,13,15,06), Account, new DateTime(2018,10,2,13,32,12), Account, false),
            new Transaction(Guid.NewGuid(), "Tanken", new DateTime(2018,8,1), -25.0, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Kinderkleidung", new DateTime(2018,8,4), -69.99, "Winterschuhen", new DateTime(2018,10,4,15,5,12), Account, new DateTime(2018,10,4,15,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Elterngeld", new DateTime(2018,8,15), 350.00, "", new DateTime(2018,10,15,12,6,12), Account, new DateTime(2018,10,15,12,6,12), Account, false),
        };

        private static readonly Dictionary<string,List<string>> Categories = new Dictionary<string, List<string>>
        {
            {"Haushalt", new List<string>{"Supermarkt", "Drogerie", "Essen unterwegs"}},
            {"Auto", new List<string>{"Werkstatt", "Tanken", "Parking", "PKW Steuer", "PKW Versicherung"}},
            {"Kinder", new List<string>{"Kinderkleidung", "Spielzeug", "Pflege", "Kindergeld", "Kita"}},
            {"Medisch", new List<string>{"Arzt", "Apotheke", "Barmenia Premie", "Barmenia Rückzahlung"}},
            {"Freizeit", new List<string>{"Urlaub", "Reszaurant & Cafe", "Party", "Bücher & Media"}},
            {"Haus", new List<string>{"Abzahlung und Fixkosten", "Baumarkt", "Ausstattung"}},
            {"Gehalt", new List<string>{"Gehalt", "Elterngeld"}},
            {"Versicherung", new List<string>{"Renteversicherung", "Berufsunfähgkeitsversicherung", "Lebensversicherung"}},
        };

        public Task<Transaction> AddTransaction(DateTime date, string category, double amount, string comments)
        {
            var now = DateTime.Now;
            var transaction = new Transaction(Guid.NewGuid(), category, date, amount, comments, now, Account, now, Account, false);
            DummyTransactions.Add(transaction);
            return Task.FromResult(transaction);
        }

        public Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, string newCategory, double newAmount,
            string newComments)
        {
            var now = DateTime.Now;
            var updatedTransaction = new Transaction(transaction.Id, newCategory, newDate, newAmount, newComments, transaction.CreatedTime, transaction.CreatedAccount, now, Account, false);
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

        public Task<IEnumerable<Transaction>> GetTransactionsForCategory(DateTime dateFrom, DateTime dateTo, string Category)
        {
            return Task.FromResult(DummyTransactions.Where(x => x.Date >= dateFrom && x.Date <= dateTo && x.Category == Category));
        }

        public Task<IEnumerable<Transaction>> GetTransactionsForArea(DateTime dateFrom, DateTime dateTo, string area)
        {
            return Categories.ContainsKey(area)
                ? Task.FromResult(DummyTransactions.Where(
                    x => x.Date >= dateFrom && x.Date <= dateTo && Categories[area].Contains(x.Category)))
                : Task.FromResult((IEnumerable<Transaction>) new List<Transaction>());
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

        private Balance CalculateBalance(List<Transaction> transactions)
        {
            return new Balance(
                transactions.Where(x => x.Amount > 0).Select(x => x.Amount).Sum(),
                transactions.Where(x => x.Amount < 0).Select(x => x.Amount).Sum());
        }

        public Task<IEnumerable<string>> GetCategories()
        {
            return Task.FromResult(Categories.SelectMany(x => x.Value));
        }

        public Task<string> GetArea(string category)
        {
            var result = Categories.Values.Any(x => x.Contains(category))
                ? Categories.First(x => x.Value.Contains(category)).Key
                : null;
            return Task.FromResult(result);
        }
    }
}
