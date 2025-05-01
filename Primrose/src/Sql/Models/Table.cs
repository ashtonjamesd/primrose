using Primrose.src.Parse;

namespace Primrose.src.Sql.Models;

public sealed class SqlTable {
    public required string Name { get; set; }
    public required List<ColumnDefinition> Columns { get; set; }
    public required List<Dictionary<string, object?>> Rows { get; set; }
}