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
        private readonly SQLiteConnection _dbConnection;

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

            SeedDummyCategories(dbConnection);
            
            return dbConnection;
        }

        private static void SeedDummyCategories(SQLiteConnection dbConnection)
        {
            var areaId = Guid.NewGuid();
            var category1Id = Guid.NewGuid();
            var category2Id = Guid.NewGuid();

            var commandText = "INSERT INTO Areas(Id,Name) VALUES(@areaId,@areaName); "+
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category1Id,@category1Name,@areaId); "+
                "Insert INTO Categories(Id, Name, Area) VALUES(@category2Id, @category2Name, @areaId);";
            var command = new SQLiteCommand(dbConnection){CommandText = commandText};
            command.Parameters.AddWithValue("@areaId", areaId);
            command.Parameters.AddWithValue("@areaName", "Area 51");
            command.Parameters.AddWithValue("@category1Id", category1Id);
            command.Parameters.AddWithValue("@category1Name", "Category 1");
            command.Parameters.AddWithValue("@category2Id", category2Id);
            command.Parameters.AddWithValue("@category2Name", "Category 2");

            try
            {
                dbConnection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public Task Initialize()
        {
            // will later contain synchronization and stuff, should be async as we want to show progress information
            return Task.CompletedTask;
        }
        
        public async Task<Transaction> AddTransaction(DateTime date, string category, double amount, string comments)
        {
            var id = Guid.NewGuid();
            var now = DateTime.Now;
            var commandText =
                "INSERT INTO Transactions(Id,[Date],Category,Amount,Comments,CreatedTime,CreatedAccount,LastUpdateTime,LastUpdateAccount,Deleted) " +
                "VALUES (@id,@date,(SELECT Id FROM Categories WHERE Name = @category),@amount,@comments,@createdTime,@createdAccount,@lastUpdateTime,@lastUpdateAccount,@deleted)";
            var command = new SQLiteCommand(_dbConnection) {CommandText = commandText};
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@date", date);
            command.Parameters.AddWithValue("@category", category);
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@comments", comments);
            command.Parameters.AddWithValue("@createdTime", now);
            command.Parameters.AddWithValue("@createdAccount", _account);
            command.Parameters.AddWithValue("@lastUpdateTime", now);
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
                return (await ReadTransactions(reader)).SingleOrDefault();
            }
            finally
            {
                _dbConnection.Close();
            }
        }

        private async Task<List<Transaction>> ReadTransactions(DbDataReader reader)
        {
            var transactions = new List<Transaction>();
            if (!reader.HasRows)
                return transactions;

            while (await reader.ReadAsync())
            {
                transactions.Add(new Transaction(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetDateTime(2),
                    reader.GetDouble(3),
                    reader.GetStringNullSafe(4),
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

        public async Task<Balance> GetBalance(DateTime dateFrom, DateTime dateTo)
        {
            var amounts = new List<double>();

            var commandText = "SELECT Amount FROM Transactions WHERE Deleted = 0 AND Date >= @dateFrom AND Date <= @dateTo";
            var command = new SQLiteCommand(_dbConnection) { CommandText = commandText };
            command.Parameters.AddWithValue("@dateFrom", dateFrom);
            command.Parameters.AddWithValue("@dateTo", dateTo);
            try
            {
                _dbConnection.Open();
                var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return default(Balance);

                while(reader.Read())
                {
                    amounts.Add(reader.GetDouble(0));
                }
            }
            finally
            {
                _dbConnection.Close();
            }

            return new Balance(amounts.Where(x => x > 0).Sum(), amounts.Where(x => x < 0).Sum());
        }

        public Task<Dictionary<string, Balance>> GetAreaBalances(DateTime dateFrom, DateTime dateTo)
        {
            return Task.FromResult(new Dictionary<string, Balance>());
        }

        public Task<Dictionary<string, Balance>> GetCategoryBalances(DateTime dateFrom, DateTime dateTo, string area)
        {
            return Task.FromResult(new Dictionary<string, Balance>());
        }

        public Task<Balance> GetBalanceForArea(DateTime dateFrom, DateTime dateTo, string area)
        {
            return Task.FromResult(default(Balance));
        }

        public Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            return Task.FromResult(default(Balance));
        }

        public async Task<IEnumerable<string>> GetCategories()
        {
            var categories = new List<string>();
            var commandText = "SELECT Name FROM Categories ORDER BY Name ASC";
            var command = new SQLiteCommand(_dbConnection) { CommandText = commandText };
            try
            {
                _dbConnection.Open();
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    categories.Add(reader.GetString(0));
                }
            }
            finally
            {
                _dbConnection.Close();
            }

            return categories;
        }

        public async Task<string> GetAreaForCategory(string category)
        {
            var commandText = "SELECT a.Name FROM Categories c JOIN Areas a ON c.Area = a.Id WHERE c.Name = @category";
            var command = new SQLiteCommand(_dbConnection) { CommandText = commandText };
            command.Parameters.AddWithValue("@category", category);
            try
            {
                _dbConnection.Open();
                var reader = await command.ExecuteReaderAsync();
                return reader.Read()
                    ? reader.GetString(0)
                    : null;
            }
            finally
            {
                _dbConnection.Close();
            }
        }
    }
}
