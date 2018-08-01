using System;
using SQLite;

namespace WheresMyMoneyApp.Data
{
    [Table("Expenses")]
    public class Expense : Java.Lang.Object
    {
        public Expense() { }

        public Expense(string category, double amount, DateTime date)
        {
            Category = category;
            Amount = amount;
            Date = date;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public double Amount { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }

        public DateTime Date { get; set; }

        public override string ToString()
        {
            return $"{Category} \t {Amount}";
        }
    }
}