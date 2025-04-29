using Primrose.src.Parse;
using Primrose.src.Tokenize;

namespace Primrose.src.Sql;

internal sealed class SqlEngine {
    private readonly EngineController controller = new();
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
            _ => controller.UnknownQuery()
        };
    }

    private QueryResult ExecCreateTable(CreateTableStatement createTable) {
        var err = controller.CheckDatabase();
        if (!err.IsSuccess) return err;

        var existingTable = controller.GetTable(createTable.TableName);
        if (existingTable is not null) {
            return QueryResult.Err($"Table '{createTable.TableName}' already exists.");
        }

        var table = new SqlTable() {
            Name = createTable.TableName,
            Columns = createTable.Columns,
            Rows = []
        };

        controller.CreateTable(table);

        return QueryResult.Ok();
    }

    private QueryResult ExecDropTable(DropTableStatement dropTable) {
        var err = controller.CheckDatabase();
        if (!err.IsSuccess) return err;

        var table = controller.GetTable(dropTable.TableName);
        if (table is null) {
            return QueryResult.Err($"Table '{dropTable.TableName}' not found.");
        }

        controller.DropTable(table);

        return QueryResult.Ok();
    }

    private QueryResult ExecUseDatabase(UseDatabaseStatement useDatabase) {
        var db = controller.GetDatabase(useDatabase.DatabaseName);
        if (db is null) {
            return QueryResult.Err($"Database '{useDatabase.DatabaseName}' not found.");
        }
        
        controller.Database = db;

        return QueryResult.Ok();
    }

    private QueryResult ExecCreateDatabase(CreateDatabaseStatement createDatabase) {
        var existingDb = controller.GetDatabase(createDatabase.DatabaseName);
        if (existingDb is not null) {
            return QueryResult.Err($"Database '{createDatabase.DatabaseName}' already exists.");
        }

        var db = new SqlDatabase() {
            Name = createDatabase.DatabaseName,
            Tables = []
        };

        controller.CreateDatabase(db);

        return QueryResult.Ok();
    }

    private QueryResult ExecDropDatabase(DropDatabaseStatement createDatabase) {
        var db = controller.GetDatabase(createDatabase.DatabaseName);
        if (db is null) {
            return QueryResult.Err($"Database '{createDatabase.DatabaseName}' not found.");
        }

        controller.DropDatabase(db);

        return QueryResult.Ok();
    }
}