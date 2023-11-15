using LanguageParser.Common;
using LanguageParser.Interfaces;

namespace LanguageParser.Expressions;

public sealed class BinaryExpression : ExpressionBase
{
    internal BinaryExpression(SyntaxKind kind, ExpressionBase left, ConstantExpression @operator, ExpressionBase right) : base(kind)
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }
    
    public ExpressionBase Left { get; }
    public ConstantExpression Operator { get; }
    public ExpressionBase Right { get; }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return Left;
        yield return Operator;
        yield return Right;
    }
}