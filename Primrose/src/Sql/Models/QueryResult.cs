namespace Primrose.src.Sql.Models;

public sealed class QueryResult {
    public required int RowsAffected { get; set; }
    public required string Message { get; set; }
    public required bool IsSuccess { get; set; }
    public required SqlTable? Table { get; set; }

    public static QueryResult Ok(int rowsAffected = 0, SqlTable? table = null) {
        return new() {
            RowsAffected = rowsAffected,
            Message = "Query complete.",
            IsSuccess = true,
            Table = table
        };
    }
    
    public static QueryResult Err(string error) {
        return new() {
            RowsAffected = 0,
            Message = error,
            IsSuccess = false,
            Table = null
        };
    }
}