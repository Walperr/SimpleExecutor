using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class RepeatExpression : ExpressionBase
{
    internal RepeatExpression(Token repeatToken, ExpressionBase countExpression, Token timesToken, ExpressionBase body) : base(SyntaxKind.RepeatExpression)
    {
        RepeatToken = repeatToken;
        CountExpression = countExpression;
        TimesToken = timesToken;
        Body = body;
    }

    public Token RepeatToken { get; }
    public ExpressionBase CountExpression { get; }
    public Token TimesToken { get; }
    public ExpressionBase Body { get; }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return RepeatToken;
        yield return CountExpression;
        yield return TimesToken;
        yield return Body;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitRepeat(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitRepeat(this, state);
    }
}