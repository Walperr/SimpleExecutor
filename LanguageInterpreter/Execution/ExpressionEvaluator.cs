using System.Globalization;
using LanguageParser;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

// ReSharper disable HeapView.BoxingAllocation

namespace LanguageInterpreter.Execution;

public sealed class ExpressionEvaluator : ExpressionVisitor<object, object>
{
    private readonly ScopeNode _rootScope;

    public ExpressionEvaluator(ScopeNode rootScope)
    {
        _rootScope = rootScope;
    }

    public static object Evaluate(ScopeNode root)
    {
        return new ExpressionEvaluator(root).Evaluate();
    }

    public object Evaluate()
    {
        return Visit(_rootScope.Scope, null!);
    }

    public override object VisitBinary(BinaryExpression expression, object state)
    {
        if (expression.Kind is SyntaxKind.AssignmentExpression)
        {
            if (expression.Left is ConstantExpression constant)
            {
                var variable = GetVariable(expression, constant.Lexeme);

                if (!variable.IsDeclared)
                    throw new Exception($"variable {variable.Name} is undeclared");

                variable.SetValue(Visit(expression.Right, state));

                return variable.Value;
            }

            throw new Exception("Cannot assign value to expression");
        }
        
        var left = Visit(expression.Left, state);
        var right = Visit(expression.Right, state);

        switch (expression.Kind)
        {
            case SyntaxKind.DivideExpression:
                return (double)left / (double)right;
            case SyntaxKind.MultiplyExpression:
                return (double)left * (double)right;
            case SyntaxKind.SubtractExpression:
                return (double)left - (double)right;
            case SyntaxKind.AddExpression:
            {
                if (left is double ld && right is double rd)
                    return ld + rd;

                return left.ToString() + right.ToString();
            }
            case SyntaxKind.RelationalExpression:
                return expression.Operator.Lexeme switch
                {
                    ">" => (double)left > (double)right,
                    "<" => (double)left < (double)right,
                    ">=" => (double)left >= (double)right,
                    "<=" => (double)left <= (double)right,
                    _ => throw new Exception()
                };
            case SyntaxKind.AndExpression:
                return (bool)left && (bool)right;
            case SyntaxKind.OrExpression:
                return (bool)left || (bool)right;
            case SyntaxKind.EqualityExpression:
                return left.Equals(right);
        }

        throw new Exception("unknown operation");
    }

    public override object VisitConstant(ConstantExpression expression, object state)
    {
        if (expression.IsBool)
            return bool.Parse(expression.Lexeme);
        if (expression.IsNumber)
            return double.Parse(expression.Lexeme, NumberStyles.Float, CultureInfo.InvariantCulture);
        if (expression.IsString)
            return expression.Lexeme;

        var variable = GetVariable(expression, expression.Lexeme);

        if (variable is null || !variable.IsDeclared)
            throw new Exception($"undeclared variable {expression.Lexeme}");

        if (variable.IsUnset)
            throw new Exception($"cannot use uninitialized variable {expression.Lexeme}");

        return variable.Value;
    }

    private Variable GetVariable(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _rootScope.Scope);

        var node = _rootScope.FindDescendant(node => node.Scope == scope);

        var variable = node?.GetVariableIncludingAncestors(name);

        if (variable is null)
            throw new Exception($"undeclarated variable {name}");

        return variable;
    }

    private FunctionBase GetFunction(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _rootScope.Scope);

        var node = _rootScope.FindDescendant(node => node.Scope == scope);

        var function = node?.GetFunctionIncludingAncestors(name);

        if (function is null)
            throw new Exception($"undeclarated function {name}");

        return function;
    }


    public override object VisitFor(ForExpression expression, object state)
    {
        if (expression.Initialization is not null)
            Visit(expression.Initialization, state);

        object value = Empty.Instance;

        while (Condition())
        {
            value = Visit(expression.Body, state);

            if (expression.Step is not null)
                Visit(expression.Step, state);
        }

        return value;

        bool Condition()
        {
            return expression.Condition is null || (bool)Visit(expression.Condition, state);
        }
    }

    public override object VisitIf(IfExpression expression, object state)
    {
        if ((bool)Visit(expression.Condition, state))
            return Visit(expression.ThenBranch, state);

        return expression.ElseBranch is null
            ? Empty.Instance
            : Visit(expression.ElseBranch, state);
    }

    public override object VisitInvocation(InvocationExpression expression, object state)
    {
        if (expression.Function is not ConstantExpression constant)
            throw new Exception("Invalid function");

        var function = GetFunction(expression, constant.Lexeme);

        var args = expression.Arguments.Select(argument => Visit(argument, state)).ToArray();

        switch (function)
        {
            case Function action:
                action.Invoke(args);
                return Empty.Instance;
            case Function<string> sFunction:
                return sFunction.Invoke(args);
            case Function<bool> bFunction:
                return bFunction.Invoke(args);
            case Function<double> dFunction:
                return dFunction.Invoke(args);
            default:
                throw new Exception($"Unknown return type for function {function.Name}");
        }
    }

    public override object VisitParenthesized(ParenthesizedExpression expression, object state)
    {
        return Visit(expression.Expression, state);
    }

    public override object VisitRepeat(RepeatExpression expression, object state)
    {
        object value;

        do
        {
            value = Visit(expression.Body, state);
        } while (!(bool)Visit(expression.Condition, state));

        return value;
    }

    public override object VisitScope(ScopeExpression expression, object state)
    {
        object value = Empty.Instance;

        foreach (var innerExpression in expression.InnerExpressions)
            value = Visit(innerExpression, state);

        return value;
    }

    public override object VisitVariable(VariableExpression expression, object state)
    {
        var name = expression.NameToken.Lexeme;

        var variable = GetVariable(expression, name);

        variable.IsDeclared = true;

        if (expression.AssignmentExpression is not null)
            variable.SetValue(Visit(expression.AssignmentExpression, state));

        return variable.Value ?? Empty.Instance;
    }

    public override object VisitWhile(WhileExpression expression, object state)
    {
        object value = Empty.Instance;

        while ((bool)Visit(expression.Condition, state))
            value = Visit(expression.Body, state);

        return value;
    }
}