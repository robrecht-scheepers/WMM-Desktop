using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SQLite;
using Environment = System.Environment;


namespace WheresMyMoneyApp.Data
{
    public class Repository
    {
        private SQLiteConnection _db;
        public Repository()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MoneyDb.db3");
            _db = new SQLiteConnection(path);
            _db.CreateTable<Expense>();
        
            // add test data if empty
            if (!_db.Table<Expense>().Any())
            {
                var expense = new Expense
                {
                    Amount = 50.12,
                    Category = "Edeka",
                    Date = DateTime.Today
                };
                _db.Insert(expense);

                expense = new Expense
                {
                    Amount = 25.00,
                    Category = "Aldi",
                    Date = DateTime.Today.AddDays(-2)
                };
                _db.Insert(expense);
            }
        }

        public List<Expense> GetExpenses()
        {
            return _db.Table<Expense>().ToList();
        }
    }
}