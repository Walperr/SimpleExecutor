using LanguageParser.Expressions;

namespace LanguageParser.Visitors;

public abstract class ExpressionWalker : ExpressionVisitor
{
    public override void VisitBinary(BinaryExpression expression)
    {
        Visit(expression.Left);
        Visit(expression.Right);
    }

    public override void VisitFor(ForExpression expression)
    {
        if (expression.Initialization is not null) 
            Visit(expression.Initialization);

        if (expression.Condition is not null)
            Visit(expression.Condition);
        
        if (expression.Step is not null)
            Visit(expression.Step);

        Visit(expression.Body);
    }

    public override void VisitIf(IfExpression expression)
    {
        Visit(expression.Condition);
        Visit(expression.ThenBranch);
        if (expression.ElseBranch is not null)
            Visit(expression.ElseBranch);
    }

    public override void VisitInvocation(InvocationExpression expression)
    {
        Visit(expression.Function);

        foreach (var argument in expression.Arguments) 
            Visit(argument);
    }

    public override void VisitParenthesized(ParenthesizedExpression expression)
    {
        Visit(expression.Expression);
    }

    public override void VisitRepeat(RepeatExpression expression)
    {
        Visit(expression.CountExpression);
        Visit(expression.Body);
    }

    public override void VisitScope(ScopeExpression expression)
    {
        foreach (var innerExpression in expression.InnerExpressions) 
            Visit(innerExpression);
    }

    public override void VisitVariable(VariableExpression expression)
    {
        if (expression.AssignmentExpression is not null) 
            
            Visit(expression.AssignmentExpression);
    }

    public override void VisitWhile(WhileExpression expression)
    {
        Visit(expression.Condition);
        Visit(expression.Body);
    }

    public override void VisitPrefixUnary(PrefixUnaryExpression expression)
    {
        Visit(expression.Operand);
    }

    public override void VisitPostfixUnary(PostfixUnaryExpression expression)
    {
        Visit(expression.Operand);
    }
}