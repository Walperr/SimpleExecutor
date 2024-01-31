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

        var result = ExpressionsParser.Parse(code);

        if (result.IsError)
        {
            Console.WriteLine(result.Error.Message);
            return 1;
        }
        
        var scope = DeclarationsCollector.Collect(result.Value);

        if (scope is null)
        {
            Console.WriteLine("Compile failed");
            return 1;
        }

        var type = TypeResolver.Resolve(scope);

        if (type.IsError)
        {
            Console.WriteLine(type.Error.Message);
            return 1;
        }

        var (bytes, constants, types, functions) = Compiler.Compile(scope);


        var path = Path.ChangeExtension(args[0]!, ".out");
        if (File.Exists(path))
            File.Delete(path);
        
        using var writer = new BinaryWriter(File.OpenWrite(path));
        
        writer.Write(constants.Length);
        writer.Write(types.Length);
        writer.Write(functions.Length);
        writer.Write(bytes.Length);
        // writer.Write((byte)255);
        writer.Write(constants);
        // writer.Write((byte)255);
        writer.Write(types);
        // writer.Write((byte)255);
        writer.Write(functions);
        // writer.Write((byte)255);
        writer.Write(bytes);
        // writer.Write((byte)255);

        return 0;
    }
}