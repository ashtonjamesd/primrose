namespace Primrose.src.Tokenize;

public sealed class Token {
    public required string Lexeme { get; set; }
    public required TokenType Type { get; set; }
}