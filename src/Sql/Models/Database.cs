namespace Primrose.src.Sql;

internal sealed class SqlDatabase {
    public required string Name { get; set; }
    public required List<SqlTable> Tables { get; set; }
}