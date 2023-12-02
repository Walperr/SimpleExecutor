using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class ForInExpression : ExpressionBase
{
    public Token ForToken { get; }
    public VariableExpression Variable { get; }
    public Token InToken { get; }
    public ExpressionBase Collection { get; }
    public ExpressionBase Body { get; }

    internal ForInExpression(Token forToken, VariableExpression variable, Token inToken, ExpressionBase collection, ExpressionBase body) : base(SyntaxKind.ForInExpression)
    {
        ForToken = forToken;
        Variable = variable;
        InToken = inToken;
        Collection = collection;
        Body = body;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return ForToken;
        yield return Variable;
        yield return InToken;
        yield return Collection;
        yield return Body;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitForIn(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitForIn(this, state);
    }
}