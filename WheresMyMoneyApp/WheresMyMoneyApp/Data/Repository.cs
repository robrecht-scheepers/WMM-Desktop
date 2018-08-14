using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Dropbox.Api;
using Dropbox.Api.Files;
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
        
            // TEMP: add test data if empty
            if (!_db.Table<Expense>().Any())
            {
                var expense = new Expense
                {
                    Amount = 50.75,
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

        public Expense GetExpense(int id)
        {
            return _db.Find<Expense>(id);
        }

        public void SaveExpense(Expense expense)
        {
            if (_db.Table<Expense>().Any(e => e.Id == expense.Id))
            {
                _db.Update(expense);
            }
            else
            {
                _db.Insert(expense);
            }
            
        }

        public void DeleteExpense(Expense expense)
        {
            _db.Delete(expense);
        }

        public async Task Upload()
        {
            //string apiKey = "mswskukexoj5fq5";
            //var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(apiKey);

            string accessToken = "P_WvdncL8IQAAAAAAAAVkd8_lFU13NapY0NloRVLSNvIAGbTzZSc38laQZRdBLHx"; // generated token for testing
            
            using (var dbx = new DropboxClient(accessToken))
            {
                byte[] content = File.ReadAllBytes(_db.DatabasePath);
                using (var mem = new MemoryStream(content))
                {
                    var updated = await dbx.Files.UploadAsync(
                        $"/WheresMyMoney/{Path.GetFileName(Path.GetFileName(_db.DatabasePath))}", // TODO: get user name from settings and rename db file to avoid overwriting other databases
                        WriteMode.Overwrite.Instance,
                        body: mem);
                }
                // TODO: error handling
            }
        }
    }
}