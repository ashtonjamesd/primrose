using Primrose.src.Sql;
using Primrose.src.Sql.Models;
using Primrose.src.Utils;

namespace Primrose.src;

public class Program {
    private static readonly TerminalHelper terminal = new();

    static void Main() {
        Console.Clear();

        var db = new SqlEngine(debug: true);
        db.Setup();

        Console.WriteLine("primrose db\n");
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
                var parts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                switch (parts[0].ToLower()) {
                    case ":quit":
                    case ":q":
                        return;

                    case ":login":
                        if (parts.Length != 3) {
                            Console.WriteLine("Usage: :login <user> <pass>");
                        } else {
                            var user = parts[1];
                            var pass = parts[2];
                            var success = db.Login(user, pass);
                            Console.WriteLine(success ? $"Logged in as {user}\n" : "Login failed\n");
                        }
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {parts[0]}");
                        break;
                }

                continue;
            }

            db.ExecuteQuery(query);
        }
    }
}