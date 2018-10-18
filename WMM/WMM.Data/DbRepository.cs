using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WMM.Data.Helpers;

namespace WMM.Data
{
    public class DbRepository : IRepository
    {
        private const string DbFileName = "wmm.db3";
        private const string ConnectionStringBase = "Data Source={0}; Version=3;LockingMode=Normal; Synchronous=Off";
        private readonly string _account;
        private readonly string _dbPath;
        private readonly string _dbConnectionString;

        private Dictionary<string, List<string>> _categories;

        public DbRepository(string dbFolder)
        {
            _account = Environment.MachineName;

            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);
            _dbPath = Path.Combine(dbFolder, DbFileName);
            _dbConnectionString = string.Format(ConnectionStringBase, _dbPath);

            if (!File.Exists(_dbPath))
                CreateNewDatabase(_dbPath);
        }

        private SQLiteConnection GetConnection() => new SQLiteConnection(_dbConnectionString);

        private void CreateNewDatabase(string dbPath)
        {
            // no need to make this method async as it is executed in the constructor before the window is visible
            string createDbSql;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WMM.Data.Sql.CreateDB.sql"))
            {
                if (stream == null)
                    throw new Exception("Embedded resource for create DB script not found");
                using (var reader = new StreamReader(stream))
                {
                    createDbSql = reader.ReadToEnd();
                }
            }
            if(string.IsNullOrEmpty(createDbSql))
                throw new Exception("Created DB script is empty");

            SQLiteConnection.CreateFile(dbPath);
            using (var dbConnection = GetConnection())
            {
                var command = new SQLiteCommand(dbConnection) { CommandText = createDbSql };
                dbConnection.Open();
                command.ExecuteNonQuery(CommandBehavior.CloseConnection);
            }

            SeedDummyCategories();
        }

        public async Task Initialize()
        {
            _categories = await GetAreasAndCategories();
        }

        private void Log(string message)
        {
            Debug.WriteLine($"DBTRACE|{DateTime.Now:hh:mm:ss.fff}|{message}");
        }
        
