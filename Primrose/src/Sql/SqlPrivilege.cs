namespace Primrose.src.Sql;

public enum SqlPrivilege {
    All,

    Select,
    Insert,
    Update,
    Delete,
    Create,
    CreateUser,
    Alter,
    Drop,
    Grant,
    Unknown,
}