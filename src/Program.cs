using Primrose.src.Sql;

namespace Primrose.src;

internal class Program {
    static void Main() {
        var db = new SqlEngine();

        Console.Clear();
        while (true) {
            Console.Write("> ");
            var query = Console.ReadLine();
            
            db.ExecuteQuery(query);
        }

    }
}