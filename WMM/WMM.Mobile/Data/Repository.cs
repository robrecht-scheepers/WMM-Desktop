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
using Environment = System.Environment;

namespace WMM.Mobile.Data
{
    public class Repository
    {
        private const string DbName = "wmm.db3";
        private const string DropboxAccessToken = "P_WvdncL8IQAAAAAAAAVlJyrbC5Kbz_0_ALBXKGqotTF7cBtvS0aqZNudXpOtlc1";// generated token for testing

        private readonly string _dbPath;
        private readonly string _account;
        public Repository(string dbFolderPath)
        {
            _dbPath = Path.Combine(dbFolderPath, DbName);
            _account = Environment.MachineName;
        }

        public async Task Initialize()
        {
            // debug code to trigger download
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }

            if (!File.Exists(_dbPath))
            {
                await DownloadEmptyDatabase();
            }
        }

        private async Task DownloadEmptyDatabase()
        {
            using (var dbx = new DropboxClient(DropboxAccessToken))
            {
                var list = await dbx.Files.ListFolderAsync(string.Empty);
                var emptyDbEntry = list.Entries.FirstOrDefault(x => x.IsFile && x.Name == "wmm_empty.db3");
                if (emptyDbEntry == null)
                {
                    throw new Exception("Empty db not found in dropbox folder");
                }

                using (var response = await dbx.Files.DownloadAsync("wmm_empty.db3"))
                {
                    var bytes = await response.GetContentAsByteArrayAsync();
                    File.WriteAllBytes(_dbPath, bytes);
                }
            }
        }

        public async Task AddTransaction(DateTime date, string category, double amount, string empty)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Transaction>> GetTransactions()
        {
            throw new NotImplementedException();
        }
    }
}