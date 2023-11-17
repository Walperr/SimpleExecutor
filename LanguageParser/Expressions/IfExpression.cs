using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;

namespace LanguageParser.Expressions;

public sealed class IfExpression : ExpressionBase
{
    internal IfExpression(Token ifToken, Token openToken, ExpressionBase condition, Token closeToken,
        ExpressionBase thenBranch, Token? elseToken = null, ExpressionBase? elseBranch = null) : base(SyntaxKind
        .IfExpression)
    {
        IfToken = ifToken;
        OpenToken = openToken;
        Condition = condition;
        CloseToken = closeToken;
        ThenBranch = thenBranch;
        ElseToken = elseToken;
        ElseBranch = elseBranch;
    }

    public Token IfToken { get; }
    public Token OpenToken { get; }
    public ExpressionBase Condition { get; }
    public Token CloseToken { get; }
    public ExpressionBase ThenBranch { get; }
    public Token? ElseToken { get; }
    public ExpressionBase? ElseBranch { get; }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return IfToken;
        yield return OpenToken;
        yield return Condition;
        yield return CloseToken;
        yield return ThenBranch;
        
        if (ElseToken is not null)
            yield return ElseToken;
        if (ElseBranch is not null)
            yield return ElseBranch;
    }
}