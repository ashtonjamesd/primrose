using Primrose.src.Auth;
using Primrose.src.Parse;
using Primrose.src.Sql.Models;

namespace Primrose.src.Sql;

internal class EngineController {
    public SqlDatabase? Db;
    public SqlUser? User;

    public readonly List<SqlDatabase> Databases = [];
    public readonly Dictionary<string, List<SqlGrant>> Grants = [];
    private readonly AuthService auth = new();

    public bool HasGrant(string user, string database, string table, SqlPrivilege privilege) {
        if (!Grants.TryGetValue(user, out List<SqlGrant>? value)) return false;

        var hasGrant = value.Any(x => {
            return x.Database == database &&
                x.Table == table &&
                x.Privilege == privilege;
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
        var table = Db!.Tables
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
        if (Db is null) {
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
}