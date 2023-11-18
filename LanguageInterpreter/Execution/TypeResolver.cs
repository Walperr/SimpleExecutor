using System.ComponentModel;
using LanguageParser;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

namespace LanguageInterpreter.Execution;

public sealed class TypeResolver : ExpressionVisitor<Type, object>
{
    private readonly ScopeNode _scopeTree;

    public TypeResolver(ScopeNode scopeTree)
    {
        _scopeTree = scopeTree;
    }

    public static Type Resolve(ScopeNode tree)
    {
        return new TypeResolver(tree).Resolve();
    }

    public Type Resolve()
    {
        return Visit(_scopeTree.Scope, null!);
    }

    public override Type VisitBinary(BinaryExpression expression, object state)
    {
        var leftType = Visit(expression.Left, state);
        var rightType = Visit(expression.Right, state);

        switch (expression.Kind)
        {
            case SyntaxKind.DivideExpression:
            case SyntaxKind.MultiplyExpression:
            case SyntaxKind.SubtractExpression:
            if (leftType != typeof(double) || rightType != typeof(double))
                    throw new Exception("left and right operands must be numbers"); 
            return typeof(double);
            case SyntaxKind.AddExpression:
                if (leftType == typeof(string) || rightType == typeof(string))
                    return typeof(string);
                if (leftType == typeof(bool) || rightType == typeof(bool))
                    throw new Exception("left and right operands of '+' cannot be bool");
                if (leftType == typeof(double) && rightType == typeof(double))
                    return typeof(double);
                throw new Exception("right operand of '+' must be number");
            case SyntaxKind.RelationalExpression:
                if (leftType != typeof(double) || rightType != typeof(double))
                    throw new Exception("left and right operands must be numbers");
                return typeof(bool);
            case SyntaxKind.AndExpression:
            case SyntaxKind.OrExpression:
                if (leftType != typeof(bool) || rightType != typeof(bool))
                    throw new Exception("left and right operands must be bool");
                return typeof(bool);
            case SyntaxKind.AssignmentExpression:
                if (expression.Left is ConstantExpression constant)
                {
                    if (constant.IsBool || constant.IsNumber || constant.IsString)
                        throw new Exception("left operand of assignment operator must be variable");

                    var variable = GetVariable(expression, constant.Lexeme);

                    if (variable.Type != rightType)
                        throw new Exception(
                            $"{rightType} expression cannot be assigned to variable of type {variable}");

                    return variable.Type;
                }

                break;
            case SyntaxKind.EqualityExpression:
                return typeof(bool);
        }

        throw new InvalidEnumArgumentException();
    }

    public override Type VisitConstant(ConstantExpression expression, object state)
    {
        if (expression.IsBool)
            return typeof(bool);
        if (expression.IsNumber)
            return typeof(double);
        if (expression.IsString)
            return typeof(string);

        var variable = GetVariable(expression, expression.Lexeme);

        if (variable is null)
            throw new Exception($"undeclarated variable {expression.Lexeme}");

        return variable.Type;
    }

    private Variable GetVariable(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _scopeTree.Scope);

        var node = _scopeTree.FindDescendant(node => node.Scope == scope);

        var variable = node?.GetVariableIncludingAncestors(name);

        if (variable is null)
            throw new Exception($"undeclarated variable {name}");

        return variable;
    }

    private FunctionBase GetFunction(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _scopeTree.Scope);

        var node = _scopeTree.FindDescendant(node => node.Scope == scope);

        var function = node?.GetFunctionIncludingAncestors(name);

        if (function is null)
            throw new Exception($"undeclarated function {name}");

        return function;
    }

    public override Type VisitFor(ForExpression expression, object state)
    {
        var conditionType = expression.Condition is null
            ? typeof(bool)
            : Visit(expression.Condition, state);

        if (conditionType != typeof(bool))
            throw new Exception($"Expected boolean expression. {expression.Condition?.Range.Start}");

        return Visit(expression.Body, state);
    }

    public override Type VisitIf(IfExpression expression, object state)
    {
        if (Visit(expression.Condition, state) != typeof(bool))
            throw new Exception($"Expected boolean expression. {expression.Condition.Range.Start}");

        var type = Visit(expression.ThenBranch, state);

        var elseType = expression.ElseBranch is not null
            ? Visit(expression.ElseBranch, state)
            : type;

        if (type != elseType)
            throw new Exception(
                $"Expected expression of type {type}, got type {elseType}. {expression.ElseBranch?.Range.Start}");

        return type;
    }

    public override Type VisitInvocation(InvocationExpression expression, object state)
    {
        if (expression.Function is not ConstantExpression constant)
            throw new Exception("Invalid function");

        var function = GetFunction(expression, constant.Lexeme);

        for (var i = 0; i < function.ArgumentTypes.Length; i++)
            if (Visit(expression.Arguments[i], state) != function.ArgumentTypes[i])
                throw new Exception($"Wrong argument type in function {function.Name}. {i}");

        return function.Type;
    }

    public override Type VisitParenthesized(ParenthesizedExpression expression, object state)
    {
        return Visit(expression.Expression, state);
    }

    public override Type VisitRepeat(RepeatExpression expression, object state)
    {
        if (Visit(expression.Condition, state) != typeof(bool))
            throw new Exception($"Expected boolean expression. {expression.Condition.Range.Start}");

        return Visit(expression.Body, state);
    }

    public override Type VisitScope(ScopeExpression expression, object state) 
    {
        var type = typeof(Empty);

        foreach (var innerExpression in expression.InnerExpressions)
            type = Visit(innerExpression, state);

        return type;
    }

    public override Type VisitVariable(VariableExpression expression, object state)
    {
        var name = expression.NameToken.Lexeme;

        var variable = GetVariable(expression, name);

        var assignmentType = variable.Type;

        if (expression.AssignmentExpression is not null)
            assignmentType = Visit(expression.AssignmentExpression, state);

        if (variable.Type != assignmentType)
            throw new Exception(
                $"expression of type {assignmentType} is not assignable. {expression.AssignmentExpression?.Range.Start}");

        return variable.Type;
    }

    public override Type VisitWhile(WhileExpression expression, object state)
    {
        if (Visit(expression.Condition, state) != typeof(bool))
            throw new Exception($"Expected boolean expression. {expression.Condition.Range.Start}");

        return Visit(expression.Body, state);
    }
}