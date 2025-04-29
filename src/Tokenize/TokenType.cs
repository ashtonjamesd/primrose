namespace Primrose.src.Tokenize;

internal enum TokenType {
    Create,
    Drop,
    Table,
    Database,
    Use,
    Insert,
    Into,
    Values,
    Select,
    From,

    Varchar,
    Integer,
    Boolean,
    Character,

    Comma,
    LeftParen,
    RightParen,
    Semicolon,
    Star,
    Quote,

    Identifier,
    String,
    Numeric,
    
    Bad,
}