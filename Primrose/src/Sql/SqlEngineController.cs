using Primrose.src.Auth;
using Primrose.src.Parse;
using Primrose.src.Sql.Models;

namespace Primrose.src.Sql;

public class SqlEngineController {
    public SqlDatabase? Database;
    public SqlUser? User;

    // this flag allows the 'init.sql' to execute despite not having a logged in user
    // it bypasses all grants and permission constraints when executing query
    //
    // this property should never be set to true again after 'DisableBootstrap' is called
    private bool allowBootstrap = true;

    public void DisableBootstrap() {
        allowBootstrap = false;
    }

    public readonly List<SqlDatabase> Databases = [];
    public readonly Dictionary<string, List<SqlGrant>> Grants = [];
    private readonly AuthService auth = new();

    public SqlTable? MapSqlTableFunction(string function) {
        if (function is "current_database") {
            return new SqlTable() {
                IsSystemTable = false,
                Name = "current_database",
                Columns = [
                    new ColumnDefinition() {
                        ColumnName = "Name",
                        Type = new SqlVarchar() { MaxChars = SqlConstants.VarcharMax },
                        CanContainNull = true,
                        IsUnique = true
                    }
                ],
                Rows = [
                    new Dictionary<string, object>() {
                        ["Name"] = Database?.Name ?? SqlConstants.Null
                    }
                ]
            };
        }

        return null;
    }

    public bool HasGrant(string user, string database, string table, SqlPrivilege privilege) {
        if (allowBootstrap) return true;
        if (!Grants.TryGetValue(user, out List<SqlGrant>? value)) return false;

        var hasGrant = value.Any(x => {
            return (x.Database == database || x.Database is "*") &&
                (x.Table == table || x.Table is "*") &&
                (x.Privilege == privilege || x.Privilege == SqlPrivilege.All);
        });

        return hasGrant;
    }

    public bool HasNonObjectGrant(string user, SqlPrivilege privilege) {
        if (allowBootstrap) return true;

        if (!Grants.TryGetValue(user, out List<SqlGrant>? value)) return false;

        var hasGrant = value.Any(x => {
            return x.Privilege == privilege || x.Privilege == SqlPrivilege.All;
        });

        return hasGrant;
    }

    public bool Login(string name, string pass) {
        var user = auth.Login(name, pass);
        if (user is null) return false;

        User = user;

        return true;
    }

    public SqlUser? GetUser(string name) {
        return auth.GetUser(name);
    }

    public void CreateUser(SqlUser user) {
        auth.Users.Add(user);
    }

    public void DeleteUser(SqlUser user) {
        auth.Users.Remove(user);
    }

    public SqlTable? GetTable(string name) {
        var table = Database!.Tables
            .FirstOrDefault(x => x.Name == name);

        return table;
    }

    public ColumnDefinition? GetColumn(SqlTable table, string columnName) {
        var column = table.Columns
            .FirstOrDefault(x => x.ColumnName == columnName);

        return column;
    }

    public SqlDatabase? GetDatabase(string name) {
        var db = Databases
            .FirstOrDefault(x => x.Name == name);

        return db;
    }

    public QueryResult TableNotFound(string table) {
        return QueryResult.Err($"Table '{table}' not found.");
    }

    public QueryResult TableAlreadyExists(string table) {
        return QueryResult.Err($"Table '{table}' already exists.");
    }

    public QueryResult DatabaseAlreadyExists(string database) {
        return QueryResult.Err($"Database '{database}' already exists.");
    }

    public QueryResult DatabaseNotFound(string database) {
        return QueryResult.Err($"Database '{database}' not found.");
    }

    public QueryResult CheckDatabase() {
        if (Database is null) {
            return QueryResult.Err("A database target is required.");
        }
        
        return QueryResult.Ok();
    }

    public QueryResult UnknownQuery() {
        return QueryResult.Err("Unknown query attempted to execute.");
    }

    public QueryResult UserAlreadyExists(string name) {
        return QueryResult.Err($"User '{name}' already exists.");
    }

    public QueryResult UserNotFound(string name) {
        return QueryResult.Err($"User '{name}' not found.");
    }

    public QueryResult PermissionDenied() {
        return QueryResult.Err("You do not have permission to perform this operation.");
    }

    public QueryResult UnknownFunction(string func) {
        return QueryResult.Err($"Unknown function '{func}'");
    }
    
    public QueryResult InvalidTypeInsertion(string column) {
        return QueryResult.Err($"Invalid type insertion for '{column}'.");
    }

    public QueryResult ColumnNotFound(string column) {
        return QueryResult.Err($"Column '{column}' does not exist.");
    }
}