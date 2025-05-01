using Primrose.src.Tokenize;

namespace  Primrose.src.Parse;

internal sealed class SqlMapper {
    public static SqlType MapTokenToType(Token token) {
        return token.Type switch {
            TokenType.Boolean => new SqlBoolean(),
            TokenType.Varchar => new SqlVarchar(),
            TokenType.Int => new SqlInt(),
            TokenType.Char => new SqlChar(),
            _ => new SqlUnknown() { Item = token }
        };
    }
}