using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WMM.Data
{
    public class DummyRepository : IRepository
    {
        private const string Account = "DummyAccount";

        private static List<Transaction> dummyTransactions = new List<Transaction>
        {
            new Transaction(Guid.NewGuid(), "Aldi", new DateTime(2018,10,1), -25.08, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Edeka", new DateTime(2018,10,1), -129.46, "Für Besuch", new DateTime(2018,10,2,13,15,06), Account, new DateTime(2018,10,2,13,32,12), Account, false),
            new Transaction(Guid.NewGuid(), "Tanken", new DateTime(2018,10,1), -25.0, "", new DateTime(2018,10,1,20,5,12), Account, new DateTime(2018,10,1,20,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Schuhen kinder", new DateTime(2018,10,4), -69.99, "Winterschuhen", new DateTime(2018,10,4,15,5,12), Account, new DateTime(2018,10,4,15,5,12), Account, false),
            new Transaction(Guid.NewGuid(), "Elterngeld", new DateTime(2018,10,15), 350.00, "", new DateTime(2018,10,15,12,6,12), Account, new DateTime(2018,10,15,12,6,12), Account, false),
        };

        private static Dictionary<string,List<string>> Categories = new Dictionary<string, List<string>>
        {
            {"Haushalt", new List<string>{"Aldi", "Edeka"}},
            {"Auto", new List<string>{"Werkstatt", "Tanken"}},
            {"Kinder", new List<string>{"Klamotten kinder", "Spielzeug"}},
            {"Einnahmen", new List<string>{"Gehalt", "Kindergeld", "Elterngeld"}},
        };

        public Transaction AddTransaction(DateTime date, string description, double amount, string comments)
        {
            var now = DateTime.Now;
            var transaction = new Transaction(Guid.NewGuid(), description, date, amount, comments, now, Account, now, Account, false);
            dummyTransactions.Add(transaction);
            return transaction;
        }

        public Transaction UpdateTransaction(Transaction transaction, DateTime newDate, string newDescription, double newAmount,
            string newComments)
        {
            var now = DateTime.Now;
            var updatedTransaction = new Transaction(transaction.Id, newDescription, newDate, newAmount, newComments, transaction.CreatedTime, transaction.CreatedAccount, now, Account, false);
            dummyTransactions.Remove(transaction);
            dummyTransactions.Add(updatedTransaction);
            return updatedTransaction;
        }

        public void DeleteTransaction(Transaction transaction)
        {
            dummyTransactions.Remove(transaction);
        }

        public IEnumerable<Transaction> GetTransactions(DateTime dateFrom, DateTime dateTo)
        {
            return dummyTransactions.Where(x => x.Date >= dateFrom && x.Date <= dateTo);
        }

        public IEnumerable<Transaction> GetTransactionsForDescription(DateTime dateFrom, DateTime dateTo, string description)
        {
            return dummyTransactions.Where(x => x.Date >= dateFrom && x.Date <= dateTo && x.Description == description);
        }

        public IEnumerable<Transaction> GetTransactionsForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            if(!Categories.ContainsKey(category))
                return new List<Transaction>();
            return dummyTransactions.Where(x => x.Date >= dateFrom && x.Date <= dateTo && Categories[category].Contains(x.Description));
        }

        public Balance GetBalance(DateTime dateFrom, DateTime dateTo)
        {
            return  CalculateBalance(GetTransactions(dateFrom, dateTo).ToList());
        }

        public Balance GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            return CalculateBalance(GetTransactionsForCategory(dateFrom, dateTo, category).ToList());
        }

        public Balance GetBalanceForDescription(DateTime dateFrom, DateTime dateTo, string description)
        {
            return CalculateBalance(GetTransactionsForDescription(dateFrom, dateTo, description).ToList());
        }

        private Balance CalculateBalance(List<Transaction> transactions)
        {
            return new Balance(
                transactions.Where(x => x.Amount > 0).Select(x => x.Amount).Sum(),
                transactions.Where(x => x.Amount < 0).Select(x => x.Amount).Sum());
        }
    }
}
