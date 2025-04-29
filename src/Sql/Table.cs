using Primrose.src.Parse;

namespace Primrose.src.Sql;

internal sealed class SqlTable {
    public required string Name { get; set; }
    public required List<ColumnDefinition> Columns { get; set; }
    public required List<object> Rows { get; set; }
}