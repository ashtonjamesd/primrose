using Primrose.src.Parse;
using Primrose.src.Tokenize;

namespace Primrose.src.Sql;

internal sealed class SqlEngine {
    private SqlDatabase? Database { get; set; }
    private readonly List<SqlDatabase> Databases = [];
    
    private readonly bool IsDebug;

    public void ExecuteQuery(string query) {
        var lexer = new Lexer(query);
        var tokens = lexer.Tokenize();

        if (IsDebug) lexer.Print();

        var parser = new Parser(tokens);
        var ast = parser.CreateAst();

        if (IsDebug) parser.Print();

        foreach (var stmt in ast.Program) {
            var result = ExecStatment(stmt);

            if (!result.IsSuccess) {
                Console.WriteLine($"{result.Message}\n");
                break;
            }
        }
    }

    private QueryResult ExecStatment(Statement stmt) {
        return stmt switch {
            _ when stmt is CreateTableStatement x => ExecCreateTable(x),
            _ when stmt is DropTableStatement x => ExecDropTable(x),
            _ when stmt is UseDatabaseStatement x => ExecUseDatabase(x),
            _ when stmt is CreateDatabaseStatement x => ExecCreateDatabase(x),
            _ when stmt is DropDatabaseStatement x => ExecDropDatabase(x),
            _ => UnknownQuery()
        };
    }

    private QueryResult ExecCreateTable(CreateTableStatement createTable) {
        var err = CheckDatabase();
        if (!CheckDatabase().IsSuccess) return err;

        var existingTable = GetTable(createTable.TableName);
        if (existingTable is not null) {
            return QueryResult.Err($"Table '{createTable.TableName}' already exists.");
        }

        var table = new SqlTable() {
            Name = createTable.TableName,
            Columns = createTable.Columns,
            Rows = []
        };

        CreateTable(table);

        return QueryResult.Ok();
    }

    private QueryResult ExecDropTable(DropTableStatement dropTable) {
        var err = CheckDatabase();
        if (!err.IsSuccess) return err;

        var table = GetTable(dropTable.TableName);
        if (table is null) {
            return QueryResult.Err($"Table '{dropTable.TableName}' not found.");
        }

        DropTable(table);

        return QueryResult.Ok();
    }

    private QueryResult ExecUseDatabase(UseDatabaseStatement useDatabase) {
        var db = GetDb(useDatabase.DatabaseName);
        if (db is null) {
            return QueryResult.Err($"Database '{useDatabase.DatabaseName}' not found.");
        }
        
        Database = db;

        return QueryResult.Ok();
    }

    private QueryResult ExecCreateDatabase(CreateDatabaseStatement createDatabase) {
        var existingDb = GetDb(createDatabase.DatabaseName);
        if (existingDb is not null) {
            return QueryResult.Err($"Database '{createDatabase.DatabaseName}' already exists.");
        }

        var db = new SqlDatabase() {
            Name = createDatabase.DatabaseName,
            Tables = []
        };

        CreateDatabase(db);

        return QueryResult.Ok();
    }

    
    private QueryResult ExecDropDatabase(DropDatabaseStatement createDatabase) {
        var db = GetDb(createDatabase.DatabaseName);
        if (db is null) {
            return QueryResult.Err($"Database '{createDatabase.DatabaseName}' not found.");
        }

        DropDatabase(db);

        return QueryResult.Ok();
    }

    private SqlTable? GetTable(string name) {
        var table = Database!.Tables
            .FirstOrDefault(x => x.Name == name);

        return table;
    }

    private SqlDatabase? GetDb(string name) {
        var db = Databases
            .FirstOrDefault(x => x.Name == name);

        return db;
    }

    private void CreateDatabase(SqlDatabase db) {
        Databases.Add(db);
    }

    private void DropDatabase(SqlDatabase db) {
        Databases.Remove(db);
    }

    private void CreateTable(SqlTable table) {
        Database!.Tables.Add(table);
    }

    private void DropTable(SqlTable table) {
        Database!.Tables.Remove(table);
    }

    private QueryResult CheckDatabase() {
        if (Database is null) {
            return QueryResult.Err("A database target is required.");
        }
        
        return QueryResult.Ok();
    }

    private static QueryResult UnknownQuery() {
        return QueryResult.Err("Unknown query attempted to execute.");
    }
}