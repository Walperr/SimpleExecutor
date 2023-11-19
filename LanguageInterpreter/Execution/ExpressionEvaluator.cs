using System.Globalization;
using LanguageInterpreter.Common;
using LanguageParser;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

// ReSharper disable HeapView.BoxingAllocation

namespace LanguageInterpreter.Execution;

public sealed class ExpressionEvaluator : ExpressionVisitor<Result<SyntaxException, object>>
{
    private readonly ScopeNode _rootScope;

    public ExpressionEvaluator(ScopeNode rootScope)
    {
        _rootScope = rootScope;
    }

    public static Result<SyntaxException, object> Evaluate(ScopeNode root)
    {
        return new ExpressionEvaluator(root).Evaluate();
    }

    public Result<SyntaxException, object> Evaluate()
    {
        return Visit(_rootScope.Scope);
    }

    public override Result<SyntaxException, object> VisitConstant(ConstantExpression expression)
    {
        if (expression.IsBool)
            return bool.Parse(expression.Lexeme);
        if (expression.IsNumber)
            return double.Parse(expression.Lexeme, NumberStyles.Float, CultureInfo.InvariantCulture);
        if (expression.IsString)
            return expression.Lexeme;

        var variable = GetVariable(expression, expression.Lexeme);

        if (variable is null || !variable.IsDeclared)
            return new UndeclaredVariableException(expression.Lexeme, expression.Range);

        if (variable.IsUnset)
            return new UninitializedVariableException(expression.Lexeme, expression.Range);

        return variable.Value;
    }

    public override Result<SyntaxException, object> VisitBinary(BinaryExpression expression)
    {
        if (expression.Kind is SyntaxKind.AssignmentExpression)
        {
            if (expression.Left is ConstantExpression constant)
            {
                var variable = GetVariable(expression, constant.Lexeme);

                if (variable is null || !variable.IsDeclared)
                    return new UndeclaredVariableException(constant.Lexeme, expression.Range);

                var result = Visit(expression.Right);
                if (result.IsError)
                    return result.Error;
                
                variable.SetValue(result.Value);

                return variable.Value;
            }

            return new InterpreterException("Cannot assign value to expression", expression.Range);
        }

        var left = Visit(expression.Left);

        if (left.IsError)
            return left.Error;

        var right = Visit(expression.Right);

        if (right.IsError)
            return right.Error;

        switch (expression.Kind)
        {
            case SyntaxKind.DivideExpression:
                return (double) left.Value / (double) right.Value;
            case SyntaxKind.MultiplyExpression:
                return (double) left.Value * (double) right.Value;
            case SyntaxKind.SubtractExpression:
                return (double) left.Value - (double) right.Value;
            case SyntaxKind.AddExpression:
            {
                if (left.Value is double ld && right.Value is double rd)
                    return ld + rd;

                return left.Value + right.Value.ToString();
            }
            case SyntaxKind.RelationalExpression:
                return expression.Operator.Lexeme switch
                {
                    ">" => (double) left.Value > (double) right.Value,
                    "<" => (double) left.Value < (double) right.Value,
                    ">=" => (double) left.Value >= (double) right.Value,
                    "<=" => (double) left.Value <= (double) right.Value,
                    _ => new UnexpectedOperatorException(expression.Operator.Lexeme, expression.Operator.Range)
                };
            case SyntaxKind.AndExpression:
                return (bool) left.Value && (bool) right.Value;
            case SyntaxKind.OrExpression:
                return (bool) left.Value || (bool) right.Value;
            case SyntaxKind.EqualityExpression:
                return left.Value.Equals(right.Value);
        }

        return new InterpreterException("Unknown operation", expression.Range);
    }

    public override Result<SyntaxException, object> VisitFor(ForExpression expression)
    {
        if (expression.Initialization is not null)
        {
            var initialization = Visit(expression.Initialization);

            if (initialization.IsError)
                return initialization.Error;
        }

        object value = Empty.Instance;

        while (true)
        {
            var condition = Condition();

            if (condition.IsError)
                return condition.Error;

            if (!condition.Value)
                break;

            var result = Visit(expression.Body);

            if (result.IsError)
                return result.Error;

            value = result.Value;

            if (expression.Step is null)
                continue;

            result = Visit(expression.Step);

            if (result.IsError)
                return result.Error;
        }

        return value;

        Result<SyntaxException, bool> Condition()
        {
            if (expression.Condition is null)
                return true;

            var result = Visit(expression.Condition);

            if (result.IsError)
                return result.Error;

            return (bool) result.Value;
        }
    }

