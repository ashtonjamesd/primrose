using Primrose.src.Parse;
using Primrose.src.Sql;
using Primrose.src.Tokenize;

namespace Primrose.src.Utils;

public sealed class SqlMapper {
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
            "all privileges" => SqlPrivilege.All,
            "select" => SqlPrivilege.Select,
            "insert" => SqlPrivilege.Insert,
            "create" => SqlPrivilege.Create,
            "alter" => SqlPrivilege.Alter,
            "delete" => SqlPrivilege.Delete,
            "drop" => SqlPrivilege.Drop,
            "update" => SqlPrivilege.Update,
            "with grant option" => SqlPrivilege.Grant,
            "create user" => SqlPrivilege.CreateUser,
            _ => SqlPrivilege.Unknown
        }; 
    }
}