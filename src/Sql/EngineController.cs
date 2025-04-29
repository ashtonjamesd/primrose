namespace Primrose.src.Sql;

internal class EngineController {
    public SqlDatabase? Database;
    public List<SqlDatabase> Databases = [];

    public SqlTable? GetTable(string name) {
        var table = Database!.Tables
            .FirstOrDefault(x => x.Name == name);

        return table;
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
}