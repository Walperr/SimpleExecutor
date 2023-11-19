using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class RepeatExpression : ExpressionBase
{
    internal RepeatExpression(Token repeatToken, ExpressionBase body, Token untilToken, Token openParenthesis,
        ExpressionBase condition, Token closeParenthesis) : base(SyntaxKind.RepeatExpression)
    {
        RepeatToken = repeatToken;
        Body = body;
        UntilToken = untilToken;
        OpenParenthesis = openParenthesis;
        Condition = condition;
        CloseParenthesis = closeParenthesis;
    }

    public Token RepeatToken { get; }
    public ExpressionBase Body { get; }
    public Token UntilToken { get; }
    public Token OpenParenthesis { get; }
    public ExpressionBase Condition { get; }
    public Token CloseParenthesis { get; }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return RepeatToken;
        yield return Body;
        yield return UntilToken;
        yield return OpenParenthesis;
        yield return Condition;
        yield return CloseParenthesis;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitRepeat(this);
    }

    public override T Visit<T>(ExpressionVisitor<T> visitor)
    {
        return visitor.VisitRepeat(this);
    }
}