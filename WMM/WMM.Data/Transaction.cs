using System;

namespace WMM.Data
{
    public struct Transaction
    {
        public Guid Id { get; }

        public string Description { get; }

        public DateTime Date { get; }

        public double Amount { get; }

        public string Comments { get; } 

        public DateTime CreatedTime { get; }

        public string CreatedAccount { get; }

        public DateTime LastUpdateTime { get; }

        public string LastUpdateAccount { get; }

        public bool Deleted { get; }

        internal Transaction(Guid id, string description, DateTime date, double amount, string comments, DateTime createdTime, string createdAccount, DateTime lastUpdateTime, string lastUpdateAccount, bool deleted)
        {
            Description = description;
            Date = date;
            Amount = amount;
            Comments = comments;
            CreatedTime = createdTime;
            CreatedAccount = createdAccount;
            LastUpdateTime = lastUpdateTime;
            LastUpdateAccount = lastUpdateAccount;
            Deleted = deleted;
            Id = id;
        }
    }
}
