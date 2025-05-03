using System.ComponentModel.DataAnnotations.Schema;
using System.Security;
using Primrose.src.Parse;
using Primrose.src.Sql.Models;
using Primrose.src.Tokenize;
using Primrose.src.Utils;

namespace Primrose.src.Sql;

public sealed class SqlEngine {
    public readonly SqlEngineController controller;
    private readonly bool IsDebug;

    public SqlEngine(bool debug) {
        controller = new();
        IsDebug = debug;
    }

    public void Setup() {
        var initQuery = File.ReadAllText("C:\\Users\\rxgqq\\projects\\primrose\\Primrose\\primrose\\init.sql");
        ExecuteQuery(initQuery);
        DisableBootstrap();
    }

    public bool Login(string name, string pass) {
        return controller.Login(name, pass);
    }

    public SqlUser? GetUser() {
        return controller.User;
    }

    public void DisableBootstrap() {
        controller.DisableBootstrap();
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

            _ when stmt is CreateDatabaseStatement x => ExecCreateDatabase(x),
            _ when stmt is DropDatabaseStatement x => ExecDropDatabase(x),
            _ when stmt is UseDatabaseStatement x => ExecUseDatabase(x),

            _ when stmt is SelectStatement x => ExecSelect(x),
            _ when stmt is InsertIntoStatement x => ExecInsertInto(x),
            
            _ when stmt is CreateUserStatement x => ExecCreateUser(x),
            _ when stmt is DropUserStatement x => ExecDropUser(x),

            _ when stmt is DeleteStatement x => ExecDelete(x),

            _ when stmt is LoginUserStatement x => ExecLoginUser(x),
            
            _ when stmt is GrantStatement x => ExecGrant(x),
            _ => controller.UnknownQuery()
        };
    }

    private QueryResult ExecDelete(DeleteStatement delete) {
        var err = controller.CheckDatabase();
        if (!err.IsSuccess) return err;

        var table = controller.GetTable(delete.From.TableName);
        if (table is null) return controller.TableNotFound(delete.From.TableName);
        
        table!.Rows = [];

        return QueryResult.Ok();
    }

    private QueryResult ExecLoginUser(LoginUserStatement loginUser) {
        var user = controller.GetUser(loginUser.Name);
        if (user is null) return controller.UserNotFound(loginUser.Name);

        Login(loginUser.Name, loginUser.Password);
        return QueryResult.Ok();
    }

    private QueryResult ExecGrant(GrantStatement grant) {
        var err = controller.CheckDatabase();
        if (!err.IsSuccess) return err;

        var table = controller.GetTable(grant.TableName);
        if (table is null && grant.TableName != "*") return controller.TableNotFound(grant.TableName);

        var user = controller.GetUser(grant.ToUser);
        if (user is null) return controller.UserNotFound(grant.ToUser);

        var hasGrant = controller.HasNonObjectGrant(
            controller.User!.Name,
            SqlPrivilege.Grant
        );
        if (!hasGrant) return controller.PermissionDenied();

        List<SqlGrant> grants = [];
        foreach (var privilege in grant.Privileges) {
            var sqlPrivilege = SqlMapper.MapStringToPrivilege(privilege);

            var sqlGrant = new SqlGrant() {
                Privilege = sqlPrivilege,
                Database = grant.Database,
                Table = grant.TableName,
                ToUser = grant.ToUser
            };

            grants.Add(sqlGrant);
        }

        controller.Grants[grant.ToUser] = [];
        controller.Grants[grant.ToUser].AddRange(grants);

        return QueryResult.Ok();
    }

    private QueryResult ExecCreateUser(CreateUserStatement createUser) {
        var existingUser = controller.GetUser(createUser.Name);
        if (existingUser is not null) return controller.UserAlreadyExists(createUser.Name);

        var hasGrant = controller.HasNonObjectGrant(
            controller.User?.Name ?? "",
            SqlPrivilege.CreateUser
        );
        if (!hasGrant) return controller.PermissionDenied();

        var user = new SqlUser() {
            Name = createUser.Name,
            Password = createUser.Password
        };

        controller.CreateUser(user);

        return QueryResult.Ok();
    }

    private void PrintTable(SqlTable table) {
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
                Console.Write($" {(value ?? SqlConstants.Null).PadRight(columnWidths[j])} |");
            }
            Console.WriteLine();
        }

        PrintSeparator();
    }

    private QueryResult ExecSelect(SelectStatement select) {
        var err = controller.CheckDatabase();
        if (!err.IsSuccess) return err;

        if (select.Item is FunctionStatement func) {
            var table = controller.MapSqlTableFunction(func.Function);
            if (table is null) return controller.UnknownFunction(func.Function);

            PrintTable(table);
            return QueryResult.Ok();
        }
        else if (select.Item is FromClause from) {
            var table = controller.GetTable(from.TableName);
            if (table is null) return controller.TableNotFound(from.TableName);

            var hasGrant = controller.HasGrant(
                controller.User!.Name,
                controller.Database!.Name,
                from.TableName,
                SqlPrivilege.Select
            );
            if (!hasGrant) return controller.PermissionDenied();

            PrintTable(table);
            return QueryResult.Ok();
        }

        return QueryResult.Err("Invalid target for select.");
    }

    private QueryResult ExecInsertInto(InsertIntoStatement insertInto) {
        var err = controller.CheckDatabase();
        if (!err.IsSuccess) return err;

        var table = controller.GetTable(insertInto.TableName);
        if (table is null) return controller.TableNotFound(insertInto.TableName);

        for (int i = 0; i < insertInto.ValuesList.Count; i++) {
            var valueList = insertInto.ValuesList[i];

            if (valueList.Values.Count != insertInto.ColumnNames.Count) {
                return QueryResult.Err("Supplied arguments in values does not match declared columns.");
            }
        }

        var hasGrant = controller.HasGrant(
            controller.User!.Name,
            controller.Database!.Name,
            insertInto.TableName,
            SqlPrivilege.Insert
        );
        if (!hasGrant) return controller.PermissionDenied();

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

        var hasGrant = controller.HasGrant(
            controller.User!.Name,
            controller.Database!.Name,
            createTable.TableName,
            SqlPrivilege.Create
        );
        if (!hasGrant) return controller.PermissionDenied();

        var columnSet = new HashSet<string>();
        foreach (var column in createTable.Columns) {
            if (!columnSet.Add(column.ColumnName)) {
                return QueryResult.Err("Duplicate column names in table.");
            }
        }

        var table = new SqlTable() {
            Name = createTable.TableName,
            Columns = createTable.Columns,
            IsSystemTable = false,
            Rows = []
        };

        controller.Database!.Tables.Add(table);

        return QueryResult.Ok();
    }

    private QueryResult ExecDropUser(DropUserStatement dropUser) {
        var user = controller.GetUser(dropUser.UserName);
        if (user is null) return controller.UserNotFound(dropUser.UserName);

        var hasGrant = controller.HasNonObjectGrant(
            controller.User!.Name,
            SqlPrivilege.Drop
        );
        if (!hasGrant) return controller.PermissionDenied();

        controller.DeleteUser(user);

        return QueryResult.Ok();
    }

    private QueryResult ExecDropTable(DropTableStatement dropTable) {
        var err = controller.CheckDatabase();
        if (!err.IsSuccess) return err;

        var table = controller.GetTable(dropTable.TableName);
        if (table is null) return controller.TableNotFound(dropTable.TableName);

        var hasGrant = controller.HasGrant(
            controller.User!.Name,
            controller.Database!.Name,
            dropTable.TableName,
            SqlPrivilege.Drop
        );
        if (!hasGrant) return controller.PermissionDenied();

        controller.Database!.Tables.Remove(table);

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

        var hasGrant = controller.HasNonObjectGrant(
            controller.User!.Name,
            SqlPrivilege.Create
        );
        if (!hasGrant) return controller.PermissionDenied();

        var db = new SqlDatabase() {
            Name = createDatabase.DatabaseName,
            Tables = []
        };

        controller.Databases.Add(db);

        return QueryResult.Ok();
    }

    private QueryResult ExecDropDatabase(DropDatabaseStatement createDatabase) {
        var db = controller.GetDatabase(createDatabase.DatabaseName);
        if (db is null) return controller.DatabaseNotFound(createDatabase.DatabaseName);

        var hasGrant = controller.HasNonObjectGrant(
            controller.User!.Name,
            SqlPrivilege.Drop
        );
        if (!hasGrant) return controller.PermissionDenied();

        controller.Databases.Remove(db);
        if (db == controller.Database) controller.Database = null;

        return QueryResult.Ok();
    }
}