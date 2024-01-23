using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class FunctionDeclarationExpression : ExpressionBase
{
    public Token FunctionToken { get; }
    public Token NameToken { get; }
    public Token OpenParenthesis { get; }
    public IEnumerable<ExpressionBase> Parameters { get; }
    public Token CloseParenthesis { get; }
    public ExpressionBase Body { get; }

    internal FunctionDeclarationExpression(Token functionToken, Token nameToken, Token openParenthesis, IEnumerable<ExpressionBase> parameters, Token closeParenthesis, ExpressionBase body) : base(SyntaxKind.FunctionDeclarationExpression)
    {
        FunctionToken = functionToken;
        NameToken = nameToken;
        OpenParenthesis = openParenthesis;
        Parameters = parameters;
        CloseParenthesis = closeParenthesis;
        Body = body;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return FunctionToken;
        yield return NameToken;
        yield return OpenParenthesis;
        foreach (var parameter in Parameters)
            yield return parameter;
        yield return CloseParenthesis;
        yield return Body;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitFunctionDeclaration(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitFunctionDeclaration(this, state);
    }
}