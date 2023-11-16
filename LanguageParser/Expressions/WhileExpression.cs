using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;

namespace LanguageParser.Expressions;

public sealed class WhileExpression : ExpressionBase
{
    public Token WhileToken { get; }
    public Token OpenParenthesis { get; }
    public ExpressionBase Condition { get; }
    public Token CloseParenthesis { get; }
    public ExpressionBase Body { get; }

    internal WhileExpression(Token whileToken, Token openParenthesis, ExpressionBase condition, Token closeParenthesis, ExpressionBase body) : base(SyntaxKind.WhileExpression)
    {
        WhileToken = whileToken;
        OpenParenthesis = openParenthesis;
        Condition = condition;
        CloseParenthesis = closeParenthesis;
        Body = body;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return WhileToken;
        yield return OpenParenthesis;
        yield return Condition;
        yield return CloseParenthesis;
        yield return Body;
    }
}