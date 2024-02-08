using LanguageParser.Parser;

namespace Compiler;

public static class Program
{
    public static int Main(string?[] args)
    {
        if (args.Length != 1 || args[0] is null || args[0] == string.Empty)
        {
            Console.WriteLine("Expected input file path");
            return 1;
        }

        using var stream = File.OpenText(args[0]!);

        var code = stream.ReadToEnd();

        var path = Path.ChangeExtension(args[0]!, ".out");
        if (File.Exists(path))
            File.Delete(path);

        using (var file = File.OpenWrite(path))
        {
            Compiler.Compile(code, file);
        }
        
        using (var file = File.OpenRead(path))
        {
            Console.Write(AssemblyBuilder.LoadAssembly(file).Build());
        }
        
        return 0;
    }
}