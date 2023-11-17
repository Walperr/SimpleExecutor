using LanguageParser.Expressions;

namespace LanguageParser.Visitors;

public abstract class ExpressionVisitor
{
    public virtual void Visit(ExpressionBase expression) => expression.Visit(this);

    public virtual void VisitBinary(BinaryExpression expression)
    {
    }

    public virtual void VisitConstant(ConstantExpression expression)
    {
    }

    public virtual void VisitFor(ForExpression expression)
    {
    }

    public virtual void VisitIf(IfExpression expression)
    {
    }

    public virtual void VisitInvocation(InvocationExpression expression)
    {
    }

    public virtual void VisitParenthesized(ParenthesizedExpression expression)
    {
    }

    public virtual void VisitRepeat(RepeatExpression expression)
    {
    }

    public virtual void VisitScope(ScopeExpression expression)
    {
    }

    public virtual void VisitVariable(VariableExpression expression)
    {
    }

    public virtual void VisitWhile(WhileExpression expression)
    {
    }
}

public abstract class ExpressionVisitor<T, TState>
{
    public virtual T Visit(ExpressionBase expression, TState state) => expression.Visit(this, state);

    public abstract T VisitBinary(BinaryExpression expression, TState state);

    public abstract T VisitConstant(ConstantExpression expression, TState state);
                    
    public abstract T VisitFor(ForExpression expression, TState state);
                    
    public abstract T VisitIf(IfExpression expression, TState state);
                    
    public abstract T VisitInvocation(InvocationExpression expression, TState state);
                    
    public abstract T VisitParenthesized(ParenthesizedExpression expression, TState state);
                    
    public abstract T VisitRepeat(RepeatExpression expression, TState state);
                    
    public abstract T VisitScope(ScopeExpression expression, TState state);
                    
    public abstract T VisitVariable(VariableExpression expression, TState state);
                    
    public abstract T VisitWhile(WhileExpression expression, TState state);
}