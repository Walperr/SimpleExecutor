using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class PostfixUnaryExpression : ExpressionBase
{
    public ConstantExpression Operand { get; }
    public Token OperatorToken { get; }

    public PostfixUnaryExpression(SyntaxKind kind, ConstantExpression operand, Token operatorToken) : base(kind)
    {
        Operand = operand;
        OperatorToken = operatorToken;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return Operand;
        yield return OperatorToken;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitPostfixUnary(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitPostfixUnary(this, state);
    }
}