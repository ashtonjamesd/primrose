namespace Primrose.src.Tokenize;

internal sealed class Token {
    public required string Lexeme { get; set; }
    public required TokenType Type { get; set; }
}