using System.Collections.Immutable;
using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class InvocationExpression : ExpressionBase
{
    internal InvocationExpression(ExpressionBase function, Token openParenthesis, IEnumerable<ExpressionBase> args,
        Token closeParenthesis) : base(SyntaxKind.InvocationExpression)
    {
        Function = function;
        OpenParenthesis = openParenthesis;
        Arguments = args.ToImmutableArray();
        CloseParenthesis = closeParenthesis;
    }

    public ExpressionBase Function { get; }
    public Token OpenParenthesis { get; }
    public ImmutableArray<ExpressionBase> Arguments { get; }
    public Token CloseParenthesis { get; }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return Function;
        yield return OpenParenthesis;
        foreach (var argument in Arguments)
            yield return argument;
        yield return CloseParenthesis;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitInvocation(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitInvocation(this, state);
    }
}