    public override Result<SyntaxException, object> VisitIf(IfExpression expression)
    {
        var condition = Visit(expression.Condition);

        if (condition.IsError)
            return condition.Error;

        if ((bool) condition)
            return Visit(expression.ThenBranch);

        return expression.ElseBranch is null
            ? Empty.Instance
            : Visit(expression.ElseBranch);
    }

    public override Result<SyntaxException, object> VisitInvocation(InvocationExpression expression)
    {
        if (expression.Function is not ConstantExpression constant)
            return new InterpreterException("Expected function name", expression.Function.Range);

        var function = GetFunction(expression, constant.Lexeme);

        if (function is null)
            return new UndeclaredFunctionException(constant.Lexeme, constant.Range);

        var args = expression.Arguments.Select(Visit).ToArray();

        if (args.Any(a => a.IsError))
            return args.FirstOrDefault(a => a.IsError).Error!;

        switch (function)
        {
            case Function action:
                action.Invoke(args.Select(a => a.Value)!);
                return Empty.Instance;
            case Function<string> sFunction:
                return sFunction.Invoke(args.Select(a => a.Value)!);
            case Function<bool> bFunction:
                return bFunction.Invoke(args.Select(a => a.Value)!);
            case Function<double> dFunction:
                return dFunction.Invoke(args.Select(a => a.Value)!);
            default:
                return new InterpreterException($"Unknown return type of function {function.Name}", expression.Range);
        }
    }

    public override Result<SyntaxException, object> VisitParenthesized(ParenthesizedExpression expression)
    {
        return Visit(expression.Expression);
    }

    public override Result<SyntaxException, object> VisitRepeat(RepeatExpression expression)
    {
        Result<SyntaxException, object> value;

        while (true)
        {
            value = Visit(expression.Body);
            if (value.IsError)
                return value.Error;

            var condition = Visit(expression.Condition);
            if (condition.IsError)
                return condition.Error;

            if ((bool) condition)
                break;
        }

        return value;
    }

    public override Result<SyntaxException, object> VisitScope(ScopeExpression expression)
    {
        object value = Empty.Instance;

        foreach (var innerExpression in expression.InnerExpressions)
        {
            var result = Visit(innerExpression);
            if (result.IsError)
                return result.Error;

            value = result.Value;
        }

        return value;
    }

    public override Result<SyntaxException, object> VisitVariable(VariableExpression expression)
    {
        var name = expression.NameToken.Lexeme;

        var variable = GetVariable(expression, name);

        if (variable is null)
            return new UndeclaredVariableException(name, expression.Range);

        variable.IsDeclared = true;

        if (expression.AssignmentExpression is not null)
        {
            var result = Visit(expression.AssignmentExpression);
            if (result.IsError)
                return result.Error;

            variable.SetValue(result.Value);
        }

        return variable.Value ?? Empty.Instance;
    }

    public override Result<SyntaxException, object> VisitWhile(WhileExpression expression)
    {
        Result<SyntaxException, object> value = Empty.Instance;

        while (true)
        {
            var condition = Visit(expression.Condition);

            if (condition.IsError)
                return condition.Error;

            if (!(bool) condition)
                break;

            value = Visit(expression.Body);

            if (value.IsError)
                return value.Error;
        }

        return value;
    }

    private Variable? GetVariable(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _rootScope.Scope);

        var node = _rootScope.FindDescendant(node => node.Scope == scope);

        var variable = node?.GetVariableIncludingAncestors(name);

        return variable;
    }

    private FunctionBase? GetFunction(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _rootScope.Scope);

        var node = _rootScope.FindDescendant(node => node.Scope == scope);

        var function = node?.GetFunctionIncludingAncestors(name);

        return function;
    }
}