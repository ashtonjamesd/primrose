namespace Primrose.src.Sql;

internal enum SqlPrivilege {
    All,

    Select,
    Insert,
    Update,
    Delete,
    Create,
    Alter,
    Drop,
    Grant,
    Unknown,
}