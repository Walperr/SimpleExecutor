using System.Globalization;
using LanguageInterpreter.Common;
using LanguageParser;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

// ReSharper disable HeapView.BoxingAllocation

namespace LanguageInterpreter.Execution;

public sealed class ExpressionEvaluator : ExpressionVisitor<object?>
{
    private readonly List<SyntaxException> _errors = new();
    private readonly ScopeNode _rootScope;

    internal ExpressionEvaluator(ScopeNode rootScope)
    {
        _rootScope = rootScope;
    }

    public static Result<SyntaxException, object> Evaluate(ScopeNode root)
    {
        return new ExpressionEvaluator(root).Evaluate();
    }

    public Result<SyntaxException, object> Evaluate()
    {
        try
        {
            var type = Visit(_rootScope.Scope);

            if (type is null)
                return _errors.First();

            return type;
        }
        catch (Exception e)
        {
            return new UnhandledInterpreterException(e);
        }
    }

    public override object? VisitConstant(ConstantExpression expression)
    {
        if (expression.IsBool)
            return bool.Parse(expression.Lexeme);
        if (expression.IsNumber)
            return double.Parse(expression.Lexeme, NumberStyles.Float, CultureInfo.InvariantCulture);
        if (expression.IsString)
            return expression.Lexeme;

        var variable = GetVariable(expression, expression.Lexeme);

        if (variable is null || !variable.IsDeclared)
        {
            _errors.Add(new UndeclaredVariableException(expression.Lexeme, expression.Range));
            return null;
        }

        if (variable.IsUnset)
        {
            _errors.Add(new UninitializedVariableException(expression.Lexeme, expression.Range));
            return null;
        }

        return variable.Value;
    }

    public override object? VisitBinary(BinaryExpression expression)
    {
        if (expression.Kind is SyntaxKind.AssignmentExpression)
        {
            if (expression.Left is ConstantExpression constant)
            {
                var variable = GetVariable(expression, constant.Lexeme);

                if (variable is null || !variable.IsDeclared)
                {
                    _errors.Add(new UndeclaredVariableException(constant.Lexeme, expression.Range));
                    return null;
                }

                var result = Visit(expression.Right);
                if (result is null)
                    return null;

                variable.SetValue(result);

                return variable.Value;
            }

            _errors.Add(new InterpreterException("Cannot assign value to expression", expression.Range));
            return null;
        }

        var left = Visit(expression.Left);

        if (left is null)
            return null;

        var right = Visit(expression.Right);

        if (right is null)
            return null;

        switch (expression.Kind)
        {
            case SyntaxKind.DivideExpression:
                return (double) left / (double) right;
            case SyntaxKind.MultiplyExpression:
                return (double) left * (double) right;
            case SyntaxKind.SubtractExpression:
                return (double) left - (double) right;
            case SyntaxKind.AddExpression:
            {
                if (left is double ld && right is double rd)
                    return ld + rd;

                return left + right.ToString();
            }
            case SyntaxKind.RelationalExpression:
                return expression.Operator.Lexeme switch
                {
                    ">" => (double) left > (double) right,
                    "<" => (double) left < (double) right,
                    ">=" => (double) left >= (double) right,
                    "<=" => (double) left <= (double) right,
                    _ => throw new UnexpectedOperatorException(expression.Operator.Lexeme, expression.Operator.Range)
                };
            case SyntaxKind.AndExpression:
                return (bool) left && (bool) right;
            case SyntaxKind.OrExpression:
                return (bool) left || (bool) right;
            case SyntaxKind.EqualityExpression:
                return left.Equals(right);
        }

        _errors.Add(new InterpreterException("Unknown operation", expression.Range));
        return null;
    }

    public override object? VisitFor(ForExpression expression)
    {
        if (expression.Initialization is not null)
        {
            var initialization = Visit(expression.Initialization);

            if (initialization is null)
                return null;
        }

        object value = Empty.Instance;

        while (true)
        {
            var condition = Condition();

            if (condition is null)
                return null;

            if (!condition.Value)
                break;

            var result = Visit(expression.Body);

            if (result is null)
                return null;

            value = result;

            if (expression.Step is null)
                continue;

            result = Visit(expression.Step);

            if (result is null)
                return result;
        }

        return value;

        bool? Condition()
        {
            if (expression.Condition is null)
                return true;

            var result = Visit(expression.Condition);

            return (bool?) result;
        }
    }

    public override object? VisitIf(IfExpression expression)
    {
        var condition = Visit(expression.Condition);

        if (condition is null)
            return null;

        if ((bool) condition)
            return Visit(expression.ThenBranch);

        return expression.ElseBranch is null
            ? Empty.Instance
            : Visit(expression.ElseBranch);
    }

    public override object? VisitInvocation(InvocationExpression expression)
    {
        if (expression.Function is not ConstantExpression constant)
        {
            _errors.Add(new InterpreterException("Expected function name", expression.Function.Range));
            return null;
        }

        var function = GetFunction(expression, constant.Lexeme);

        if (function is null)
        {
            _errors.Add(new UndeclaredFunctionException(constant.Lexeme, constant.Range));
            return null;
        }

        var args = expression.Arguments.Select(Visit).ToArray();

        if (args.Any(a => a is null))
            return null;

        switch (function)
        {
            case Function action:
                action.Invoke(args!);
                return Empty.Instance;
            case Function<string> sFunction:
                return sFunction.Invoke(args!);
            case Function<bool> bFunction:
                return bFunction.Invoke(args!);
            case Function<double> dFunction:
                return dFunction.Invoke(args!);
            default:
                _errors.Add(new InterpreterException($"Unknown return type of function {function.Name}",
                    expression.Range));
                return null;
        }
    }

    public override object? VisitParenthesized(ParenthesizedExpression expression)
    {
        return Visit(expression.Expression);
    }

    public override object? VisitRepeat(RepeatExpression expression)
    {
        object? value;

        while (true)
        {
            value = Visit(expression.Body);
            if (value is null)
                return null;

            var condition = Visit(expression.Condition);
            if (condition is null)
                return null;

            if ((bool) condition)
                break;
        }

        return value;
    }

    public override object? VisitScope(ScopeExpression expression)
    {
        object value = Empty.Instance;

        foreach (var innerExpression in expression.InnerExpressions)
        {
            var result = Visit(innerExpression);
            if (result is null)
                return null;

            value = result;
        }

        return value;
    }

    public override object? VisitVariable(VariableExpression expression)
    {
        var name = expression.NameToken.Lexeme;

        var variable = GetVariable(expression, name);

        if (variable is null)
        {
            _errors.Add(new UndeclaredVariableException(name, expression.Range));
            return null;
        }

        variable.IsDeclared = true;

        if (expression.AssignmentExpression is null)
            return variable.Value ?? Empty.Instance;

        var result = Visit(expression.AssignmentExpression);

        if (result is null)
            return null;

        variable.SetValue(result);

        return variable.Value ?? Empty.Instance;
    }

    public override object? VisitWhile(WhileExpression expression)
    {
        object? value = Empty.Instance;

        while (true)
        {
            var condition = Visit(expression.Condition);

            if (condition is null)
                return null;

            if (!(bool) condition)
                break;

            value = Visit(expression.Body);

            if (value is null)
                return null;
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