using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WMM.Data
{
    public class DbRepository : IRepository
    {
        private const string DbFileName = "wmm.db3";
        private const string ConnectionString = "Data Source={0}; Version=3";

        private readonly string _dbPath;
        private SQLiteConnection _dbConnection;

        public DbRepository(string dbFolder)
        {
            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);
            _dbPath = Path.Combine(dbFolder, DbFileName);

            _dbConnection = File.Exists(_dbPath) 
                ? new SQLiteConnection(string.Format(ConnectionString, _dbPath))
                : CreateNewDatabase(_dbPath);
        }

        private SQLiteConnection CreateNewDatabase(string dbPath)
        {
            SQLiteConnection.CreateFile(_dbPath);
            _dbConnection = new SQLiteConnection(string.Format(ConnectionString, _dbPath));

            string createDbSql;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CreateDB.sql"))
            {
                if (stream == null)
                    throw new Exception("Create DB script not found");
                using (var reader = new StreamReader(stream))
                {
                    createDbSql = reader.ReadToEnd();
                }
            }
            var command = new SQLiteCommand(_dbConnection) {CommandText = createDbSql};
            command.ExecuteNonQuery();
            
            return _dbConnection;
        }


        public Task<Transaction> AddTransaction(DateTime date, string category, double amount, string comments)
        {
            throw new NotImplementedException();
        }

        public Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, string newCategory, double newAmount,
            string newComments)
        {
            throw new NotImplementedException();
        }

        public Task DeleteTransaction(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<Balance> GetBalance(DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, Balance>> GetAreaBalances(DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, Balance>> GetCategoryBalances(DateTime dateFrom, DateTime dateTo, string area)
        {
            throw new NotImplementedException();
        }

        public Task<Balance> GetBalanceForArea(DateTime dateFrom, DateTime dateTo, string area)
        {
            throw new NotImplementedException();
        }

        public Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            throw new NotImplementedException();
        }

        public Task Initialize()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetCategories()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAreaForCategory(string category)
        {
            throw new NotImplementedException();
        }
    }
}
