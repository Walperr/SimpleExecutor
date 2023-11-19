using LanguageInterpreter.Common;
using LanguageParser;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

namespace LanguageInterpreter.Execution;

public sealed class TypeResolver : ExpressionVisitor<Result<SyntaxException, Type>>
{
    private readonly ScopeNode _scopeTree;

    public TypeResolver(ScopeNode scopeTree)
    {
        _scopeTree = scopeTree;
    }

    public static Result<SyntaxException, Type> Resolve(ScopeNode tree)
    {
        return new TypeResolver(tree).Resolve();
    }

    public Result<SyntaxException, Type> Resolve()
    {
        return Visit(_scopeTree.Scope);
    }

    public override Result<SyntaxException, Type> VisitConstant(ConstantExpression expression)
    {
        if (expression.IsBool)
            return typeof(bool);
        if (expression.IsNumber)
            return typeof(double);
        if (expression.IsString)
            return typeof(string);

        var variable = GetVariable(expression, expression.Lexeme);

        if (variable is null)
            return new UndeclaredVariableException(expression.Lexeme, expression.Range);

        return variable.Type;
    }

    public override Result<SyntaxException, Type> VisitBinary(BinaryExpression expression)
    {
        var leftType = Visit(expression.Left);
        if (leftType.IsError)
            return leftType.Error;

        var rightType = Visit(expression.Right);
        if (rightType.IsError)
            return rightType.Error;

        switch (expression.Kind)
        {
            case SyntaxKind.DivideExpression:
            case SyntaxKind.MultiplyExpression:
            case SyntaxKind.SubtractExpression:

                if (leftType != typeof(double) || rightType != typeof(double))
                    return new IncompatibleOperandsException(expression.Operator);

                return typeof(double);

            case SyntaxKind.AddExpression:

                if (leftType == typeof(string) || rightType == typeof(string))
                    return typeof(string);

                if (leftType == typeof(bool) || rightType == typeof(bool))
                    return new UnexpectedOperandsException(expression.Operator, typeof(bool));

                if (leftType == typeof(double) && rightType == typeof(double))
                    return typeof(double);

                return new ExpectedOtherTypeException(expression.Right, rightType.Value, leftType.Value);

            case SyntaxKind.RelationalExpression:

                if (leftType != typeof(double) || rightType != typeof(double))
                    return new IncompatibleOperandsException(expression.Operator, typeof(double));

                return typeof(bool);

            case SyntaxKind.AndExpression:
            case SyntaxKind.OrExpression:

                if (leftType != typeof(bool) || rightType != typeof(bool))
                    return new IncompatibleOperandsException(expression.Operator, typeof(bool));

                return typeof(bool);

            case SyntaxKind.AssignmentExpression:
                if (expression.Left is ConstantExpression constant)
                {
                    if (constant.IsBool || constant.IsNumber || constant.IsString)
                        return new InterpreterException($"Expected variable, but got {constant.Lexeme}",
                            constant.Range);

                    var variable = GetVariable(expression, constant.Lexeme);

                    if (variable is null)
                        return new UndeclaredVariableException(constant.Lexeme, expression.Range);

                    if (variable.Type != rightType)
                        return new ExpectedOtherTypeException(expression.Right, rightType.Value, variable.Type);

                    return variable.Type;
                }

                break;
            case SyntaxKind.EqualityExpression:
                return typeof(bool);
        }

        return new InterpreterException($"Unknown operator {expression.Operator.Lexeme}", expression.Range);
    }

    public override Result<SyntaxException, Type> VisitFor(ForExpression expression)
    {
        var conditionType = expression.Condition is null
            ? typeof(bool)
            : Visit(expression.Condition);

        if (conditionType.IsError)
            return conditionType.Error;

        if (conditionType != typeof(bool))
            return new ExpectedOtherTypeException(expression.Condition!, conditionType.Value, typeof(bool));

        return Visit(expression.Body);
    }

