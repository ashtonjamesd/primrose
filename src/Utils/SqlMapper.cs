using Primrose.src.Parse;
using Primrose.src.Sql;
using Primrose.src.Tokenize;

namespace Primrose.src.Utils;

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

    public static SqlPrivilege MapStringToPrivilege(string privilege) {
        return privilege switch {
            "*" or "all" or "all privileges" => SqlPrivilege.all,
            "select" => SqlPrivilege.Select,
            "insert" => SqlPrivilege.Insert,
            "create" => SqlPrivilege.Create,
            "alter" => SqlPrivilege.Alter,
            "delete" => SqlPrivilege.Delete,
            "drop" => SqlPrivilege.Drop,
            "update" => SqlPrivilege.Update,
            _ => SqlPrivilege.unknown
        }; 
    }
}