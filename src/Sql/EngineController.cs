using System.ComponentModel.DataAnnotations.Schema;
using Primrose.src.Parse;
using Primrose.src.Sql.Models;

namespace Primrose.src.Sql;

internal class EngineController {
    public SqlDatabase? Database;
    public List<SqlDatabase> Databases = [];
    public List<SqlUser> Users = [];

    public SqlUser? GetUser(string name) {
        var user = Users
            .FirstOrDefault(x => x.Name == name);

        return user;
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

    public void CreateDatabase(SqlDatabase db) {
        Databases.Add(db);
    }

    public void DropDatabase(SqlDatabase db) {
        Databases.Remove(db);
    }

    public void CreateTable(SqlTable table) {
        Database!.Tables.Add(table);
    }

    public void DropTable(SqlTable table) {
        Database!.Tables.Remove(table);
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
}