using System.Collections.Immutable;
using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;

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
}