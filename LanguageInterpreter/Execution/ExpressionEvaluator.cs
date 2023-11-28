using System.Collections;
using System.Globalization;
using LanguageInterpreter.Common;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

// ReSharper disable HeapView.BoxingAllocation

namespace LanguageInterpreter.Execution;

public sealed class ExpressionEvaluator : ExpressionVisitor<object?, CancellationToken>
{
    private readonly List<SyntaxException> _errors = new();
    private readonly ScopeNode _rootScope;

    internal ExpressionEvaluator(ScopeNode rootScope)
    {
        _rootScope = rootScope;
    }

    public static Result<SyntaxException, object> Evaluate(ScopeNode root, CancellationToken? token = null)
    {
        return new ExpressionEvaluator(root).Evaluate(token ?? CancellationToken.None);
    }

    public Result<SyntaxException, object> Evaluate(CancellationToken token)
    {
        try
        {
            var type = Visit(_rootScope.Scope, token);

            if (type is null)
                return _errors.First();

            return type;
        }
        catch (Exception e)
        {
            return new UnhandledInterpreterException(e);
        }
    }

    public override object? VisitConstant(ConstantExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

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

    public override object? VisitBinary(BinaryExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        if (expression.Kind is SyntaxKind.AssignmentExpression)
        {
            object? result;
            switch (expression.Left)
            {
                case ConstantExpression constant:
                    var variable = GetVariable(expression, constant.Lexeme);

                    if (variable is null || !variable.IsDeclared)
                    {
                        _errors.Add(new UndeclaredVariableException(constant.Lexeme, expression.Range));
                        return null;
                    }

                    result = Visit(expression.Right, token);
                    if (result is null)
                        return null;

                    variable.SetValue(result);

                    return variable.Value;
                case ElementAccessExpression indexer when indexer.Elements.Length != 1:
                    _errors.Add(new InterpreterException("Wrong arguments count in array indexer", indexer.Range));
                    return null;
                case ElementAccessExpression indexer:
                {
                    if (Visit(indexer.Elements[0], token) is not double index)
                    {
                        _errors.Add(new InterpreterException("Parameter type mismatch 'index', expected number",
                            indexer.Elements[0].Range));
                        return null;
                    }
                
                    var value = Visit(indexer.Expression, token);

                    if (value is null)
                        return null;

                    result = Visit(expression.Right, token);
                    if (result is null)
                        return null;

                    switch (value)
                    {
                        case double[] doubles:
                            doubles[(long)index] = (double)result;
                            return result;
                        case string[] strings:
                            strings[(long)index] = (string)result;
                            return result;
                        case bool[] bools:
                            bools[(long)index] = (bool)result;
                            return result;
                        case not null when value.GetType().IsAssignableTo(typeof(Array[])):
                            ((Array[])value)[(long)index] = (Array)result;
                            return result;
                        default:
                            _errors.Add(new InterpreterException("Cannot use indexer for non array expressions", indexer.Range));
                            return null;
                    }
                }
                default:
                    _errors.Add(new InterpreterException("Cannot assign value to expression", expression.Range));
                    return null;
            }
        }

        var left = Visit(expression.Left, token);

        if (left is null)
            return null;

        var right = Visit(expression.Right, token);

        if (right is null)
            return null;

        switch (expression.Kind)
        {
            case SyntaxKind.DivideExpression:
                return (double)left / (double)right;
            case SyntaxKind.RemainderExpression:
                return (double)left % (double)right;
            case SyntaxKind.MultiplyExpression:
                return (double)left * (double)right;
            case SyntaxKind.SubtractExpression:
                return (double)left - (double)right;
            case SyntaxKind.AddExpression:
            {
                if (left is double ld && right is double rd)
                    return ld + rd;

                return left + right.ToString();
            }
            case SyntaxKind.RelationalExpression:
                return expression.Operator.Lexeme switch
                {
                    ">" => (double)left > (double)right,
                    "<" => (double)left < (double)right,
                    ">=" => (double)left >= (double)right,
                    "<=" => (double)left <= (double)right,
                    _ => throw new UnexpectedOperatorException(expression.Operator.Lexeme, expression.Operator.Range)
                };
            case SyntaxKind.AndExpression:
                return (bool)left && (bool)right;
            case SyntaxKind.OrExpression:
                return (bool)left || (bool)right;
            case SyntaxKind.EqualityExpression:
                return left.Equals(right);
        }

        _errors.Add(new InterpreterException("Unknown operation", expression.Range));
        return null;
    }

    public override object? VisitFor(ForExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        if (expression.Initialization is not null)
        {
            var initialization = Visit(expression.Initialization, token);

            if (initialization is null)
                return null;
        }

        object value = Empty.Instance;

        while (true)
        {
            var condition = Condition(token);

            if (condition is null)
                return null;

            if (!condition.Value)
                break;

            var result = Visit(expression.Body, token);

            if (result is null)
                return null;

            value = result;

            if (expression.Step is null)
                continue;

            result = Visit(expression.Step, token);

            if (result is null)
                return result;
        }

        return value;

        bool? Condition(CancellationToken t)
        {
            if (expression.Condition is null)
                return true;

            var result = Visit(expression.Condition, t);

            return (bool?)result;
        }
    }

    public override object? VisitIf(IfExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        var condition = Visit(expression.Condition, token);

        if (condition is null)
            return null;

        if ((bool)condition)
            return Visit(expression.ThenBranch, token);

        return expression.ElseBranch is null
            ? Empty.Instance
            : Visit(expression.ElseBranch, token);
    }

    public override object? VisitInvocation(InvocationExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

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

        var args = expression.Arguments.Select(a => Visit(a, token)).ToArray();

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

    public override object? VisitParenthesized(ParenthesizedExpression expression, CancellationToken token)
    {
        return Visit(expression.Expression, token);
    }

    public override object? VisitRepeat(RepeatExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        object? value = Empty.Instance;

        if (Visit(expression.CountExpression, token) is not double count)
            return null;

        int i = 0;

        while (true)
        {
            if (i >= count)
                break;
            i++;
            
            value = Visit(expression.Body, token);
            if (value is null)
                return null;
        }

        return value;
    }

    public override object? VisitScope(ScopeExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        object value = Empty.Instance;

        foreach (var innerExpression in expression.InnerExpressions)
        {
            var result = Visit(innerExpression, token);
            if (result is null)
                return null;

            value = result;
        }

        return value;
    }

    public override object? VisitVariable(VariableExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

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

        var result = Visit(expression.AssignmentExpression, token);

        if (result is null)
            return null;

        variable.SetValue(result);

        return variable.Value ?? Empty.Instance;
    }

    public override object? VisitWhile(WhileExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        object? value = Empty.Instance;

        while (true)
        {
            var condition = Visit(expression.Condition, token);

            if (condition is null)
                return null;

            if (!(bool)condition)
                break;

            value = Visit(expression.Body, token);

            if (value is null)
                return null;
        }

        return value;
    }

    public override object? VisitPrefixUnary(PrefixUnaryExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }
        
        var value = Visit(expression.Operand, token);
        
        if (value is null)
            return null;

        switch (expression.Kind)
        {
            case SyntaxKind.PreIncrementExpression:
            case SyntaxKind.PreDecrementExpression:
                return VisitPrefixIncrementDecrement(expression, token);
            case SyntaxKind.UnaryPlusExpression:
                return value;
            case SyntaxKind.UnaryMinusExpression:
                return -(double)value;
            default:
                _errors.Add(new InterpreterException($"Unknown prefix operator", expression.Range));
                return null;
        }
    }

    private object? VisitPrefixIncrementDecrement(PrefixUnaryExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }
        
        if (expression.Operand is not ConstantExpression constant)
        {
            _errors.Add(new InterpreterException("Expected constant expression", expression.Operand.Range));
            return null;
        }

        var variable = GetVariable(expression, constant.Lexeme);

        if (variable is null || !variable.IsDeclared)
        {
            _errors.Add(new UndeclaredVariableException(constant.Lexeme, constant.Range));
            return null;
        }
        
        if (variable.IsUnset)
        {
            _errors.Add(new UninitializedVariableException(constant.Lexeme, constant.Range));
            return null;
        }

        var value = (double)variable.Value;

        switch (expression.Kind)
        {
            case SyntaxKind.PreIncrementExpression:
                variable.SetValue(value + 1);
                return variable.Value;
            case SyntaxKind.PreDecrementExpression:
                variable.SetValue(value - 1);
                return variable.Value;
            default:
                _errors.Add(new InterpreterException($"Unknown prefix operator", expression.Range));
                return null;
        }
    }

