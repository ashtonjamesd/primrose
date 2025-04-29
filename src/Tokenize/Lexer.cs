namespace Primrose.src.Tokenize;

internal sealed class Lexer {
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
        ["int"] = TokenType.Integer,
        ["bool"] = TokenType.Boolean,
        ["varchar"] = TokenType.Varchar,
        ["char"] = TokenType.Character,
        ["use"] = TokenType.Use,
        ["database"] = TokenType.Database,
        ["insert"] = TokenType.Insert,
        ["into"] = TokenType.Into,
        ["values"] = TokenType.Values,
    };

    private readonly Dictionary<string, TokenType> Symbols = new() {
        ["("] = TokenType.LeftParen,
        [")"] = TokenType.RightParen,
        [","] = TokenType.Commma,
        [";"] = TokenType.Semicolon,
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
            while (CurrentChar() is ' ' or '\n' or '\t' or '\r') {
                Advance();
            }

            var token = ParseToken();
            Tokens.Add(token);

            Advance();
        }

        return Tokens;
    }

    private Token ParseToken() {
        var c = CurrentChar();

        return c switch {
            _ when IsValidIdentifierChar(c) => ParseIdentifier(),
            _ => ParseSymbol()
        }; 
    }

    private Token ParseSymbol() {
        var c = CurrentChar().ToString();

        if (Symbols.TryGetValue(c, out var type)) {
            return NewToken(c, type);
        }

        return NewToken("", TokenType.Bad);
    }

    private Token ParseIdentifier() {
        int start = Current;
        while (!IsEnd() && IsValidIdentifierChar(CurrentChar())) {
            Advance();
        }

        var lexeme = Source[start..Current];
        Current--;
        
        if (Keywords.TryGetValue(lexeme, out var type)) {
            return NewToken(lexeme, type);
        }

        return NewToken(lexeme, TokenType.Identifier);
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

    private void Advance() {
        Current++;
    }
}