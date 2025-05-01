using System.Runtime.CompilerServices;
using Primrose.src.Tokenize;
using Primrose.src.Utils;

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
                Console.Write(new string(' ', (depth + 1) * 2));
                Console.WriteLine($"{col.ColumnName}: {col.Type}");
            }
        }
        else if (stmt is DropTableStatement dropTable) {
            Console.WriteLine($"drop table {dropTable.TableName}");
        }
        else if (stmt is DropUserStatement dropUser) {
            Console.WriteLine($"drop user {dropUser.UserName}");
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
            Console.Write(new string(' ', (depth + 1) * 2) + "columns: ");
            Console.WriteLine(string.Join(", ", insertInto.ColumnNames));
            Console.WriteLine(new string(' ', (depth + 1) * 2) + $"values list ({insertInto.ValuesList.Count}):");
            for (int i = 0; i < insertInto.ValuesList.Count; i++) {
                Console.Write(new string(' ', (depth + 2) * 2) + $"values ({i}): ");
                Console.WriteLine(string.Join(", ", insertInto.ValuesList[i].Values.Select(v => v.Lexeme)));
            }
        }
        else if (stmt is SelectClause select) {
            Console.WriteLine("select");
            Console.Write(new string(' ', (depth + 1) * 2) + "columns: ");
            Console.WriteLine(string.Join(", ", select.Columns));
            Console.WriteLine(new string(' ', (depth + 1) * 2) + $"from: {select.TableName}");
        }
        else if (stmt is WhereClause where) {
            Console.WriteLine("where");
            PrintStatement(where.Condition, depth + 1);
        }
        else if (stmt is CreateUserStatement createUser) {
            Console.WriteLine("create user");
            Console.Write(new string(' ', (depth + 1) * 2) + $"user: {createUser.Name}\n");
            Console.WriteLine(new string(' ', (depth + 1) * 2) + $"pass: {createUser.Password}");
        }
        else if (stmt is GrantStatement grant) {
            Console.WriteLine("grant");
            var privs = string.Join(", ", grant.Privileges);
            Console.Write(new string(' ', (depth + 1) * 2) + $"privileges: {privs}\n");
            Console.Write(new string(' ', (depth + 1) * 2) + $"on: {grant.Database}.{grant.TableName}\n");
            Console.Write(new string(' ', (depth + 1) * 2) + $"to: {grant.ToUser}\n");
        }
        else if (stmt is LoginUserStatement loginUser) {
            Console.WriteLine("login user");
            Console.Write(new string(' ', (depth + 1) * 2) + $"user: {loginUser.Name}\n");
            Console.WriteLine(new string(' ', (depth + 1) * 2) + $"pass: {loginUser.Password}");
        }
        else if (stmt is BinaryExpression binary) {
            Console.WriteLine($"BinaryExpression ({binary.Op.Lexeme})");
            Console.Write(new string(' ', (depth + 1) * 2) + "Left:\n");
            PrintStatement(binary.Left, depth + 2);
            Console.Write(new string(' ', (depth + 1) * 2) + "Right:\n");
            PrintStatement(binary.Right, depth + 2);
        }
        else if (stmt is UnaryExpression unary) {
            Console.WriteLine($"UnaryExpression ({unary.Op.Lexeme})");
            Console.Write(new string(' ', (depth + 1) * 2) + "Right:\n");
            PrintStatement(unary.Right, depth + 2);
        }
        else if (stmt is IdentifierExpression ident) {
            Console.WriteLine($"Identifier: {ident.Value}");
        }
        else if (stmt is NumericExpression num) {
            Console.WriteLine($"Number: {num.Value}");
        }
        else if (stmt is StringExpression str) {
            Console.WriteLine($"String: \"{str.Value}\"");
        }
        else if (stmt is BoolExpression b) {
            Console.WriteLine($"Bool: {b.Value}");
        }
        else if (stmt is BadStatement bad) {
            Console.WriteLine($"Bad Statement: {bad.Error}");
        }
        else {
            Console.WriteLine("UNKNOWN STATEMENT IN AST");
        }
    }


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
        var c = CurrentToken();

        return c.Type switch {
            TokenType.Create => ParseCreate(),
            TokenType.Drop => ParseDrop(),
            TokenType.Use => ParseUseDatabase(),
            TokenType.Insert => ParseInsertInto(),
            TokenType.Select => ParseSelect(),
            TokenType.Where => ParseWhere(),
            TokenType.Grant => ParseGrant(),
            TokenType.Login => ParseLogin(),
            _ => UnknownStatement(c.Lexeme)
        };
    }

    private Statement ParseLogin() {
        var isCreate = Expect(TokenType.Login);
        if (!isCreate) return Error();

        var isUser = Expect(TokenType.User);
        if (!isUser) return Error();

        var nameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();
        Advance();

        var isIdentified = Expect(TokenType.Identified);
        if (!isIdentified) return Error();
        
        var isBy = Expect(TokenType.By);
        if (!isBy) return Error();

        var passwordToken = CurrentToken();
        if (!Match(TokenType.String)) return Error();

        return new LoginUserStatement() {
            Name = nameToken.Lexeme,
            Password = passwordToken.Lexeme
        };
    }

    private Statement ParseGrant() {
        var isGrant = Expect(TokenType.Grant);
        if (!isGrant) return Error();

        List<string> privileges = [];
        if (Match(TokenType.All)) {
            Advance();

            var isPrivileges = Expect(TokenType.Privileges);
            if (!isPrivileges) return Error();

            privileges.Add("*");
        }

        Recede();
        do {
            Advance();
            if (Match(TokenType.On)) break;

            var privilege = CurrentToken();
            if (!SqlTypeHelper.IsTokenPrivilegeMatch(privilege)) return Error();

            if (Match(TokenType.Create)) {
                Advance();

                if (Match(TokenType.User)) {
                    var userToken = CurrentToken();
                    privileges.Add($"{privilege.Lexeme} {userToken.Lexeme}");
                    Advance();
                }
            } 
            else {
                privileges.Add(privilege.Lexeme);
                Advance();
            }

        } while (Match(TokenType.Comma));

        var isOn = Expect(TokenType.On);
        if (!isOn) return Error();

        var database = CurrentToken();
        if (!Match(TokenType.Star) && !Match(TokenType.Identifier)) return Error();
        Advance();

        var isDot = Expect(TokenType.Dot);
        if (!isDot) return Error();

        var table = CurrentToken();
        if (!Match(TokenType.Star) && !Match(TokenType.Identifier)) return Error();
        Advance();

        var isTo = Expect(TokenType.To);
        if (!isTo) return Error();

        var userNameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();

        // Advance();

        // if (Match(TokenType.With)) {
        //     var isGrantSecond = Expect(TokenType.Grant);
        //     if (!isGrantSecond) return Error();

        //     var isOption = Expect(TokenType.Option);
        //     if (!isOption) return Error();

        //     Recede();
        // }
        // else {
        //     Recede();
        // }

        return new GrantStatement() {
            Privileges = privileges,
            Database = database.Lexeme,
            TableName = table.Lexeme,
            ToUser = userNameToken.Lexeme
        };
    }

    private Statement ParseCreate() {
        var next = Peek();

        return next.Type switch {
            TokenType.Table => ParseCreateTable(),
            TokenType.Database => ParseCreateDatabase(),
            TokenType.User => ParseCreateUser(),
            _ => UnknownStatement(next.Lexeme)
        }; 
    }

    private Statement ParseCreateUser() {
        var isCreate = Expect(TokenType.Create);
        if (!isCreate) return Error();

        var isUser = Expect(TokenType.User);
        if (!isUser) return Error();

        var nameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();
        Advance();

        var isIdentified = Expect(TokenType.Identified);
        if (!isIdentified) return Error();
        
        var isBy = Expect(TokenType.By);
        if (!isBy) return Error();

        var passwordToken = CurrentToken();
        if (!Match(TokenType.String)) return Error();

        return new CreateUserStatement() {
            Name = nameToken.Lexeme,
            Password = passwordToken.Lexeme
        };
    }

    private Statement ParseDrop() {
        var next = Peek();

        return next.Type switch {
            TokenType.Table => ParseDropTable(),
            TokenType.Database => ParseDropDatabase(),
            TokenType.User => ParseDropUser(),
            _ => UnknownStatement(next.Lexeme)
        }; 
    }

    private Statement ParseDropUser() {
        var isDrop = Expect(TokenType.Drop);
        if (!isDrop) return Error();

        var isUser =  Expect(TokenType.User);
        if (!isUser) return Error();

        var userNameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();
        
        return new DropUserStatement() {
            UserName = userNameToken.Lexeme
        };
    }

    private static Statement UnknownStatement(string stmt) {
        return new BadStatement() {
            Error = $"Invalid statement: {stmt}"
        };
    }

    private Statement ParseSelect() {
        var isSelect = Expect(TokenType.Select);
        if (!isSelect) return Error();

        var isStar = Expect(TokenType.Star);
        if (!isStar) return Error();

        var isFrom = Expect(TokenType.From);
        if (!isFrom) return Error();

        var tableNameToken = CurrentToken();
        if (!Match(TokenType.Identifier)) return Error();

        return new SelectClause() {
            TableName = tableNameToken.Lexeme,
            Columns = []
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

        Recede();
        var columns = new List<ColumnDefinition>();
        do {
            Advance();

            var columnName = CurrentToken();
            if (!Match(TokenType.Identifier)) return Error();
            Advance();

            var columnType = CurrentToken();
            if (!IsDataTypeToken(columnType)) return Error();
            Advance();

            var type = SqlMapper.MapTokenToType(columnType);
            if (type is SqlVarchar sqlVarchar) {
                var isVarcharLeftParen = Expect(TokenType.LeftParen);
                if (!isVarcharLeftParen) return Error();

                var count = CurrentToken();
                if (!Match(TokenType.Numeric) && !Match(TokenType.Identifier)) return Error();
                if (Match(TokenType.Identifier) && count.Lexeme.ToLower() != "max") return Error();
                Advance();

                sqlVarchar.MaxChars = count.Type is TokenType.Numeric 
                    ? int.Parse(count.Lexeme) 
                    : SqlConstants.VarcharMax;

                var isVarcharRightParen = Expect(TokenType.RightParen);
                if (!isVarcharRightParen) return Error();
            }
            else if (type is SqlChar sqlChar) {
                var isCharLeftParen = Expect(TokenType.LeftParen);
                if (!isCharLeftParen) return Error();

                var count = CurrentToken();
                if (!Match(TokenType.Numeric)) return Error();
                Advance();

                sqlChar.MaxChars = int.Parse(count.Lexeme);

                var isCharRightParen = Expect(TokenType.RightParen);
                if (!isCharRightParen) return Error();
            }

            bool canContainNull = true;
            bool isUnique = false;
            if (Match(TokenType.Not)) {
                Advance();

                var isNullToken = Expect(TokenType.Null);
                if (!isNullToken) return Error();

                canContainNull = false;
            }

            if (Match(TokenType.Unique)) {
                isUnique = true;
                Advance();
            }

            var column = new ColumnDefinition() {
                ColumnName = columnName.Lexeme,
                Type = type,
                CanContainNull = canContainNull,
                IsUnique = isUnique
            };

            columns.Add(column);
        } while (Match(TokenType.Comma));

        var isRightParen =  Expect(TokenType.RightParen);
        if (!isRightParen) return Error();

        Recede();

        return new CreateTableStatement() {
            TableName = tableNameToken.Lexeme,
            Columns = columns
        };
    }

    private Statement ParsePrimary() {
        var token = CurrentToken();

        if (token.Type is TokenType.Identifier) {
            return new IdentifierExpression() {
                Value = token.Lexeme
            };
        } 
        else if (token.Type is TokenType.Numeric) {
            return new NumericExpression() {
                Value = int.Parse(token.Lexeme)
            };
        }
        else if (token.Type is TokenType.String) {
            return new StringExpression() {
                Value = token.Lexeme
            };
        }
        else if (token.Type is TokenType.True or TokenType.False) {
            return new BoolExpression() {
                Value = bool.Parse(token.Lexeme)
            };
        }
        else {
            return new BadStatement() { 
                Error = $"Unknown primary expression {token.Type}"
            };
        }
    }

    private Statement ParseUnary() {
        while (Match(TokenType.Not)) {
            var op = CurrentToken();
            Advance();

            var right = ParseUnary();

            return new UnaryExpression() {
                Right = right,
                Op = op
            };
        }

        return ParsePrimary();
    }

    private Statement ParseFactor() {
        var left = ParseUnary();
        Advance();

        while (Match(TokenType.Star) || Match(TokenType.Divide) || Match(TokenType.Modulo)) {
            var op = CurrentToken();
            Advance();

            var right = ParseFactor();

            return new BinaryExpression() {
                Left = left,
                Op = op,
                Right = right
            };
        }

        return left;
    }

    private Statement ParseTerm() {
        var left = ParseFactor();

        while (Match(TokenType.Plus) || Match(TokenType.Minus)) {
            var op = CurrentToken();
            Advance();

            var right = ParseTerm();
            return new BinaryExpression() {
                Left = left,
                Op = op,
                Right = right
            };
        }

        return left;
    }

    private Statement ParseRelational() {
        var left = ParseTerm();

        while (Match(TokenType.LessThan) || Match(TokenType.GreaterThan) || 
            Match(TokenType.GreaterThanEquals) || Match(TokenType.LessThanEquals)) {
            var op = CurrentToken();
            Advance();

            var right = ParseRelational();
            return new BinaryExpression() {
                Left = left,
                Op = op,
                Right = right
            };
        }

        return left;
    }

    private Statement ParseEquality() {
        var left = ParseRelational();

        while (Match(TokenType.Equals) || Match(TokenType.NotEquals)) {
            var op = CurrentToken();
            Advance();

            var right = ParseEquality();
            return new BinaryExpression() {
                Left = left,
                Op = op,
                Right = right
            };
        }

        return left;
    }

    private Statement ParseLogicalAnd() {
        var left = ParseEquality();

        while (Match(TokenType.And)) {
            var op = CurrentToken();
            Advance();

            var right = ParseLogicalAnd();
            return new BinaryExpression() {
                Left = left,
                Op = op,
                Right = right
            };
        }

        return left;
    }

    private Statement ParseLogicalOr() {
        var left = ParseLogicalAnd();

        while (Match(TokenType.Or)) {
            var op = CurrentToken();
            Advance();

            var right = ParseLogicalOr();
            return new BinaryExpression() {
                Left = left,
                Op = op,
                Right = right
            };
        }

        return left;
    }

    private Statement ParseExpression() {
        return ParseLogicalOr();
    }

    private Statement ParseWhere() {
        var isWhere = Expect(TokenType.Where);
        if (!isWhere) return Error();

        var condition = ParseExpression();

        return new WhereClause() {
            Condition = condition
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

        Recede();
        do {
            Advance();

            var columnName = CurrentToken();
            if (!Match(TokenType.Identifier)) return Error();
            Advance();

            columns.Add(columnName.Lexeme);
        } while (Match(TokenType.Comma));

        var isRightParen =  Expect(TokenType.RightParen);
        if (!isRightParen) return Error();

        var isValues =  Expect(TokenType.Values);
        if (!isValues) return Error();

        List<InsertValues> insertValuesList = [];

        Recede();

        do {
            InsertValues insertValues = new() {
                Values = []
            };

            Advance();

            var isValuesLeftParen =  Expect(TokenType.LeftParen);
            if (!isValuesLeftParen) return Error();

            Recede();
            do {
                Advance();
                
                var value = CurrentToken();
                if (!SqlTypeHelper.IsValidTypeToken(value)) return Error();
                Advance();

                insertValues.Values.Add(value);
            } while (Match(TokenType.Comma));

            var isValuesRightParen =  Expect(TokenType.RightParen);
            if (!isValuesRightParen) return Error();

            insertValuesList.Add(insertValues);

        } while (Match(TokenType.Comma));

        Recede();

        return new InsertIntoStatement() {
            TableName = tableNameToken.Lexeme,
            ColumnNames = columns,
            ValuesList = insertValuesList
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
        if (token.Type is TokenType.Char) return true;
        if (token.Type is TokenType.Int) return true;
        if (token.Type is TokenType.Varchar) return true;
        if (token.Type is TokenType.Boolean) return true;
        
        return false;
    }

    private BadStatement Error() {
        Recede();
        var errorToken = CurrentToken();

        return new() {
            Error = $"Syntax error near '{errorToken.Lexeme}'"
        };
    }

    private bool Match(TokenType type) {
        return CurrentToken().Type == type;
    }

    private Token Peek() {
        Advance();
        var token = CurrentToken();
        Recede();

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

    private void Recede() {
        Current--;
    }

    private bool IsLast() {
        return Current >= Tokens.Count;
    }
}