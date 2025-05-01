namespace Primrose.src.Sql.Models;

internal sealed class QueryResult {
    public required int RowsAffected { get; set; }
    public required string Message { get; set; }
    public required bool IsSuccess { get; set; }

    public static QueryResult Ok(int rowsAffected = 0) {
        return new() {
            RowsAffected = rowsAffected,
            Message = "Query complete.",
            IsSuccess = true,
        };
    }
    
    public static QueryResult Err(string error) {
        return new() {
            RowsAffected = 0,
            Message = error,
            IsSuccess = false,
        };
    }
}