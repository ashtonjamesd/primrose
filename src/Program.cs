using Primrose.src.Sql;
using Primrose.src.Sql.Models;
using Primrose.src.Utils;

namespace Primrose.src;

internal class Program {
    private static readonly TerminalHelper terminal = new();

    static void Main() {
        Console.Clear();
        Console.WriteLine("Database connected.\n");

        var db = new SqlEngine(debug: true);
        var initQuery = File.ReadAllText("example/init.sql");
        db.ExecuteQuery(initQuery);

        while (true) {
            var user = terminal.GetInput("user");
            var pass = terminal.GetInput("pass");

            var isValid = db.Login(user, pass);
            if (isValid) break;
        }

        Console.WriteLine();
        while (true) {
            Console.Write($"#{db.GetUser()?.Name ?? "none"} > ");
            var query = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(query)) continue;
            if (query.StartsWith(':')) {
                if (query.ToLower() is ":quit" or ":q") break;
                continue;
            }

            db.ExecuteQuery(query);
        }

        Console.WriteLine("\nClosed database connection.");
    }
}