    public override Result<SyntaxException, Type> VisitIf(IfExpression expression)
    {
        var conditionType = Visit(expression.Condition);

        if (conditionType.IsError)
            return conditionType.Error;

        if (conditionType != typeof(bool))
            return new ExpectedOtherTypeException(expression.Condition, conditionType.Value, typeof(bool));

        var type = Visit(expression.ThenBranch);

        if (type.IsError)
            return type.Error;

        var elseType = expression.ElseBranch is not null
            ? Visit(expression.ElseBranch)
            : type;

        if (elseType.IsError)
            return elseType.Error;

        if (type != elseType)
            return new ExpectedOtherTypeException(expression.ElseBranch!, elseType.Value, type.Value);

        return type;
    }

    public override Result<SyntaxException, Type> VisitInvocation(InvocationExpression expression)
    {
        if (expression.Function is not ConstantExpression constant)
            return new InterpreterException("expected function name", expression.Function.Range);

        var function = GetFunction(expression, constant.Lexeme);

        if (function is null)
            return new UndeclaredFunctionException(constant.Lexeme, constant.Range);

        for (var i = 0; i < function.ArgumentTypes.Length; i++)
        {
            var argType = Visit(expression.Arguments[i]);

            if (argType.IsError)
                return argType.Error;

            if (argType != function.ArgumentTypes[i])
                return new WrongFunctionArgumentException(argType.Value, function.Name, expression.Arguments[i].Range);
        }

        return function.ReturnType;
    }

    public override Result<SyntaxException, Type> VisitParenthesized(ParenthesizedExpression expression)
    {
        return Visit(expression.Expression);
    }

    public override Result<SyntaxException, Type> VisitRepeat(RepeatExpression expression)
    {
        var conditionType = Visit(expression.Condition);

        if (conditionType.IsError)
            return conditionType.Error;

        if (conditionType != typeof(bool))
            return new ExpectedOtherTypeException(expression.Condition, conditionType.Value, typeof(bool));

        return Visit(expression.Body);
    }

    public override Result<SyntaxException, Type> VisitScope(ScopeExpression expression)
    {
        var type = typeof(Empty);

        foreach (var innerExpression in expression.InnerExpressions)
        {
            var result = Visit(innerExpression);

            if (result.IsError)
                return result.Error;

            type = result.Value;
        }

        return type;
    }

    public override Result<SyntaxException, Type> VisitVariable(VariableExpression expression)
    {
        var name = expression.NameToken.Lexeme;

        var variable = GetVariable(expression, name);

        if (variable is null)
            return new UndeclaredVariableException(name, expression.Range);

        var assignmentType = variable.Type;

        if (expression.AssignmentExpression is not null)
        {
            var result = Visit(expression.AssignmentExpression);

            if (result.IsError)
                return result.Error;

            assignmentType = result.Value;
        }

        if (variable.Type != assignmentType)
            return new ExpectedOtherTypeException(expression.AssignmentExpression!, assignmentType, variable.Type);

        return variable.Type;
    }

    public override Result<SyntaxException, Type> VisitWhile(WhileExpression expression)
    {
        var conditionType = Visit(expression.Condition);

        if (conditionType.IsError)
            return conditionType.Error;

        if (conditionType != typeof(bool))
            return new ExpectedOtherTypeException(expression.Condition, conditionType.Value, typeof(bool));

        return Visit(expression.Body);
    }

    private Variable? GetVariable(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _scopeTree.Scope);

        var node = _scopeTree.FindDescendant(node => node.Scope == scope);

        var variable = node?.GetVariableIncludingAncestors(name);

        return variable;
    }

    private FunctionBase? GetFunction(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _scopeTree.Scope);

        var node = _scopeTree.FindDescendant(node => node.Scope == scope);

        var function = node?.GetFunctionIncludingAncestors(name);

        return function;
    }
}