using Primrose.src.Tokenize;

namespace Primrose.src.Parse;

internal sealed class SqlAst {
    public readonly List<Statement> Program = [];
}

internal abstract class Statement {}

internal sealed class CreateTableStatement : Statement {
    public required string TableName { get; set; }
    public required List<ColumnDefinition> Columns { get; set; }
}

internal sealed class CreateDatabaseStatement : Statement {
    public required string DatabaseName { get; set; }
}

internal sealed class DropDatabaseStatement : Statement {
    public required string DatabaseName { get; set; }
}

internal sealed class UseDatabaseStatement : Statement {
    public required string DatabaseName { get; set; }
}

internal sealed class InsertIntoStatement : Statement {
    public required string TableName { get; set; }
    public required List<string> ColumnNames { get; set; }
    public required List<InsertValues> ValuesList { get; set; }
}

internal sealed class SelectStatement : Statement {
    public required string TableName { get; set; }
    public required List<string> Columns { get; set; }
}

internal sealed class ColumnDefinition {
    public required string ColumnName { get; set; }
    public required SqlType Type { get; set; }
    public required bool CanContainNull { get; set; }
    public required bool IsUnique { get; set; }
}

internal sealed class InsertValues {
    public required List<Token> Values { get; set; }
}

internal sealed class DropTableStatement : Statement {
    public required string TableName { get; set; }
}

internal sealed class BadStatement : Statement {
    public required string Error { get; set; }
}