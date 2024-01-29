namespace Compiler2;

public record Variable(Type Type, string Name)
{
    public Type Type { get; set; } = Type;
}