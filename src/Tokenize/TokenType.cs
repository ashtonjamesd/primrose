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

    Varchar,
    Integer,
    Boolean,
    Character,

    Commma,
    LeftParen,
    RightParen,
    Semicolon,

    Identifier,
    Numeric,
    
    Bad,
}