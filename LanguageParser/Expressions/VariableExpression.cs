using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class VariableExpression : ExpressionBase
{
    internal VariableExpression(ExpressionBase typeExpression, Token nameToken,
        BinaryExpression? assignmentExpression = null) : base(SyntaxKind.VariableExpression)
    {
        TypeExpression = typeExpression;
        NameToken = nameToken;
        AssignmentExpression = assignmentExpression;
    }

    public ExpressionBase TypeExpression { get; }
    public Token NameToken { get; }
    public ExpressionBase? AssignmentExpression { get; }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return TypeExpression;
        yield return NameToken;

        if (AssignmentExpression is not null)
            yield return AssignmentExpression;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitVariable(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitVariable(this, state);
    }
}