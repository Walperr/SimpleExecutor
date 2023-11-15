namespace LanguageParser.Expressions;

public sealed class Program
{
    public IEnumerable<ExpressionBase> Expressions { get; }

    public Program(IEnumerable<ExpressionBase> expressions)
    {
        Expressions = expressions;
    }
}