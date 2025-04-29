using Primrose.src.Sql;

namespace Primrose.src;

internal class Program {
    static void Main() {
        var db = new SqlEngine(debug: true);

        Console.Clear();
        Console.WriteLine("Database connected.\n");

        //
            db.ExecuteQuery(File.ReadAllText("example/source.sql"));
        //
        return;

        while (true) {
            Console.Write("> ");
            var query = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(query)) continue;
            if (query.StartsWith(':')) {
                if (query.ToLower() is ":quit" or ":q") break;
            }

            db.ExecuteQuery(query);
        }

        Console.WriteLine("\nClosed database connection.");
    }
}