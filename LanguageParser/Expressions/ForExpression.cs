using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class ForExpression : ExpressionBase
{
    public Token ForToken { get; }
    public Token OpenParenthesis { get; }
    public ExpressionBase? Initialization { get; }
    public ExpressionBase? Condition { get; }
    public ExpressionBase? Step { get; }
    public Token CloseParenthesis { get; }
    public ExpressionBase Body { get; }

    internal ForExpression(Token forToken, Token openParenthesis, ExpressionBase? initialization, ExpressionBase? condition, ExpressionBase? step, Token closeParenthesis, ExpressionBase body) : base(SyntaxKind.ForExpression)
    {
        ForToken = forToken;
        OpenParenthesis = openParenthesis;
        Initialization = initialization;
        Condition = condition;
        Step = step;
        CloseParenthesis = closeParenthesis;
        Body = body;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return ForToken;
        yield return OpenParenthesis;
        if (Initialization is not null)
            yield return Initialization;
         
        if (Condition is not null)
            yield return Condition;
        
        if (Step is not null)
            yield return Step;
        yield return CloseParenthesis;
        yield return Body;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitFor(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitFor(this, state);
    }
}