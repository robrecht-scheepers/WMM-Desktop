﻿using System;

namespace WMM.Data
{
    public class Transaction
    {
        public Guid Id { get; }

        public string Category { get; }

        public DateTime Date { get; }

        public double Amount { get; }

        public string Comments { get; } 

        public DateTime CreatedTime { get; }

        public string CreatedAccount { get; }

        public DateTime LastUpdateTime { get; }

        public string LastUpdateAccount { get; }

        public bool Deleted { get; }

        internal Transaction(Guid id, string category, DateTime date, double amount, string comments, DateTime createdTime, string createdAccount, DateTime lastUpdateTime, string lastUpdateAccount, bool deleted)
        {
            Category = category;
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
