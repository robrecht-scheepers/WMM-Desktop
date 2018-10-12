using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SQLite;
using System.IO;
using System.Linq;
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

        private readonly string _account;
        private readonly string _dbPath;
        private SQLiteConnection _dbConnection;

        public DbRepository(string dbFolder)
        {
            _account = System.Environment.MachineName;

            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);
            _dbPath = Path.Combine(dbFolder, DbFileName);

            _dbConnection = File.Exists(_dbPath) 
                ? new SQLiteConnection(string.Format(ConnectionString, _dbPath))
                : CreateNewDatabase(_dbPath);
        }

        private static SQLiteConnection CreateNewDatabase(string dbPath)
        {
            // no need to make this method async as it is executed in the constructor before the window is visible
            string createDbSql;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WMM.Data.CreateDB.sql"))
            {
                if (stream == null)
                    throw new Exception("Embedded resource for create DB script not found");
                using (var reader = new StreamReader(stream))
                {
                    createDbSql = reader.ReadToEnd();
                }
            }
            if(string.IsNullOrEmpty(createDbSql))
                throw new Exception("Created DB script found");

            SQLiteConnection.CreateFile(dbPath);
            var dbConnection = new SQLiteConnection(string.Format(ConnectionString, dbPath));
            var command = new SQLiteCommand(dbConnection) {CommandText = createDbSql};
            
            try
            {
                dbConnection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                dbConnection.Close();
            }
            
            return dbConnection;
        }

        public Task Initialize()
        {
            // will later contain synchronization and stuff, shoul dbe async as we want to show progress information
            return Task.CompletedTask;
        }
        
        public async Task<Transaction> AddTransaction(DateTime date, string category, double amount, string comments)
        {
            var id = Guid.NewGuid();
            var now = DateTime.Now;
            var commandText =
                "INSERT INTO Transactions(Id,[Date],Category,Amount,Comments,CreatedTime,CreatedAccount,LastUpdateTime,LastUpdateAccount,Deleted) " +
                "VALUES (@id,@date,@category,@amount,@comments,@createdTime,@createdAccount,@lastUpdateTime,@lastUpdateAccount,@deleted)";
            var command = new SQLiteCommand(_dbConnection) {CommandText = commandText};
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@date", date.ToString("YYYY-MM-dd"));
            command.Parameters.AddWithValue("@category", GetCategoryId(category));
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@comments", comments);
            command.Parameters.AddWithValue("@createdTime", now.ToString("YYYY-MM-dd HH:mm:ss.fff"));
            command.Parameters.AddWithValue("@createdAccount", _account);
            command.Parameters.AddWithValue("@lastUpdateTime", now.ToString("YYYY-MM-dd HH:mm:ss.fff"));
            command.Parameters.AddWithValue("@lastUpdateAccount", _account);
            command.Parameters.AddWithValue("@deleted", 0);

            int lines = 0;
            try
            {
                _dbConnection.Open();
                lines = await command.ExecuteNonQueryAsync();
            }
            finally
            {
                _dbConnection.Close();
            }

            return lines == 0
                ? null
                : await GetTransaction(id);
        }

        private Guid GetCategoryId(string category)
        {
            throw new NotImplementedException();
        }

        private async Task<Transaction> GetTransaction(Guid id)
        {
            var commandText = "SELECT t.Id,c.Name,t.[Date],t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted  "
                              + "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id "
                              +"WHERE t.Id = @id";
            var command = new SQLiteCommand(_dbConnection) { CommandText = commandText };
            command.Parameters.AddWithValue("@id", id);
            try
            {
                _dbConnection.Open();
                var reader = await command.ExecuteReaderAsync();
                return ReadTransactions(reader).SingleOrDefault();
            }
            finally
            {
                _dbConnection.Close();
            }
        }

        private List<Transaction> ReadTransactions(DbDataReader reader)
        {
            var transactions = new List<Transaction>();
            while (reader.Read())
            {
                transactions.Add(new Transaction(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetDateTime(2),
                    reader.GetDouble(3),
                    reader.GetString(4),
                    reader.GetDateTime(5),
                    reader.GetString(6),
                    reader.GetDateTime(7),
                    reader.GetString(8),
                    reader.GetBoolean(9)));
            }

            return transactions;
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
