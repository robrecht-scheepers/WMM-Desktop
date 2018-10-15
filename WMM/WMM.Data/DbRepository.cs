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
            _account = Environment.MachineName;

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
            var area1Id = Guid.NewGuid();
            var area2Id = Guid.NewGuid();
            var category1Id = Guid.NewGuid();
            var category2Id = Guid.NewGuid();
            var category3Id = Guid.NewGuid();

            const string commandText = 
                "INSERT INTO Areas(Id,Name) VALUES(@area1Id,@area1Name); "+
                "INSERT INTO Areas(Id,Name) VALUES(@area2Id,@area2Name); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category1Id,@category1Name,@area1Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category2Id,@category2Name,@area1Id); " +
                "Insert INTO Categories(Id, Name, Area) VALUES(@category3Id, @category3Name, @area2Id);";
            var command = new SQLiteCommand(dbConnection){CommandText = commandText};
            command.Parameters.AddWithValue("@area1Id", area1Id);
            command.Parameters.AddWithValue("@area1Name", "Area 1");
            command.Parameters.AddWithValue("@area2Id", area2Id);
            command.Parameters.AddWithValue("@area2Name", "Area 2");
            command.Parameters.AddWithValue("@category1Id", category1Id);
            command.Parameters.AddWithValue("@category1Name", "Category 1");
            command.Parameters.AddWithValue("@category2Id", category2Id);
            command.Parameters.AddWithValue("@category2Name", "Category 2");
            command.Parameters.AddWithValue("@category3Id", category3Id);
            command.Parameters.AddWithValue("@category3Name", "Category 3");

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
        
        public async Task<Transaction> AddTransaction(DateTime date, string category, double amount, string comments, bool recurring)
        {
            var id = Guid.NewGuid();
            var now = DateTime.Now;
            const string commandText = 
                "INSERT INTO Transactions(Id,[Date],Category,Amount,Comments,CreatedTime,CreatedAccount,LastUpdateTime,LastUpdateAccount,Deleted,Recurring) " +
                "VALUES (@id,@date,(SELECT Id FROM Categories WHERE Name = @category),@amount,@comments,@createdTime,@createdAccount,@lastUpdateTime,@lastUpdateAccount,@deleted,@recurring)";
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
            command.Parameters.AddWithValue("@deleted", recurring);

            int lines;
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

        public async Task<Transaction> AddRecurringTransactionTemplate(string category, double amount, string comments)
        {
            return await AddTransaction(DateTime.MinValue, category, amount, comments, true);
        }

        private async Task<Transaction> GetTransaction(Guid id)
        {
            const string commandText =
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id "+
                "WHERE t.Id = @id";
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

        private static async Task<List<Transaction>> ReadTransactions(DbDataReader reader)
        {
            var transactions = new List<Transaction>();
            if (!reader.HasRows)
                return transactions;

            while (await reader.ReadAsync())
            {
                transactions.Add(new Transaction(
                    reader.GetGuid(0),
                    reader.GetDateTimeNullSafe(1),
                    reader.GetString(2),
                    reader.GetDouble(3),
                    reader.GetStringNullSafe(4),
                    reader.GetDateTime(5),
                    reader.GetString(6),
                    reader.GetDateTime(7),
                    reader.GetString(8),
                    reader.GetBoolean(9),
                    reader.GetBoolean(10)));
            }

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetRecurringTransactionTemplates()
        {
            const string commandText =
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id " +
                "WHERE t.Recurring = 1 AND t.Date IS NULL";
            var command = new SQLiteCommand(_dbConnection) { CommandText = commandText };
            try
            {
                _dbConnection.Open();
                var reader = await command.ExecuteReaderAsync();
                return await ReadTransactions(reader);
            }
            finally
            {
                _dbConnection.Close();
            }
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
            const string commandText = 
                "SELECT Amount FROM Transactions " +
                "WHERE Deleted = 0 AND Date >= @dateFrom AND Date <= @dateTo";
            var command = new SQLiteCommand(_dbConnection) { CommandText = commandText };
            command.Parameters.AddWithValue("@dateFrom", dateFrom);
            command.Parameters.AddWithValue("@dateTo", dateTo);

            return await CalculateBalanceFromQuery(command);
        }

        public async Task<Dictionary<string, Balance>> GetAreaBalances(DateTime dateFrom, DateTime dateTo)
        {
            const string commandText = 
                "SELECT a.Name, t.Amount " +
                "FROM Transactions t JOIN categories c ON t.Category = c.Id JOIN Areas a ON c.Area = a.Id " +
                "WHERE t.Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo ";
            var command = new SQLiteCommand(_dbConnection) { CommandText = commandText };
            command.Parameters.AddWithValue("@dateFrom", dateFrom);
            command.Parameters.AddWithValue("@dateTo", dateTo);

            return await CalculateNamedBalancesFromQuery(command);
        }

        public async Task<Dictionary<string, Balance>> GetCategoryBalances(DateTime dateFrom, DateTime dateTo, string area)
        {
            const string commandText = 
                "SELECT c.Name, t.Amount " +
                "FROM Transactions t JOIN categories c ON t.Category = c.Id JOIN Areas a ON c.Area = a.Id " +
                "WHERE t.Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo AND a.Name = @area";
            var command = new SQLiteCommand(_dbConnection) { CommandText = commandText };
            command.Parameters.AddWithValue("@dateFrom", dateFrom);
            command.Parameters.AddWithValue("@dateTo", dateTo);
            command.Parameters.AddWithValue("@area", area);

            return await CalculateNamedBalancesFromQuery(command);
        }

        public async Task<Balance> GetBalanceForArea(DateTime dateFrom, DateTime dateTo, string area)
        {
            const string commandText = 
                "SELECT t.Amount " +
                "FROM Transactions t JOIN categories c ON t.Category = c.Id JOIN Areas a ON c.Area = a.Id " +
                "WHERE t.Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo AND a.Name = @area";
            var command = new SQLiteCommand(_dbConnection) { CommandText = commandText };
            command.Parameters.AddWithValue("@dateFrom", dateFrom);
            command.Parameters.AddWithValue("@dateTo", dateTo);
            command.Parameters.AddWithValue("@area", area);

            return await CalculateBalanceFromQuery(command);
        }

        public async Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            const string commandText = 
                "SELECT t.Amount " +
                "FROM Transactions t JOIN categories c ON t.Category = c.Id " +
                "WHERE t.Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo AND c.Name = @category";
            var command = new SQLiteCommand(_dbConnection) { CommandText = commandText };
            command.Parameters.AddWithValue("@dateFrom", dateFrom);
            command.Parameters.AddWithValue("@dateTo", dateTo);
            command.Parameters.AddWithValue("@category", category);

            return await CalculateBalanceFromQuery(command);
        }

        private static async Task<Balance> CalculateBalanceFromQuery(SQLiteCommand command)
        {
            var amounts = new List<double>();
            try
            {
                command.Connection.Open();
                var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return default(Balance);

                while (reader.Read())
                {
                    amounts.Add(reader.GetDouble(0));
                }
            }
            finally
            {
                command.Connection.Close();
            }

            return new Balance(amounts.Where(x => x > 0).Sum(), amounts.Where(x => x < 0).Sum());
        }

        private static async Task<Dictionary<string, Balance>> CalculateNamedBalancesFromQuery(SQLiteCommand command)
        {
            var balances = new Dictionary<string, Balance>();
            try
            {
                command.Connection.Open();
                var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return balances;

                while (reader.Read())
                {
                    var category = reader.GetString(0);
                    var amount = reader.GetDouble(1);
                    var addIncome = amount > 0 ? amount : 0;
                    var addExpense = amount < 0 ? amount : 0;
                    if (balances.ContainsKey(category))
                    {
                        balances[category] = new Balance(balances[category].Income + addIncome, balances[category].Expense + addExpense);
                    }
                    else
                    {
                        balances[category] = new Balance(addIncome, addExpense);
                    }
                }
            }
            finally
            {
                command.Connection.Close();
            }

            return balances;
        }

        public async Task<IEnumerable<string>> GetCategories()
        {
            var categories = new List<string>();
            const string commandText = 
                "SELECT Name FROM Categories ORDER BY Name ASC";
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
            const string commandText = 
                "SELECT a.Name " +
                "FROM Categories c JOIN Areas a ON c.Area = a.Id " +
                "WHERE c.Name = @category";
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
