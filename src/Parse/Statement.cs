using System.Data;
using System.Numerics;
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

internal sealed class WhereClause : Statement {
    public required Statement Condition { get; set; }
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

internal sealed class SelectClause : Statement {
    public required string TableName { get; set; }
    public required List<string> Columns { get; set; }
}

internal sealed class IdentifierExpression : Statement {
    public required string Value { get; set; }
}

internal sealed class NumericExpression : Statement {
    public required int Value { get; set; }
}

internal sealed class StringExpression : Statement {
    public required string Value { get; set; }
}

internal sealed class BoolExpression : Statement {
    public required bool Value { get; set; }
}

internal sealed class BinaryExpression : Statement {
    public required Statement Left { get; set; }
    public required Token Op { get; set; }
    public required Statement Right { get; set; }
}

internal sealed class UnaryExpression : Statement {
    public required Statement Right { get; set; }
    public required Token Op { get; set; }
}

internal sealed class CreateUserStatement : Statement {
    public required string Name { get; set; }
    public required string Password { get; set; }
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