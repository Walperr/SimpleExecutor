using LanguageInterpreter.Common;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

namespace LanguageInterpreter.Execution;

public sealed class TypeResolver : ExpressionVisitor<Type?, CancellationToken>
{
    private readonly List<SyntaxException> _errors = new();
    private readonly ScopeNode _rootScope;

    private TypeResolver(ScopeNode rootScope)
    {
        _rootScope = rootScope;
    }

    public static Result<SyntaxException, Type> Resolve(ScopeNode tree, CancellationToken? token = null)
    {
        return new TypeResolver(tree).Resolve(token ?? CancellationToken.None);
    }

    public static Result<SyntaxException, Type> Resolve(ScopeNode tree, ExpressionBase expression,
        CancellationToken? token = null)
    {
        return new TypeResolver(tree).Resolve(expression, token ?? CancellationToken.None);
    }

    private Result<SyntaxException, Type> Resolve(CancellationToken token)
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

    private Result<SyntaxException, Type> Resolve(ExpressionBase expression, CancellationToken token)
    {
        try
        {
            var type = Visit(expression, token);

            if (type is null)
                return _errors.First();

            return type;
        }
        catch (Exception e)
        {
            return new UnhandledInterpreterException(e);
        }
    }

    public override Type? VisitConstant(ConstantExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        if (expression.IsBool || expression.Kind == SyntaxKind.Bool)
            return typeof(bool);
        if (expression.IsNumber || expression.Kind == SyntaxKind.Number)
            return typeof(double);
        if (expression.IsString || expression.Kind == SyntaxKind.String)
            return typeof(string);

        var variable = GetVariable(expression, expression.Lexeme);

        if (variable is not null)
            return variable.Type;

        _errors.Add(new UndeclaredVariableException(expression.Lexeme, expression.Range));
        return null;
    }

    public override Type? VisitBinary(BinaryExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        var leftType = Visit(expression.Left, token);

        if (leftType is null)
            return null;

        var rightType = Visit(expression.Right, token);

        if (rightType is null)
            return null;

        switch (expression.Kind)
        {
            case SyntaxKind.DivideExpression:
            case SyntaxKind.MultiplyExpression:
            case SyntaxKind.SubtractExpression:
            case SyntaxKind.RemainderExpression:

                if (leftType == typeof(double) && rightType == typeof(double))
                    return typeof(double);

                _errors.Add(new IncompatibleOperandsException(expression.Operator));
                return null;

            case SyntaxKind.AddExpression:

                if (leftType == typeof(string) || rightType == typeof(string))
                    return typeof(string);

                if (leftType == typeof(double) && rightType == typeof(double))
                    return typeof(double);

                if (leftType == typeof(bool) || rightType == typeof(bool))
                {
                    _errors.Add(new UnexpectedOperandsException(expression.Operator, typeof(bool)));
                    return null;
                }

                _errors.Add(new ExpectedOtherTypeException(expression.Right, rightType, leftType));
                return null;

            case SyntaxKind.RelationalExpression:

                if (leftType == typeof(double) && rightType == typeof(double))
                    return typeof(bool);

                _errors.Add(new IncompatibleOperandsException(expression.Operator, typeof(double)));
                return null;

            case SyntaxKind.AndExpression:
            case SyntaxKind.OrExpression:

                if (leftType == typeof(bool) && rightType == typeof(bool))
                    return typeof(bool);

                _errors.Add(new IncompatibleOperandsException(expression.Operator, typeof(bool)));
                return null;

            case SyntaxKind.AssignmentExpression:
                if (expression.Left is ConstantExpression constant)
                {
                    if (constant.IsBool || constant.IsNumber || constant.IsString)
                    {
                        _errors.Add(new InterpreterException($"Expected variable, but got {constant.Lexeme}",
                            constant.Range));
                        return null;
                    }

                    var variable = GetVariable(expression, constant.Lexeme);

                    if (variable is null)
                    {
                        _errors.Add(new UndeclaredVariableException(constant.Lexeme, expression.Range));
                        return null;
                    }

                    if (variable.Type != rightType)
                    {
                        _errors.Add(new ExpectedOtherTypeException(expression.Right, rightType, variable.Type));
                        return null;
                    }

                    return variable.Type;
                }

                if (expression.Left is ElementAccessExpression {IsElementAccessor: true, Elements.Length: 1} indexerExpression)
                {
                    if (leftType == rightType)
                        return leftType;

                    _errors.Add(new IncompatibleOperandsException(expression.Operator, leftType));
                    return null;
                }

                break;
            case SyntaxKind.EqualityExpression:
                return typeof(bool);
        }

        _errors.Add(new InterpreterException($"Unknown operator {expression.Operator.Lexeme}", expression.Range));
        return null;
    }

