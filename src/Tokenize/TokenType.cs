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
    Not,
    Unique,

    Varchar,
    Int,
    Boolean,
    Char,

    Comma,
    LeftParen,
    RightParen,
    Semicolon,
    Star,
    Quote,
    Minus,

    Identifier,
    String,
    Numeric,
    True,
    False,
    Null,
    
    Bad,
}