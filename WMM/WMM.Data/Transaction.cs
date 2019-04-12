using System;

namespace WMM.Data
{
    public class Transaction
    {
        public Guid Id { get; }

        public Category Category { get; }

        public DateTime Date { get; }

        public double Amount { get; }

        public string Comments { get; } 

        public DateTime CreatedTime { get; }

        public string CreatedAccount { get; }

        public DateTime LastUpdateTime { get; }

        public string LastUpdateAccount { get; }

        public bool Deleted { get; }

        public bool Recurring { get; }

        internal Transaction(Guid id, DateTime date, Category category, double amount, string comments, DateTime createdTime, string createdAccount, DateTime lastUpdateTime, string lastUpdateAccount, bool deleted, bool recurring)
        {
            Id = id; 
            Date = date;
            Category = category;
            Amount = amount;
            Comments = comments;
            CreatedTime = createdTime;
            CreatedAccount = createdAccount;
            LastUpdateTime = lastUpdateTime;
            LastUpdateAccount = lastUpdateAccount;
            Deleted = deleted;
            Recurring = recurring;
        }
    }
}
