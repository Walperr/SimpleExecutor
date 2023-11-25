using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class PrefixUnaryExpression : ExpressionBase
{
    public Token OperatorToken { get; }
    public ExpressionBase Operand { get; }

    internal PrefixUnaryExpression(SyntaxKind kind, Token operatorToken, ExpressionBase operand) : base(kind)
    {
        OperatorToken = operatorToken;
        Operand = operand;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return OperatorToken;
        yield return Operand;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitPrefixUnary(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitPrefixUnary(this, state);
    }
}