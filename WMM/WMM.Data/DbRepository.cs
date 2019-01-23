using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        #region constructor and initialization
        private static readonly Dictionary<string, List<string>> InitialCategories = new Dictionary<string, List<string>>
        {
            {"Haushalt", new List<string>{"Supermarkt", "Drogerie", "Essen unterwegs"}},
            {"Auto", new List<string>{"Werkstatt", "Tanken", "Parking", "PKW steuer", "PKW versicherung"}},
            {"Kinder", new List<string>{"Kinderkleidung", "Spielzeug", "Pflege", "Kindergeld", "Kita"}},
            {"Medisch", new List<string>{"Arzt", "Apotheke", "Barmenia premie", "Barmenia rückzahlung"}},
            {"Freizeit", new List<string>{"Urlaub", "Restaurant & Cafe", "Party", "Bücher & Media"}},
            {"Haus", new List<string>{"Abzahlung" ,"Nebenkosten", "Baumarkt", "Ausstattung"}},
            {"Gehalt", new List<string>{"Gehalt", "Elterngeld"}},
            {"Versicherung", new List<string>{"Versicherung"}},
            {"Sonstiges", new List<string>{"Sonstiges"}},
        };

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

            SeedCategories(InitialCategories);
        }

        public async Task Initialize()
        {
            _categories = await GetAreasAndCategories();
        }
        #endregion

        #region transactions
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

                    dbConnection.Open();
                    lines = await command.ExecuteNonQueryAsync();
                }
            }
            
            return lines == 0
                ? null
                : await GetTransaction(id);
        }

        public async Task<Transaction> UpdateTransaction(Transaction transaction, DateTime newDate, string newCategory, double newAmount,
            string newComments)
        {
            const string commandText =
                "UPDATE Transactions " +
                "SET [Date] = @date, Category = (SELECT Id FROM Categories WHERE Name = @category), Amount = @amount, Comments = @comments, LastUpdateTime = @now,LastUpdateAccount = @account " +
                "WHERE Id = @id";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@id", transaction.Id);
                    command.Parameters.AddWithValue("@date", newDate);
                    command.Parameters.AddWithValue("@category", newCategory);
                    command.Parameters.AddWithValue("@amount", newAmount);
                    command.Parameters.AddWithValue("@comments", newComments);
                    command.Parameters.AddWithValue("@now", DateTime.Now);
                    command.Parameters.AddWithValue("@account", _account);
                    dbConnection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }
            return await GetTransaction(transaction.Id);
        }

        public async Task<Transaction> UpdateTransaction(Transaction transaction, string newCategory, double newAmount, string newComments)
        {
            const string commandText =
                "UPDATE Transactions " +
                "SET Category = (SELECT Id FROM Categories WHERE Name = @category), Amount = @amount, Comments = @comments, LastUpdateTime = @now,LastUpdateAccount = @account " +
                "WHERE Id = @id";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    command.Parameters.AddWithValue("@id", transaction.Id);
                    command.Parameters.AddWithValue("@category", newCategory);
                    command.Parameters.AddWithValue("@amount", newAmount);
                    command.Parameters.AddWithValue("@comments", newComments);
                    command.Parameters.AddWithValue("@now", DateTime.Now);
                    command.Parameters.AddWithValue("@account", _account);
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
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return (await ReadTransactions(reader)).SingleOrDefault();
                    }
                }
            }
            
        }

        public async Task<IEnumerable<Transaction>> GetTransactions()
        {
            const string commandText =
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id" +
                "WHERE Deleted = 0";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return await ReadTransactions(reader);
                    }
                }
            }
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(SearchConfiguration searchConfiguration)
        {
            var commandTextBuilder = new StringBuilder(
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t JOIN categories c ON t.Category = c.Id JOIN Areas a ON c.Area = a.Id " +
                "WHERE Deleted = 0");
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection))
                {
                    if (searchConfiguration.Parameters.HasFlag(SearchParameter.Date))
                    {
                        commandTextBuilder.AppendLine("AND t.Date >= @dateFrom AND t.Date <= @dateTo");
                        command.Parameters.AddWithValue("@dateFrom", searchConfiguration.DateFrom);
                        command.Parameters.AddWithValue("@dateTo", searchConfiguration.DateTo);
                    }
                    if (searchConfiguration.Parameters.HasFlag(SearchParameter.Area))
                    {
                        commandTextBuilder.AppendLine("AND a.Name = @area");
                        command.Parameters.AddWithValue("@area", searchConfiguration.Area);
                    }
                    if (searchConfiguration.Parameters.HasFlag(SearchParameter.Category))
                    {
                        commandTextBuilder.AppendLine("AND c.Name = @category");
                        command.Parameters.AddWithValue("@category", searchConfiguration.Category);
                    }
                    if (searchConfiguration.Parameters.HasFlag(SearchParameter.Comments))
                    {
                        commandTextBuilder.AppendLine($"AND t.Comments LIKE '%{searchConfiguration.Comments.Trim()}%");
                    }

                    if (searchConfiguration.Parameters.HasFlag(SearchParameter.Amount))
                    {
                        commandTextBuilder.AppendLine($"AND t.Amount BETWEEN @amountMin AND @amountMax");
                        command.Parameters.AddWithValue("@amountMin", searchConfiguration.Amount - 0.001);
                        command.Parameters.AddWithValue("@amountMax", searchConfiguration.Amount + 0.001);
                    }

                    command.CommandText = commandTextBuilder.ToString();
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return await ReadTransactions(reader);
                    }
                }
            }
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(DateTime dateFrom, DateTime dateTo)
        {
            const string commandText =
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id " +
                "WHERE Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return await ReadTransactions(reader);
                    }
                }
            }
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(DateTime dateFrom, DateTime dateTo, string category)
        {
            const string commandText =
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id " +
                "WHERE Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo AND c.Name = @category";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);
                    command.Parameters.AddWithValue("@category", category);
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return await ReadTransactions(reader);
                    }
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
            return transactions;
        }
        #endregion

        #region recurring
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
                "WHERE t.Deleted = 0 AND t.Recurring = 1 AND t.Date IS NULL";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return await ReadTransactions(reader);
                    }
                }
            }
        }

        public async Task<Balance> GetRecurringTemplatesBalance()
        {
            var amounts = new List<double>();
            const string commandText =
                "SELECT t.Amount " +
                "FROM Transactions t " +
                "WHERE t.Deleted = 0 AND t.Recurring = 1 AND t.Date IS NULL";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return default(Balance);
                        }
                        while (reader.Read())
                        {
                            amounts.Add(reader.GetDouble(0));
                        }
                    }
                }
            }

            return CalculateBalance(amounts);
        }

        public async Task<IEnumerable<Transaction>> GetRecurringTransactions(DateTime dateFrom, DateTime dateTo)
        {
            const string commandText =
                "SELECT t.Id,t.[Date],c.Name,t.Amount,t.Comments,t.CreatedTime,t.CreatedAccount,t.LastUpdateTime,t.LastUpdateAccount,t.Deleted,t.Recurring " +
                "FROM Transactions t LEFT JOIN Categories c ON t.Category = c.Id " +
                "WHERE Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo AND Recurring = 1";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return await ReadTransactions(reader);
                    }
                }
            }
        }

        public async Task<Balance> GetRecurringTransactionsBalance(DateTime dateFrom, DateTime dateTo)
        {
            var amounts = new List<double>();
            const string commandText =
                "SELECT t.Amount " +
                "FROM Transactions t " +
                "WHERE Deleted = 0 AND t.Date >= @dateFrom AND t.Date <= @dateTo AND Recurring = 1";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return default(Balance);
                        }
                        while (reader.Read())
                        {
                            amounts.Add(reader.GetDouble(0));
                        }
                    }
                }
            }

            return CalculateBalance(amounts);
        }

        public async Task ApplyRecurringTemplates(DateTime date)
        {
            var templates = await GetRecurringTemplates();
            foreach (var template in templates)
            {
                await AddTransaction(date, template.Category, template.Amount, template.Comments, true);
            }
        }
        #endregion

        #region balances
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

                    command.Connection = dbConnection;
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return default(Balance);
                        }
                        while (reader.Read())
                        {
                            amounts.Add(reader.GetDouble(0));
                        }
                    }
                }
            }
            return CalculateBalance(amounts);
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

                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
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
                                amounts[category] = new List<double> {amount};
                            }
                        }
                    }
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

                    command.Connection = dbConnection;
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return default(Balance);
                        }

                        while (reader.Read())
                        {
                            amounts.Add(reader.GetDouble(0));
                        }
                    }
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
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    command.Parameters.AddWithValue("@dateTo", dateTo);
                    command.Parameters.AddWithValue("@category", category);

                    command.Connection = dbConnection;
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return default(Balance);
                        }
                        while (reader.Read())
                        {
                            amounts.Add(reader.GetDouble(0));
                        }
                    }
                }
            }
            return CalculateBalance(amounts);
        }

        private Balance CalculateBalance(List<double> amounts)
        {
            return new Balance(amounts.Where(x => x > 0).Sum(), amounts.Where(x => x < 0).Sum());
        }
        #endregion

        #region categories

        public IEnumerable<string> GetCategories()
        {
            return _categories?.SelectMany(x => x.Value).Distinct();
        }

        public IEnumerable<string> GetAreas()
        {
            return _categories?.Select(x => x.Key);
        }

        public async Task<Dictionary<string,List<string>>> GetAreasAndCategories()
        {
            var dictionary = new Dictionary<string, List<string>>();
            const string commandText =
                "SELECT a.Name AS Area, c.Name AS Category FROM Categories c JOIN Areas a on c.Area = a.Id ORDER BY a.Name, c.Name";
            var command = new SQLiteCommand() { CommandText = commandText };
            using (var dbConnection = GetConnection())
            {
                command.Connection = dbConnection;
                dbConnection.Open();
                using (var reader = await command.ExecuteReaderAsync())
                { 
                    if (!reader.HasRows)
                        return dictionary;
                    while (reader.Read())
                    {
                        var area = reader.GetString(0);
                        var category = reader.GetString(1);
                        if (dictionary.ContainsKey(area))
                            dictionary[area].Add(category);
                        else
                            dictionary[area] = new List<string> {category};
                    }
                }
            }
            return dictionary;
        }

        public async Task AddArea(string area)
        {
            const string commandText =
                "INSERT INTO Areas (Id,Name) VALUES (@id,@name);";
            using (var dbConnection = GetConnection())
            {
                using (var dbCommand = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    dbCommand.Parameters.AddWithValue("@id", Guid.NewGuid());
                    dbCommand.Parameters.AddWithValue("@name", area);
                    dbConnection.Open();
                    await dbCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task AddCategory(string area, string category)
        {
            const string commandText =
                "INSERT INTO Categories (Id,Area,Name) " +
                "VALUES (@id,(SELECT Id FROM Areas WHERE Name = @area), @category);";
            using (var dbConnection = GetConnection())
            {
                using (var dbCommand = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    dbCommand.Parameters.AddWithValue("@id", Guid.NewGuid());
                    dbCommand.Parameters.AddWithValue("@area", area);
                    dbCommand.Parameters.AddWithValue("@category", category);
                    dbConnection.Open();
                    await dbCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task EditCategory(string oldCategory, string newArea, string newCategory)
        {
            const string commandText =
                "UPDATE Categories " +
                "SET Name = @newName, Area = (SELECT Id FROM Areas WHERE Name = @newArea) " +
                "WHERE Name = @oldName;";
            using (var dbConnection = GetConnection())
            {
                using (var dbCommand = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    dbCommand.Parameters.AddWithValue("@oldName", oldCategory);
                    dbCommand.Parameters.AddWithValue("@newArea", newArea);
                    dbCommand.Parameters.AddWithValue("@newName", newCategory);
                    dbConnection.Open();
                    await dbCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public string GetAreaForCategory(string category)
        {
            return _categories.Keys.FirstOrDefault(x => _categories[x].Contains(category));
        }
        
        private void SeedCategories(Dictionary<string, List<string>> categories)
        {
            const string insertAreaCommand = "INSERT INTO Areas(Id,Name) VALUES(@area{0}Id,'{1}'); ";
            const string insertCategoryCommand = "INSERT INTO Categories(Id,Name,Area) VALUES(@category{0}Id,'{1}',(SELECT Id FROM Areas WHERE Name = '{2}')); ";

            StringBuilder commandText = new StringBuilder();
            using (var dbConnection = new SQLiteConnection(_dbConnectionString))
            {
                using (var command = new SQLiteCommand(dbConnection))
                {
                    foreach (var area in categories.Keys)
                    {
                        var adaptedArea = area.Replace(' ', '_').Replace("&",""); // adapt name for usage as parameter name
                        commandText.AppendLine(string.Format(insertAreaCommand,adaptedArea, area));
                        command.Parameters.AddWithValue($"@area{adaptedArea}Id", Guid.NewGuid());
                        foreach (var category in categories[area])
                        {
                            var adaptedCategory = category.Replace(' ', '_').Replace("&", ""); // adapt name for usage as parameter name
                            commandText.AppendLine(string.Format(insertCategoryCommand,adaptedCategory, category, area));
                            command.Parameters.AddWithValue($"@category{adaptedCategory}Id", Guid.NewGuid());
                        }
                    }

                    command.CommandText = commandText.ToString();

                    dbConnection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        #endregion

        //private void Log(string message)
        //{
        //    Debug.WriteLine($"DBTRACE|{DateTime.Now:hh:mm:ss.fff}|{message}");
        //}

    }
}
