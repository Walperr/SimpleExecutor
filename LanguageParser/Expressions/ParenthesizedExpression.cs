using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;

namespace LanguageParser.Expressions;

public sealed class ParenthesizedExpression : ExpressionBase
{
    internal ParenthesizedExpression(Token open, ExpressionBase expression, Token close) : base(SyntaxKind
        .ParenthesizedExpression)
    {
        OpenParenthesis = open;
        Expression = expression;
        CloseParenthesis = close;
    }

    public Token OpenParenthesis { get; }
    public ExpressionBase Expression { get; }
    public Token CloseParenthesis { get; }


    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return OpenParenthesis;
        yield return Expression;
        yield return CloseParenthesis;
    }
}