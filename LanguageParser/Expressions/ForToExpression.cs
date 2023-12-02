using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class ForToExpression : ExpressionBase
{
    public Token ForToken { get; }
    public VariableExpression Variable { get; }
    public Token? DownToken { get; }
    public Token ToToken { get; }
    public ExpressionBase Count { get; }
    public ExpressionBase Body { get; }

    internal ForToExpression(Token forToken, VariableExpression variable, Token? downToken, Token toToken, ExpressionBase count, ExpressionBase body) : base(SyntaxKind.ForToExpression)
    {
        ForToken = forToken;
        Variable = variable;
        DownToken = downToken;
        ToToken = toToken;
        Count = count;
        Body = body;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return ForToken;
        yield return Variable;
        if (DownToken is not null)
            yield return DownToken;
        yield return ToToken;
        yield return Count;
        yield return Body;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitForTo(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitForTo(this, state);
    }
}