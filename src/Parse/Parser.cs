using System.Runtime.CompilerServices;
using Primrose.src.Tokenize;

namespace Primrose.src.Parse;

internal sealed class Parser {
    private readonly List<Token> Tokens;
    private readonly SqlAst Ast = new();

    private int Current;

    public Parser(List<Token> tokens) {
        Tokens = tokens;
    }

    public void Print() {
        Console.WriteLine($"\nCurrent: {Current}");
        Console.WriteLine($"Ast ({Ast.Program.Count}):");

        foreach (var statement in Ast.Program) {
            PrintStatement(statement, 1);
        }
        Console.WriteLine();
    }

    private static void PrintStatement(Statement stmt, int depth) {
        Console.Write(new string(' ', depth * 2));
        
        if (stmt is CreateTableStatement createTable) {
            Console.WriteLine($"create table {createTable.TableName}");
            foreach (var col in createTable.Columns) {
                Console.Write(new string(' ', depth * 2));
                Console.WriteLine($"  {col.ColumnName}: {col.Type}");
            }
        } else if (stmt is DropTableStatement dropTable) {
            Console.WriteLine($"drop table {dropTable.TableName}");
        }
        else if (stmt is CreateDatabaseStatement createDatabase) {
            Console.WriteLine($"create database {createDatabase.DatabaseName}");
        }
        else if (stmt is DropDatabaseStatement dropDatabase) {
            Console.WriteLine($"drop database {dropDatabase.DatabaseName}");
        }
        else if (stmt is UseDatabaseStatement useDatabase) {
            Console.WriteLine($"use database {useDatabase.DatabaseName}");
        }
        else if (stmt is InsertIntoStatement insertInto) {
            Console.WriteLine($"insert into {insertInto.TableName}");
            Console.Write(new string(' ', depth * 3) + "columns: ");
            foreach (var column in insertInto.ColumnNames) {
                Console.Write($"{column}, ");
            }
            Console.WriteLine();
            
            Console.Write(new string(' ', depth * 3) + $"values list ({insertInto.ValuesList.Count}):\n");
            for (int i = 0; i < insertInto.ValuesList.Count; i++) {
                Console.Write(new string(' ', depth * 5) + $"values ({i}): ");

                foreach (var value in insertInto.ValuesList[i].Values) {
                    Console.Write($"{value}, ");
                }
            }
        }
    }

    // insert into x (ca, cb, cc) values (a, b, c)

    public SqlAst CreateAst() {
        while (!IsLast()) {
            var statement = ParseStatement();
            if (statement is BadStatement bad) {
                Console.WriteLine($"{bad.Error}\n");
                return Ast;
            }

            Ast.Program.Add(statement);
            Advance();
        }

        return Ast;
    }

    private Statement ParseStatement() {
        return CurrentToken().Type switch {
            TokenType.Create => ParseCreate(),
            TokenType.Drop => ParseDrop(),
            TokenType.Use => ParseUseDatabase(),
            TokenType.Insert => ParseInsertInto(),
            _ => BadStatement("Invalid SQL statement.")
        };
    }

    private Statement ParseCreate() {
        var next = Peek();

        return next.Type switch {
            TokenType.Table => ParseCreateTable(),
            TokenType.Database => ParseCreateDatabase(),
            _                => BadStatement("Invalid SQL statement.")
        }; 
    }

    private Statement ParseDrop() {
        var next = Peek();

        return next.Type switch {
            TokenType.Table => ParseDropTable(),
            TokenType.Database => ParseDropDatabase(),
            _                => BadStatement("Invalid SQL statement.")
        }; 
    }

    private static BadStatement BadStatement(string error) {
        return new BadStatement() {
            Error = error
        };
    }

    private BadStatement Error() {
        Current--;
        var errorToken = CurrentToken();

        return new() {
            Error = $"Syntax error near '{errorToken.Lexeme}'"
        };
    }

    private Statement ParseDropTable() {
        var isDrop = Expect(TokenType.Drop);
        if (!isDrop) return Error();

        var isTable =  Expect(TokenType.Table);
        if (!isTable) return Error();

        var tableNameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();

        return new DropTableStatement() {
            TableName = tableNameToken.Lexeme
        };
    }

    private Statement ParseDropDatabase() {
        var isDrop = Expect(TokenType.Drop);
        if (!isDrop) return Error();

        var isDatabase =  Expect(TokenType.Database);
        if (!isDatabase) return Error();

        var tableNameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();

        return new DropDatabaseStatement() {
            DatabaseName = tableNameToken.Lexeme
        };
    }

