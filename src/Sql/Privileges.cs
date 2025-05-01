namespace Primrose.src.Sql;

internal enum SqlPrivilege {
    all,

    Select,
    Insert,
    Update,
    Delete,
    Create,
    Alter,
    Drop,
    unknown,
}