namespace Primrose.src.Parse;

internal sealed class SqlAst {
    public readonly List<Statement> Program = [];
}

internal abstract class Statement {}

internal sealed class CreateTableStatement : Statement {
    public required string TableName { get; set; }
    public required List<ColumnDefinition> Columns { get; set; }

    public override string ToString() {
        return $"create table {TableName}:\n{string.Join(", ", Columns.Select(x => x.ColumnName))}";
    }
}

internal sealed class CreateDatabaseStatement : Statement {
    public required string DatabaseName { get; set; }

    public override string ToString() {
        return $"create database {DatabaseName}";
    }
}

internal sealed class DropDatabaseStatement : Statement {
    public required string DatabaseName { get; set; }

    public override string ToString() {
        return $"drop database {DatabaseName}";
    }
}

internal sealed class UseDatabaseStatement : Statement {
    public required string DatabaseName { get; set; }

    public override string ToString() {
        return $"use database {DatabaseName}";
    }
}

internal sealed class ColumnDefinition {
    public required string ColumnName { get; set; }
    public required string Type { get; set; }
}

internal sealed class DropTableStatement : Statement {
    public required string TableName { get; set; }
}

internal sealed class BadStatement : Statement {
    public required string Error { get; set; }
}