    public override Type? VisitFor(ForExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        var conditionType = expression.Condition is null
            ? typeof(bool)
            : Visit(expression.Condition, token);

        if (conditionType is null)
            return null;

        if (conditionType != typeof(bool))
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Condition!, conditionType, typeof(bool)));
            return null;
        }

        return Visit(expression.Body, token);
    }

    public override Type? VisitIf(IfExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        var conditionType = Visit(expression.Condition, token);

        if (conditionType is null)
            return null;

        if (conditionType != typeof(bool))
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Condition, conditionType, typeof(bool)));
            return null;
        }

        var type = Visit(expression.ThenBranch, token);

        if (type is null)
            return null;

        var elseType = expression.ElseBranch is not null
            ? Visit(expression.ElseBranch, token)
            : type;

        if (elseType is null)
            return null;

        if (type != elseType)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.ElseBranch!, elseType, type));
            return null;
        }

        return type;
    }

    public override Type? VisitInvocation(InvocationExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        if (expression.Function is not ConstantExpression constant)
        {
            _errors.Add(new InterpreterException("expected function name", expression.Function.Range));
            return null;
        }

        var function = GetFunction(expression, constant.Lexeme);

        if (function is null)
        {
            _errors.Add(new UndeclaredFunctionException(constant.Lexeme, constant.Range));
            return null;
        }

        for (var i = 0; i < function.ArgumentTypes.Length; i++)
        {
            var argType = Visit(expression.Arguments[i], token);

            if (argType is null)
                return null;

            if (!argType.IsAssignableTo(function.ArgumentTypes[i]))
            {
                _errors.Add(new WrongFunctionArgumentException(argType, function.Name, expression.Arguments[i].Range));
                return null;
            }
        }

        return function.ReturnType;
    }

    public override Type? VisitParenthesized(ParenthesizedExpression expression, CancellationToken token)
    {
        return Visit(expression.Expression, token);
    }

    public override Type? VisitRepeat(RepeatExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        var countType = Visit(expression.CountExpression, token);

        if (countType is null)
            return null;

        if (countType != typeof(double))
        {
            _errors.Add(new ExpectedOtherTypeException(expression.CountExpression, countType, typeof(double)));
            return null;
        }

        return Visit(expression.Body, token);
    }

    public override Type? VisitScope(ScopeExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        var type = typeof(Empty);

        foreach (var innerExpression in expression.InnerExpressions)
        {
            var result = Visit(innerExpression, token);

            if (result is null)
                return null;

            type = result;
        }

        return type;
    }

    public override Type? VisitVariable(VariableExpression expression, CancellationToken token)
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

        var assignmentType = variable.Type;

        if (expression.AssignmentExpression is not null)
        {
            var result = Visit(expression.AssignmentExpression, token);

            if (result is null)
                return null;

            assignmentType = result;
        }

        if (variable.Type != assignmentType)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.AssignmentExpression!, assignmentType,
                variable.Type));
            return null;
        }

        return variable.Type;
    }

    public override Type? VisitWhile(WhileExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        var conditionType = Visit(expression.Condition, token);

        if (conditionType is null)
            return null;

        if (conditionType != typeof(bool))
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Condition, conditionType, typeof(bool)));
            return null;
        }

        return Visit(expression.Body, token);
    }

    public override Type? VisitPrefixUnary(PrefixUnaryExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        var type = Visit(expression.Operand, token);

        if (type is null)
            return null;

        if (type == typeof(double))
            return typeof(double);

        _errors.Add(new InterpreterException("Left operand of prefix expression must be number", expression.Range));
        return null;
    }

    public override Type? VisitPostfixUnary(PostfixUnaryExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        var variable = GetVariable(expression, expression.Operand.Lexeme);

        if (variable is null)
        {
            _errors.Add(new UndeclaredVariableException(expression.Operand.Lexeme, expression.Range));
            return null;
        }

        if (variable.Type == typeof(double))
            return typeof(double);

        _errors.Add(new InterpreterException("Right operand of postfix expression must be number", expression.Range));
        return null;
    }

    public override Type? VisitElementAccess(ElementAccessExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        var type = Visit(expression.Expression, token);

        if (type is null)
            return null;

        if (expression.IsArrayDeclaration || expression.IsArrayConstructor)
        {
            if (type == typeof(double))
                return typeof(double[]);
            if (type == typeof(string))
                return typeof(string[]);
            if (type == typeof(bool))
                return typeof(bool[]);
            if (type.IsAssignableTo(typeof(Array)))
                return typeof(Array[]);
        }

        if (expression.Elements.Length != 1)
        {
            _errors.Add(new InterpreterException("Array indexer must contain only 1 index", expression.Range));
            return null;
        }

        var indexType = Visit(expression.Elements[0], token);

        if (indexType == typeof(double))
        {
            if (type == typeof(double[]))
                return typeof(double);
            if (type == typeof(string[]))
                return typeof(string);
            if (type == typeof(bool[]))
                return typeof(bool);
            if (type.IsAssignableTo(typeof(Array[])))
                return typeof(Array);
        }

        _errors.Add(new InterpreterException("Array indexer must be number", expression.Elements[0].Range));
        return null;
    }

    public override Type? VisitArrayInitialization(ArrayInitializationExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new InterpreterException("Operation was cancelled", default));
            return null;
        }

        if (expression.Elements.IsDefaultOrEmpty)
            return typeof(Empty);

        var type = Visit(expression.Elements[0], token);

        foreach (var element in expression.Elements)
        {
            if (type == Visit(element, token))
                continue;

            _errors.Add(new InterpreterException("Array cannot contain elements of different types",
                element.Range));

            return null;
        }

        if (type == typeof(double))
            return typeof(double[]);
        if (type == typeof(string))
            return typeof(string[]);
        if (type == typeof(bool))
            return typeof(bool[]);
        if (type?.IsAssignableTo(typeof(Array)) ?? false)
            return typeof(Array[]);

        _errors.Add(new InterpreterException("Unkown type", expression.Range));
        return null;
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