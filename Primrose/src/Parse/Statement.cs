using Primrose.src.Tokenize;

namespace Primrose.src.Parse;

public sealed class SqlAst {
    public readonly List<Statement> Program = [];
}

public abstract class Statement {}

public sealed class CreateTableStatement : Statement {
    public required string TableName { get; set; }
    public required List<ColumnDefinition> Columns { get; set; }
}

public sealed class FunctionStatement : Statement {
    public required string Function { get; set; }
}

public sealed class WhereClause : Statement {
    public required Statement Condition { get; set; }
}

public sealed class CreateDatabaseStatement : Statement {
    public required string DatabaseName { get; set; }
}

public sealed class DropDatabaseStatement : Statement {
    public required string DatabaseName { get; set; }
}

public sealed class UseDatabaseStatement : Statement {
    public required string DatabaseName { get; set; }
}
public sealed class InsertIntoStatement : Statement {
    public required string TableName { get; set; }
    public required List<string> ColumnNames { get; set; }
    public required List<InsertValues> ValuesList { get; set; }
}

public sealed class SelectStatement : Statement {
    public required Statement Item { get; set; }
    public required List<string> Columns { get; set; }
}

public sealed class DeleteStatement : Statement {
    public required FromClause From { get; set; }
    public required Statement? Condition { get; set; }
}

public sealed class LogoutUserStatement : Statement { }

public sealed class FromClause : Statement {
    public required string TableName { get; set; }
}

public sealed class GrantStatement : Statement {
    public required List<string> Privileges { get; set; }
    public required string Database { get; set; }
    public required string TableName { get; set; }
    public required string ToUser { get; set; }
}

public sealed class IdentifierExpression : Statement {
    public required string Value { get; set; }
}

public sealed class NumericExpression : Statement {
    public required int Value { get; set; }
}

public sealed class StringExpression : Statement {
    public required string Value { get; set; }
}

public sealed class BoolExpression : Statement {
    public required bool Value { get; set; }
}

public sealed class BinaryExpression : Statement {
    public required Statement Left { get; set; }
    public required Token Op { get; set; }
    public required Statement Right { get; set; }
}

public sealed class UnaryExpression : Statement {
    public required Statement Right { get; set; }
    public required Token Op { get; set; }
}

public sealed class CreateUserStatement : Statement {
    public required string Name { get; set; }
    public required string Password { get; set; }
}

public sealed class LoginUserStatement : Statement {
    public required string Name { get; set; }
    public required string Password { get; set; }
}

public sealed class ColumnDefinition {
    public required string ColumnName { get; set; }
    public required SqlType Type { get; set; }
    public required bool CanContainNull { get; set; }
    public required bool IsUnique { get; set; }
}

public sealed class InsertValues {
    public required List<Token> Values { get; set; }
}

public sealed class DropTableStatement : Statement {
    public required string TableName { get; set; }
}

public sealed class DropUserStatement : Statement {
    public required string UserName { get; set; }
}

public sealed class BadStatement : Statement {
    public required string Error { get; set; }
}