        public async Task<Transaction> AddTransaction(DateTime date, string category, double amount, string comments, bool recurring)
        {
            var id = Guid.NewGuid();
            var now = DateTime.Now;
            const string commandText = 
                "INSERT INTO Transactions(Id,[Date],Category,Amount,Comments,CreatedTime,CreatedAccount,LastUpdateTime,LastUpdateAccount,Deleted,Recurring) " +
                "VALUES (@id,@date,(SELECT Id FROM Categories WHERE Name = @category),@amount,@comments,@createdTime,@createdAccount,@lastUpdateTime,@lastUpdateAccount,@deleted,@recurring)";
            int lines;
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
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
                    command.Parameters.AddWithValue("@recurring", recurring);

                    Log("AddTransaction-Open");
                    dbConnection.Open();
                    lines = await command.ExecuteNonQueryAsync();
                }
            }
            Log("AddTransaction-Close");

            return lines == 0
                ? null
                : await GetTransaction(id);
        }

        public async Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, string newCategory, double newAmount,
            string newComments)
        {
            const string commandText =
                "UPDATE Transactions " +
                "SET [Date] = @date, Category = (SELECT Id FROM Categories WHERE Name = @category), AMount = @amount, Comments = @comments " +
                "WHERE Id = @id";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@id", transaction.Id);
                    dbConnection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }
            return await GetTransaction(transaction.Id);
        }

        public async Task DeleteTransaction(Transaction transaction)
        {
            const string commandText =
                "UPDATE Transactions Set Deleted = 1 WHERE Id = @id";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@id", transaction.Id);
                    dbConnection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<Transaction> GetTransaction(Guid id)
        {
            const string commandText =
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id " +
                "WHERE t.Id = @id";
            
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@id", id);

                    dbConnection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    return (await ReadTransactions(reader)).SingleOrDefault();
                }
            }
            
        }

        private async Task<IEnumerable<Transaction>> GetTransactions(DateTime dateFrom, DateTime dateTo)
        {
            const string commandText =
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id " +
                "WHERE t.Date >= @dateFrom AND t.Date <= @dateTo";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);
                    dbConnection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    return await ReadTransactions(reader);
                }
            }
        }

        public async Task<Transaction> AddRecurringTemplate(string category, double amount, string comments)
        {
            var id = Guid.NewGuid();
            var now = DateTime.Now;
            int linesAffected = 0;
            const string commandText =
                "INSERT INTO Transactions(Id,Category,Amount,Comments,CreatedTime,CreatedAccount,LastUpdateTime,LastUpdateAccount,Deleted,Recurring) " +
                "VALUES (@id,(SELECT Id FROM Categories WHERE Name = @category),@amount,@comments,@createdTime,@createdAccount,@lastUpdateTime,@lastUpdateAccount,@deleted,@recurring)";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@category", category);
                    command.Parameters.AddWithValue("@amount", amount);
                    command.Parameters.AddWithValue("@comments", comments);
                    command.Parameters.AddWithValue("@createdTime", now);
                    command.Parameters.AddWithValue("@createdAccount", _account);
                    command.Parameters.AddWithValue("@lastUpdateTime", now);
                    command.Parameters.AddWithValue("@lastUpdateAccount", _account);
                    command.Parameters.AddWithValue("@deleted", 0);
                    command.Parameters.AddWithValue("@recurring", 1);
                    command.Connection = dbConnection;
                    dbConnection.Open();
                    linesAffected = await command.ExecuteNonQueryAsync(); // ==0 mean no row was added
                        
                }
            }

            return linesAffected == 0 
                ? default(Transaction)
                : await GetTransaction(id);
        }

        public async Task<IEnumerable<Transaction>> GetRecurringTemplates()
        {
            const string commandText =
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id " +
                "WHERE t.Recurring = 1 AND t.Date IS NULL";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    dbConnection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    return await ReadTransactions(reader);
                }
            }
        }

        public async Task<IEnumerable<Transaction>> GetRecurringTransactions(DateTime dateFrom, DateTime dateTo)
        {
            const string commandText =
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id " +
                "WHERE t.Date >= @dateFrom AND t.Date <= @dateTo AND Recurring = 1";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);
                    dbConnection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    return await ReadTransactions(reader);
                }
            }
        }

        public async Task ApplyRecurringTemplates(DateTime date)
        {
            var templates = await GetRecurringTemplates();
            foreach (var template in templates)
            {
                await AddTransaction(date, template.Category, template.Amount, template.Comments, true);
            }
        }

        public async Task<Balance> GetBalance(DateTime dateFrom, DateTime dateTo)
        {
            var amounts = new List<double>();
            const string commandText = 
                "SELECT Amount FROM Transactions " +
                "WHERE Deleted = 0 AND Date >= @dateFrom AND Date <= @dateTo";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                { 
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);

                    Log("CalculateBalanceFromQuery-Open");
                    command.Connection = dbConnection;
                    dbConnection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    if (!reader.HasRows)
                    {
                        Log("CalculateBalanceFromQuery-Close");
                        return default(Balance);
                    }
                    while (reader.Read())
                    {
                        amounts.Add(reader.GetDouble(0));
                    }
                    Log("CalculateBalanceFromQuery-Close");
                }
            }
            return new Balance(amounts.Where(x => x > 0).Sum(), amounts.Where(x => x < 0).Sum());
        }

        public async Task<Dictionary<string, Balance>> GetAreaBalances(DateTime dateFrom, DateTime dateTo)
        {
            var balances = new Dictionary<string,Balance>();
            var transactions = (await GetTransactions(dateFrom, dateTo)).ToList();
            var categories = await GetAreasAndCategories();

            foreach (var area in categories.Keys)
            {
                balances[area] = CalculateBalance(transactions.Where(x => categories[area].Contains(x.Category))
                    .Select(x => x.Amount).ToList());
            }

            return balances;
        }

        public async Task<Dictionary<string, Balance>> GetCategoryBalances(DateTime dateFrom, DateTime dateTo, string area)
        {
            var amounts = new Dictionary<string,List<double>>();
            var balances = new Dictionary<string, Balance>();
            const string commandText = 
                "SELECT c.Name, t.Amount " +
                "FROM Transactions t JOIN categories c ON t.Category = c.Id JOIN Areas a ON c.Area = a.Id " +
                "WHERE t.Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo AND a.Name = @area";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);
                    command.Parameters.AddWithValue("@area", area);

                    Log("CalculateNamedBalancesFromQuery-Open");
                    dbConnection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    if (!reader.HasRows)
                    {
                        Log("CalculateNamedBalancesFromQuery-Close");
                        return balances;
                    }
                    while (reader.Read())
                    {
                        var category = reader.GetString(0);
                        var amount = reader.GetDouble(1);
                        if (amounts.ContainsKey(category))
                        {
                            amounts[category].Add(amount);   
                        }
                        else
                        {
                            amounts[category] = new List<double>{amount};
                        }
                    }

                    Log("CalculateNamedBalancesFromQuery-Close");
                }
            }

            foreach (var category in amounts.Keys)
            {
                balances[category] = CalculateBalance(amounts[category]);
            }

            return balances;
        }

        public async Task<Balance> GetBalanceForArea(DateTime dateFrom, DateTime dateTo, string area)
        {
            var amounts = new List<double>();
            const string commandText =
                "SELECT t.Amount " +
                "FROM Transactions t JOIN categories c ON t.Category = c.Id JOIN Areas a ON c.Area = a.Id " +
                "WHERE t.Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo AND a.Name = @area";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);
                    command.Parameters.AddWithValue("@area", area);

                    Log("CalculateBalanceFromQuery-Open");
                    command.Connection = dbConnection;
                    dbConnection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    if (!reader.HasRows)
                    {
                        Log("CalculateBalanceFromQuery-Close");
                        return default(Balance);
                    }
                    while (reader.Read())
                    {
                        amounts.Add(reader.GetDouble(0));
                    }
                    Log("CalculateBalanceFromQuery-Close");
                }
            }

            return CalculateBalance(amounts);
        }

        public async Task<Balance> GetBalanceForCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            var amounts = new List<double>();
            const string commandText =
                "SELECT t.Amount " +
                "FROM Transactions t JOIN categories c ON t.Category = c.Id " +
                "WHERE t.Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo AND c.Name = @category";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);
                    command.Parameters.AddWithValue("@category", category);

                    Log("CalculateBalanceFromQuery-Open");
                    command.Connection = dbConnection;
                    dbConnection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    if (!reader.HasRows)
                    {
                        Log("CalculateBalanceFromQuery-Close");
                        return default(Balance);
                    }
                    while (reader.Read())
                    {
                        amounts.Add(reader.GetDouble(0));
                    }
                    Log("CalculateBalanceFromQuery-Close");
                }
            }
            return CalculateBalance(amounts);
        }

        public Task<IEnumerable<string>> GetCategories()
        {
            return Task.FromResult(_categories?.SelectMany(x => x.Value).Distinct());
        }

        private async Task<Dictionary<string,List<string>>> GetAreasAndCategories()
        {
            var dictionary = new Dictionary<string, List<string>>();
            const string commandText =
                "SELECT a.Name AS Area, c.Name AS Category FROM Categories c JOIN Areas a on c.Area = a.Id ORDER BY a.Name, c.Name";
            var command = new SQLiteCommand() { CommandText = commandText };
            using (var dbConnection = GetConnection())
            {
                command.Connection = dbConnection;
                dbConnection.Open();
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var area = reader.GetString(0);
                    var category = reader.GetString(1);
                    if(dictionary.ContainsKey(area))
                        dictionary[area].Add(category);
                    else
                        dictionary[area] = new List<string>{category};
                }
            }
            return dictionary;
        }

        public async Task<string> GetAreaForCategory(string category)
        {
            const string commandText =
                "SELECT a.Name " +
                "FROM Categories c JOIN Areas a ON c.Area = a.Id " +
                "WHERE c.Name = @category";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@category", category);
                    dbConnection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    if (!reader.HasRows)
                        return null;

                    reader.Read();
                    return reader.GetString(0);
                }
            }
        }


        private async Task<int> ExecuteNonQuery(string commandText, SQLiteParameterCollection parameters)
        {
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        command.Parameters.Add(parameters[i]);
                    }

                    dbConnection.Open();
                    return await command.ExecuteNonQueryAsync();
                }
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
            Log("ReadTransactions-Done");
            return transactions;
        }

        private Balance CalculateBalance(List<double> amounts)
        {
            return new Balance(amounts.Where(x => x > 0).Sum(), amounts.Where(x => x < 0).Sum());
        }

        private void SeedDummyCategories()
        {
            const string commandText =
                "INSERT INTO Areas(Id,Name) VALUES(@area1Id,'Area 1'); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category11Id,'category 1.1',@area1Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category12Id,'category 1.2',@area1Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category13Id,'category 1.3',@area1Id);" +
                "INSERT INTO Areas(Id,Name) VALUES(@area2Id,'Area 2'); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category21Id,'category 2.1',@area2Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category22Id,'category 2.2',@area2Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category23Id,'category 2.3',@area2Id);" +
                "INSERT INTO Areas(Id,Name) VALUES(@area3Id,'Area 3'); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category31Id,'category 3.1',@area3Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category32Id,'category 3.2',@area3Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category33Id,'category 3.3',@area3Id);" +
                "INSERT INTO Areas(Id,Name) VALUES(@area4Id,'Area 4'); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category41Id,'category 4.1',@area4Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category42Id,'category 4.2',@area4Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category43Id,'category 4.3',@area4Id);" +
                "INSERT INTO Areas(Id,Name) VALUES(@area5Id,'Area 5'); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category51Id,'category 5.1',@area5Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category52Id,'category 5.2',@area5Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category53Id,'category 5.3',@area5Id);" +
                "INSERT INTO Areas(Id,Name) VALUES(@area6Id,'Area 6'); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category61Id,'category 6.1',@area6Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category62Id,'category 6.2',@area6Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category63Id,'category 6.3',@area6Id);" +
                "INSERT INTO Areas(Id,Name) VALUES(@area7Id,'Area 7'); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category71Id,'category 7.1',@area7Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category72Id,'category 7.2',@area7Id); " +
                "INSERT INTO Categories(Id,Name,Area) VALUES(@category73Id,'category 7.3',@area7Id);";

            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@area1Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@area2Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@area3Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@area4Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@area5Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@area6Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@area7Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category11Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category12Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category13Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category21Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category22Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category23Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category31Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category32Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category33Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category41Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category42Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category43Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category51Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category52Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category53Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category61Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category62Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category63Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category71Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category72Id", Guid.NewGuid());
                    command.Parameters.AddWithValue("@category73Id", Guid.NewGuid());

                    dbConnection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }


    }
}
