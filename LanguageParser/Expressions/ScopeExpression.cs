using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class ScopeExpression : ExpressionBase
{
    public Token OpenBrace { get; }
    public IEnumerable<ExpressionBase> InnerExpressions { get; }
    public Token CloseBrace { get; }

    internal ScopeExpression(Token openBrace, IEnumerable<ExpressionBase> innerExpressions, Token closeBrace) : base(SyntaxKind.ScopeExpression)
    {
        OpenBrace = openBrace;
        InnerExpressions = innerExpressions;
        CloseBrace = closeBrace;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return OpenBrace;

        foreach (var expression in InnerExpressions)
            yield return expression;

        yield return CloseBrace;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitScope(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitScope(this, state);
    }
}