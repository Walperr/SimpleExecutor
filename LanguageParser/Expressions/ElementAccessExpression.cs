using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class ElementAccessExpression : ExpressionBase
{
    internal ElementAccessExpression(ExpressionBase expression,
        Token openBracket,
        IEnumerable<ExpressionBase> arguments,
        Token closeBracket) : base(SyntaxKind.ElementAccessExpression)
    {
        Expression = expression;
        OpenBracket = openBracket;
        Arguments = arguments;
        CloseBracket = closeBracket;
    }

    public ExpressionBase Expression { get; }
    public Token OpenBracket { get; }
    public IEnumerable<ExpressionBase> Arguments { get; }
    public Token CloseBracket { get; }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return Expression;
        yield return OpenBracket;
        foreach (var argument in Arguments)
            yield return argument;
        yield return CloseBracket;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        throw new NotImplementedException();
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        throw new NotImplementedException();
    }
}