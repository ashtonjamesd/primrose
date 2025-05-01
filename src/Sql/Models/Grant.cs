namespace Primrose.src.Sql;

internal sealed class SqlGrant {
    public required SqlPrivilege Privilege { get; set; }
    public required string Database { get; set; }
    public required string Table { get; set; }
    public required string ToUser { get; set; }
}