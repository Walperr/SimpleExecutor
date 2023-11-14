namespace LanguageParser.Expressions;

public sealed class TopLevelExpression
{
    public TopLevelExpression(ExpressionBase expression, string? options)
    {
        Expression = expression;
        Options = options;
    }
    public ExpressionBase Expression { get; }
    
    public string? Options { get; }

    public override string ToString()
    {
        var options = string.IsNullOrEmpty(Options)
            ? string.Empty
            : $"Options: '{Options}'";

        return $"{GetType().Name} ({Expression}) {options}";
    }
}