    public override object? VisitPostfixUnary(PostfixUnaryExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }
        
        var variable = GetVariable(expression, expression.Operand.Lexeme);

        if (variable is null || !variable.IsDeclared)
        {
            _errors.Add(new UndeclaredVariableException(expression.Operand.Lexeme, expression.Range));
            return null;
        }

        if (variable.IsUnset)
        {
            _errors.Add(new UninitializedVariableException(expression.Operand.Lexeme, expression.Range));
            return null;
        }

        var value = (double) variable.Value;

        switch (expression.Kind)
        {
            case SyntaxKind.PostIncrementExpression:
                variable.SetValue(value + 1);
                break;
            case SyntaxKind.PostDecrementExpression:
                variable.SetValue(value - 1);
                break;
            default:
                _errors.Add(new InterpreterException($"Unknown postfix operator", expression.Range));
                return null;
        }

        return value;
    }

    public override object? VisitElementAccess(ElementAccessExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        if (expression.IsArrayDeclaration)
            return Empty.Instance;

        if (expression.IsArrayConstructor)
        {
            if (expression.Elements.Length != 1)
            {
                _errors.Add(new InterpreterException("Array constructor should have only 1 size argument",
                    expression.Range));
                return null;
            }

            if (Visit(expression.Elements[0], token) is not double size)
            {
                _errors.Add(new InterpreterException("Parameter type mismatch 'size', expected number",
                    expression.Elements[0].Range));
                return null;
            }

            var typeResult = TypeResolver.Resolve(_rootScope, expression, token);

            if (typeResult.IsError)
            {
                _errors.Add(typeResult.Error);
                return null;
            }

            if (typeResult.Value == typeof(double[]))
                return new double[(long)size];
            if (typeResult.Value == typeof(string[]))
                return new string[(long)size];
            if (typeResult.Value == typeof(bool[]))
                return new bool[(long)size];
            if (typeResult.Value == typeof(Array[]))
                return new Array[(long)size];

            _errors.Add(new InterpreterException("Unknown type", expression.Expression.Range));
            return null;
        }

        if (Visit(expression.Expression, token) is not Array array)
        {
            _errors.Add(new InterpreterException("Cannot index non array expression", expression.Expression.Range));
            return null;
        }
            
        if (Visit(expression.Elements[0], token) is not double index)
        {
            _errors.Add(new InterpreterException("Parameter type mismatch 'index', expected number",
                expression.Elements[0].Range));
            return null;
        }

        return ((IList)array)[(int)index];
    }

    public override object? VisitArrayInitialization(ArrayInitializationExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        if (expression.Elements.IsDefaultOrEmpty)
        {
            _errors.Add(new InterpreterException("Array initializer cannot be empty", expression.Range));
            return null;
        }

        var first = Visit(expression.Elements[0], token);

        IList? array = first switch
        {
            double => new double[expression.Elements.Length],
            string => new string[expression.Elements.Length],
            bool => new bool[expression.Elements.Length],
            Array => new Array[expression.Elements.Length],
            _ => null
        };

        if (array is null)
        {
            _errors.Add(new InterpreterException("Unkonwn type", expression.Range));
            return null;
        }

        array[0] = first;

        for (int i = 1; i < array.Count; i++) 
            array[i] = Visit(expression.Elements[i], token);

        return array;
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