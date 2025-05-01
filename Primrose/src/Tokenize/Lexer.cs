namespace Primrose.src.Tokenize;

public sealed class Lexer {
    private int Current = 0;
    private readonly string Source;
    private readonly List<Token> Tokens = []; 
    
    public Lexer(string source) {
        Source = source;
    }

    private readonly Dictionary<string, TokenType> Keywords = new() {
        ["create"] = TokenType.Create,
        ["table"] = TokenType.Table,
        ["drop"] = TokenType.Drop,
        ["int"] = TokenType.Int,
        ["boolean"] = TokenType.Boolean,
        ["varchar"] = TokenType.Varchar,
        ["char"] = TokenType.Char,
        ["use"] = TokenType.Use,
        ["database"] = TokenType.Database,
        ["insert"] = TokenType.Insert,
        ["into"] = TokenType.Into,
        ["values"] = TokenType.Values,
        ["select"] = TokenType.Select,
        ["from"] = TokenType.From,
        ["true"] = TokenType.True,
        ["false"] = TokenType.False,
        ["null"] = TokenType.Null,
        ["not"] = TokenType.Not,
        ["unique"] = TokenType.Unique,
        ["where"] = TokenType.Where,
        ["and"] = TokenType.And,
        ["or"] = TokenType.Or,
        ["user"] = TokenType.User,
        ["identified"] = TokenType.Identified,
        ["by"] = TokenType.By,
        ["grant"] = TokenType.Grant,
        ["all"] = TokenType.All,
        ["privileges"] = TokenType.Privileges,
        ["on"] = TokenType.On,
        ["to"] = TokenType.To,
        ["login"] = TokenType.Login,
        ["with"] = TokenType.With,
        ["option"] = TokenType.Option,
    };

    private readonly Dictionary<string, TokenType> SingleSymbols = new() {
        ["("] = TokenType.LeftParen,
        [")"] = TokenType.RightParen,
        [","] = TokenType.Comma,
        [";"] = TokenType.Semicolon,
        ["\'"] = TokenType.Quote,
        ["*"] = TokenType.Star,
        ["-"] = TokenType.Minus,
        ["+"] = TokenType.Plus,
        ["/"] = TokenType.Divide,
        ["%"] = TokenType.Modulo,
        ["="] = TokenType.Equals,
        [">"] = TokenType.GreaterThan,
        ["<"] = TokenType.LessThan,
        ["&"] = TokenType.Ampersand,
        ["|"] = TokenType.Pipe,
        ["^"] = TokenType.Caret,
        ["."] = TokenType.Dot,
    };

    private readonly Dictionary<string, TokenType> DoubleSymbols = new() {
        ["<>"] = TokenType.NotEquals,
        [">="] = TokenType.GreaterThanEquals,
        ["<="] = TokenType.LessThanEquals,
    };

    public void Print() {
        Console.WriteLine($"\nCurrent:        {Current}");
        Console.WriteLine($"Source:         {Source}");
        Console.WriteLine($"Source Length:  {Source.Length}");
        Console.WriteLine($"Tokens ({Tokens.Count}):");

        foreach (var token in Tokens) {
            Console.WriteLine($"  '{token.Lexeme}': {token.Type}");
        }
        Console.WriteLine();
    }

    public List<Token> Tokenize() {
        while (!IsEnd()) {
            SkipWhitespaceAndComments();

            var token = ParseToken();
            Tokens.Add(token);

            Advance();
        }

        return Tokens;
    }

        private void SkipWhitespaceAndComments() {
            while (!IsEnd()) {
                var c = CurrentChar();

                if (c is ' ' or '\n' or '\t' or '\r') {
                    Advance();
                }
                else if (c is '-' && Peek() is '-') {
                    Advance();
                    Advance();
                    while (CurrentChar() is not ('\n' or '\r') && !IsEnd()) {
                        Advance();
                    }
                }
                else {
                    break;
                }
            }
        }

    private Token ParseToken() {
        var c = CurrentChar();

        return c switch {
            _ when char.IsLetter(c) => ParseIdentifier(),
            _ when char.IsDigit(c) => ParseNumeric(),
            _ when c is '\'' => ParseString(),
            _ => ParseSymbol()
        }; 
    }

    private Token ParseNumeric() {
        int start = Current;

        while (char.IsDigit(CurrentChar())) {
            Advance();
        }

        var lexeme = Source[start..Current];
        Recede();

        return NewToken(lexeme, TokenType.Numeric);
    }

    private Token ParseString() {
        int start = Current;
        Advance();

        while (CurrentChar() is not '\'') {
            Advance();
        }

        var lexeme = Source[(start + 1)..Current];

        return NewToken(lexeme, TokenType.String);
    }

    private Token ParseIdentifier() {
        int start = Current;
        while (!IsEnd() && IsValidIdentifierChar(CurrentChar())) {
            Advance();
        }

        var lexeme = Source[start..Current];
        Recede();
        
        if (Keywords.TryGetValue(lexeme, out var type)) {
            return NewToken(lexeme, type);
        }

        return NewToken(lexeme, TokenType.Identifier);
    }

    private Token ParseSymbol() {
        var first = CurrentChar().ToString();

        if (SingleSymbols.TryGetValue(first, out var firstType)) {
            Advance();
            var second = CurrentChar();
            var str = (first + second).ToString();

            if (DoubleSymbols.TryGetValue(first, out var secondType)) {
                return NewToken(str, secondType);
            }

            Current--;
            return NewToken(first, firstType);
        }

        return NewToken(first, TokenType.Bad);
    }

    private static bool IsValidIdentifierChar(char c) {
        if (char.IsLetterOrDigit(c)) return true;
        if (c is '_' or '-') return true;

        return false;
    }

    private static Token NewToken(string lexeme, TokenType type) {
        return new Token() {
            Lexeme = lexeme,
            Type = type
        };
    }

    private bool IsEnd() {
        return Current >= Source.Length;
    }

    private char CurrentChar() {
        return IsEnd() ? '\0' : Source[Current];
    }

    private char Peek() {
        Advance();
        var c = CurrentChar();
        Recede();

        return c;
    }

    private void Advance() {
        Current++;
    }

    private void Recede() {
        Current--;
    }
}