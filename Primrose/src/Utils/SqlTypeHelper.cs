using Primrose.src.Parse;
using Primrose.src.Tokenize;

namespace Primrose.src.Utils;

public static class SqlTypeHelper {

    // Determines whether a token maps to a valid type.
    public static bool IsValidTypeToken(Token token) {
        return token.Type is TokenType.Numeric 
            or TokenType.String
            or TokenType.True
            or TokenType.False
            or TokenType.Null;
    }

    // Determines whether a Type and a Token are compatible
    //
    // For instance, the 'SqlBool' type is compatible with: 
    //   - TokenType.True 
    //   - TokenType.False 
    // 
    // but not TokenType.Int
    public static bool IsTokenTypeMatch(SqlType type, Token token) {
        return type switch {
            SqlBoolean => token.Type is TokenType.True or TokenType.False,
            SqlInt => token.Type is TokenType.Numeric,
            SqlVarchar => token.Type is TokenType.String,
            SqlChar => token.Type is TokenType.String,
            _ => false
        };
    }

    public static bool IsTokenPrivilegeMatch(Token token) {
        List<TokenType> allowedTokens = [
            TokenType.Select,
            TokenType.Insert,
            // TokenType.Update,
            TokenType.Delete,
            TokenType.Create,
            // TokenType.Alter,
            TokenType.Drop,
        ];

        return allowedTokens.Contains(token.Type);
    }
}