    private Statement ParseCreateTable() {
        var isCreate = Expect(TokenType.Create);
        if (!isCreate) return Error();

        var isTable =  Expect(TokenType.Table);
        if (!isTable) return Error();

        var tableNameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();
        Advance();

        var isLeftParen =  Expect(TokenType.LeftParen);
        if (!isLeftParen) return Error();

        Current--;
        var columns = new List<ColumnDefinition>();
        do {
            Advance();

            var columnName = CurrentToken();
            if (!Match(TokenType.Identifier)) return Error();
            Advance();

            var columnType = CurrentToken();
            if (!IsDataTypeToken(columnType)) return Error();
            Advance();

            var column = new ColumnDefinition() {
                ColumnName = columnName.Lexeme,
                Type = columnType.Lexeme
            };

            columns.Add(column);
        } while (Match(TokenType.Commma));

        var isRightParen =  Expect(TokenType.RightParen);
        if (!isRightParen) return Error();

        Current--;

        return new CreateTableStatement() {
            TableName = tableNameToken.Lexeme,
            Columns = columns
        };
    }

    private Statement ParseInsertInto() {
        var isInsert = Expect(TokenType.Insert);
        if (!isInsert) return Error();

        var isInto = Expect(TokenType.Into);
        if (!isInto) return Error();

        var tableNameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();
        Advance();
        
        var isLeftParen =  Expect(TokenType.LeftParen);
        if (!isLeftParen) return Error();

        List<string> columns = [];

        Current--;
        do {
            Advance();

            var columnName = CurrentToken();
            if (!Match(TokenType.Identifier)) return Error();
            Advance();

            columns.Add(columnName.Lexeme);
        } while (Match(TokenType.Commma));
        
        // Expect(TokenType.RightParen)
        //    .Then(TokenType.Values)
        //    .Then(TokenType.LeftParen)

        var isRightParen =  Expect(TokenType.RightParen);
        if (!isRightParen) return Error();

        var isValues =  Expect(TokenType.Values);
        if (!isValues) return Error();

        var isValuesLeftParen =  Expect(TokenType.LeftParen);
        if (!isValuesLeftParen) return Error();
        
        InsertValues insertValues = new() {
            Values = [],
        };

        Current--;
        do {
            Advance();

            var value = CurrentToken();
            if (!Match(TokenType.Identifier)) return Error();
            Advance();

            insertValues.Values.Add(value.Lexeme);
        } while (Match(TokenType.Commma));

        var isValuesRightParen =  Expect(TokenType.RightParen);
        if (!isValuesRightParen) return Error();

        return new InsertIntoStatement() {
            TableName = tableNameToken.Lexeme,
            ColumnNames = columns,
            ValuesList = [insertValues] // to do
        };
    }

    private Statement ParseCreateDatabase() {
        var isCreate = Expect(TokenType.Create);
        if (!isCreate) return Error();

        var isDatabase =  Expect(TokenType.Database);
        if (!isDatabase) return Error();
        
        var dbNameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();

        return new CreateDatabaseStatement() {
            DatabaseName = dbNameToken.Lexeme
        };
    }

    private Statement ParseUseDatabase() {
        var isUse = Expect(TokenType.Use);
        if (!isUse) return Error();
        
        var dbNameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();

        return new UseDatabaseStatement() {
            DatabaseName = dbNameToken.Lexeme
        };
    }

    private static bool IsDataTypeToken(Token token) {
        if (token.Type is TokenType.Character) return true;
        if (token.Type is TokenType.Integer) return true;
        if (token.Type is TokenType.Varchar) return true;
        if (token.Type is TokenType.Boolean) return true;
        
        return false;
    }

    private bool Match(TokenType type) {
        return CurrentToken().Type == type;
    }

    private Token Peek() {
        Advance();
        var token = CurrentToken();
        Current--;

        return token;
    }

    private bool Expect(TokenType type) {
        if (!IsLast() && CurrentToken().Type == type) {
            Advance();
            return true;
        }

        return false;
    }

    private Token CurrentToken() {
        return !IsLast() ? Tokens[Current] : new() { Lexeme = "", Type = TokenType.Bad };
    }

    private void Advance() {
        Current++;
    }

    private bool IsLast() {
        return Current >= Tokens.Count;
    }
}