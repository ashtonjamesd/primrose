namespace Primrose.src.Utils;

public sealed class CliHelper {
    public string GetInput(string message) {
        while (true) {
            Console.Write($"{message}: ");
            var input = Console.ReadLine();
            
            if (input is null or "") continue;
            return input;
        }
    }
}