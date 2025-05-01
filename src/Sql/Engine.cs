
using Primrose.src.Parse;
using Primrose.src.Sql.Models;
using Primrose.src.Tokenize;
using Primrose.src.Utils;

namespace Primrose.src.Sql;

internal sealed class SqlEngine {
    private readonly EngineController controller = new();
    private readonly bool IsDebug;

    public SqlEngine(bool debug) {
        IsDebug = debug;
    }

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
            _ when stmt is InsertIntoStatement x => ExecInsertInto(x),
            _ when stmt is SelectClause x => ExecSelect(x),
            _ when stmt is CreateUserStatement x => ExecCreateUser(x),
            _ => controller.UnknownQuery()
        };
    }

    private QueryResult ExecCreateUser(CreateUserStatement createUser) {
        var existingUser = controller.GetUser(createUser.Name);
        if (existingUser is not null) return controller.UserAlreadyExists(createUser.Name);

        var user = new SqlUser() {
            Name = createUser.Name,
            Password = createUser.Password
        };

        controller.Users.Add(user);

        return QueryResult.Ok();
    }

    private QueryResult ExecSelect(SelectClause select) {
        var err = controller.CheckDatabase();
        if (!err.IsSuccess) return err;

        var table = controller.GetTable(select.TableName);
        if (table is null) return controller.TableNotFound(select.TableName);

        const string leftWhitespaceGap = "";
        var columnWidths = table.Columns.Select((col, index) => {
            if (table.Rows.Count is 0) return col.ColumnName.Length;

            int maxCellLength = table.Rows
                .Select(row => row.Values.ElementAtOrDefault(index)?.ToString()?.Length ?? 4) // 'null' length = 4
                .Max();
            return Math.Max(col.ColumnName.Length, maxCellLength);
        }).ToList();

        void PrintSeparator() {
            Console.Write($"{leftWhitespaceGap}+");
            foreach (var width in columnWidths) {
                Console.Write(new string('-', width + 2));
                Console.Write("+");
            }
            Console.WriteLine();
        }

        Console.WriteLine();
        PrintSeparator();

        Console.Write($"{leftWhitespaceGap}|");
        for (int i = 0; i < table.Columns.Count; i++) {
            Console.Write($" {table.Columns[i].ColumnName.PadRight(columnWidths[i])} |");
        }
        Console.WriteLine();
        PrintSeparator();

        for (int i = 0; i < table.Rows.Count; i++) {
            var row = table.Rows[i];

            Console.Write($"{leftWhitespaceGap}|");
            for (int j = 0; j < table.Columns.Count; j++) {
                var column = table.Columns[j];

                var value = row[column.ColumnName]?.ToString();
                Console.Write($" {(value ?? "null").PadRight(columnWidths[j])} |");
            }
            Console.WriteLine();
        }

        PrintSeparator();
        return QueryResult.Ok();
    }

    private QueryResult ExecInsertInto(InsertIntoStatement insertInto) {
        var err = controller.CheckDatabase();
        if (!err.IsSuccess) return err;

        for (int i = 0; i < insertInto.ValuesList.Count; i++) {
            var valueList = insertInto.ValuesList[i];

            if (valueList.Values.Count != insertInto.ColumnNames.Count) {
                return QueryResult.Err("Supplied arguments in values does not match declared columns.");
            }
        }

        var table = controller.GetTable(insertInto.TableName);
        if (table is null) return controller.TableNotFound(insertInto.TableName);

        foreach (var column in insertInto.ColumnNames) {
            var foundColumn = table.Columns
                .FirstOrDefault(x => x.ColumnName == column);

            if (foundColumn is null) {
                return QueryResult.Err($"Column '{column}' does not exist.");
            }
        }

        foreach (var valueList in insertInto.ValuesList) {
            var row = new Dictionary<string, object?>();

            for (int i = 0; i < valueList.Values.Count; i++) {
                var column = controller.GetColumn(table, insertInto.ColumnNames[i]);
                var value = valueList.Values[i];

                object? insertedValue;
                if (value.Type is TokenType.Null) {
                    if (!column!.CanContainNull) {
                        return QueryResult.Err($"Not null constraint violation: Column '{column.ColumnName}' cannot contain NULL values.");
                    }
                    insertedValue = null;
                } 
                else {
                    if (!SqlTypeHelper.IsTokenTypeMatch(column!.Type, value)) {
                        return QueryResult.Err($"Invalid type insertion for '{column.ColumnName}'.");
                    }
                    insertedValue = value.Lexeme;
                }
                
                if (column.IsUnique && insertedValue != null) {
                    bool conflict = table.Rows.Any(existingRow => 
                        existingRow.TryGetValue(column.ColumnName, out var existingValue) && existingValue?.Equals(insertedValue) is true
                    );

                    if (conflict) {
                        return QueryResult.Err($"Unique constraint violation: value '{insertedValue}' already exists for column '{column.ColumnName}'.");
                    }
                }

                row[column.ColumnName] = insertedValue;
            }

            foreach (var tableColumn in table.Columns) {
                if (!row.ContainsKey(tableColumn.ColumnName)) {
                    if (!tableColumn.CanContainNull) {
                        return QueryResult.Err($"Not null constraint violation: Column '{tableColumn.ColumnName}' cannot contain NULL values.");
                    }
                    row[tableColumn.ColumnName] = null;
                }
            }

            table.Rows.Add(row);
        }

        return QueryResult.Ok();
    }

    private QueryResult ExecCreateTable(CreateTableStatement createTable) {
        var err = controller.CheckDatabase();
        if (!err.IsSuccess) return err;

        var existingTable = controller.GetTable(createTable.TableName);
        if (existingTable is not null) return controller.TableAlreadyExists(createTable.TableName);

        var columnSet = new HashSet<string>();
        foreach (var column in createTable.Columns) {
            if (!columnSet.Add(column.ColumnName)) {
                return QueryResult.Err("Duplicate column names in table.");
            }
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
        if (table is null) return controller.TableNotFound(dropTable.TableName);

        controller.DropTable(table);

        return QueryResult.Ok();
    }

    private QueryResult ExecUseDatabase(UseDatabaseStatement useDatabase) {
        var db = controller.GetDatabase(useDatabase.DatabaseName);
        if (db is null) return controller.DatabaseNotFound(useDatabase.DatabaseName);
        
        controller.Database = db;

        return QueryResult.Ok();
    }

    private QueryResult ExecCreateDatabase(CreateDatabaseStatement createDatabase) {
        var existingDb = controller.GetDatabase(createDatabase.DatabaseName);
        if (existingDb is not null) return controller.DatabaseAlreadyExists(createDatabase.DatabaseName);

        var db = new SqlDatabase() {
            Name = createDatabase.DatabaseName,
            Tables = []
        };

        controller.CreateDatabase(db);

        return QueryResult.Ok();
    }

    private QueryResult ExecDropDatabase(DropDatabaseStatement createDatabase) {
        var db = controller.GetDatabase(createDatabase.DatabaseName);
        if (db is null) return controller.DatabaseNotFound(createDatabase.DatabaseName);

        controller.DropDatabase(db);

        return QueryResult.Ok();
    }
}