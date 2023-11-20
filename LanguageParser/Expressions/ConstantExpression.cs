using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class ConstantExpression : ExpressionBase, ISyntaxElement
{
    internal ConstantExpression(SyntaxKind kind, Token token) : base(kind)
    {
        Token = token;
    }

    public Token Token { get; }

    public bool IsNumber => Kind == SyntaxKind.NumberLiteral;

    public bool IsString => Kind == SyntaxKind.StringLiteral;

    public bool IsBool => Kind is SyntaxKind.True or SyntaxKind.False;

    public string Lexeme => Token.Lexeme;

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return Token;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitConstant(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitConstant(this, state);
    }
}