namespace Compiler;

public sealed class Function
{
    public required string Name { get; init; }

    public required ushort ID { get; init; }

    public required IReadOnlyDictionary<ushort, Variable> Variables { get; init; } // including parameters

    public required Variable[] Parameters { get; init; }

    public required Type ReturnType { get; init; }

    public required byte[]? Instructions { get; init; }
}