using Compiler.Common;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

namespace Compiler;

public sealed class TypeResolver : ExpressionVisitor<Type?, CancellationToken>
{
    private readonly List<SyntaxException> _errors = new();
    private readonly ScopeNode _rootScope;
    private FunctionBuilder? _functionBuilder;

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
            return new UnhandledCompilerException(e);
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
            return new UnhandledCompilerException(e);
        }
    }

    public override Type? VisitConstant(ConstantExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        if (expression.IsBool || expression.Kind == SyntaxKind.Bool)
            return PrimitiveTypes.Boolean;
        if (expression.IsNumber || expression.Kind == SyntaxKind.Number)
            return PrimitiveTypes.Double;
        if (expression.IsString || expression.Kind == SyntaxKind.String)
            return PrimitiveTypes.String;

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
            _errors.Add(new CompilerException("Operation was cancelled", default));
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

                if (leftType == PrimitiveTypes.Double && rightType == PrimitiveTypes.Double)
                    return PrimitiveTypes.Double;

                _errors.Add(new IncompatibleOperandsException(expression.Operator));
                return null;

            case SyntaxKind.AddExpression:

                if (leftType == PrimitiveTypes.String || rightType == PrimitiveTypes.String)
                    return PrimitiveTypes.String;

                if (leftType == PrimitiveTypes.Double && rightType == PrimitiveTypes.Double)
                    return PrimitiveTypes.Double;

                if (leftType == PrimitiveTypes.Boolean || rightType == PrimitiveTypes.Boolean)
                {
                    _errors.Add(new UnexpectedOperandsException(expression.Operator, PrimitiveTypes.Boolean));
                    return null;
                }

                _errors.Add(new ExpectedOtherTypeException(expression.Right, rightType, leftType));
                return null;

            case SyntaxKind.RelationalExpression:

                if (leftType == PrimitiveTypes.Double && rightType == PrimitiveTypes.Double)
                    return PrimitiveTypes.Boolean;

                _errors.Add(new IncompatibleOperandsException(expression.Operator, PrimitiveTypes.Double));
                return null;

            case SyntaxKind.AndExpression:
            case SyntaxKind.OrExpression:

                if (leftType == PrimitiveTypes.Boolean && rightType == PrimitiveTypes.Boolean)
                    return PrimitiveTypes.Boolean;

                _errors.Add(new IncompatibleOperandsException(expression.Operator, PrimitiveTypes.Boolean));
                return null;

            case SyntaxKind.AssignmentExpression:
                if (expression.Left is ConstantExpression constant)
                {
                    if (constant.IsBool || constant.IsNumber || constant.IsString)
                    {
                        _errors.Add(new CompilerException($"Expected variable, but got {constant.Lexeme}",
                            constant.Range));
                        return null;
                    }

                    var variable = GetVariable(expression, constant.Lexeme);

                    if (variable is null)
                    {
                        _errors.Add(new UndeclaredVariableException(constant.Lexeme, expression.Range));
                        return null;
                    }

                    if (variable.Type != rightType || variable.Type.GenericArgument != rightType.GenericArgument)
                    {
                        _errors.Add(new ExpectedOtherTypeException(expression.Right, rightType, variable.Type));
                        return null;
                    }

                    return variable.Type;
                }

                if (expression.Left is ElementAccessExpression { IsElementAccessor: true, Elements.Length: 1 })
                {
                    if (leftType == rightType)
                        return leftType;

                    _errors.Add(new IncompatibleOperandsException(expression.Operator, leftType));
                    return null;
                }

                break;
            case SyntaxKind.EqualityExpression:
                return PrimitiveTypes.Boolean;
        }

        _errors.Add(new CompilerException($"Unknown operator {expression.Operator.Lexeme}", expression.Range));
        return null;
    }

    public override Type? VisitFor(ForExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var conditionType = expression.Condition is null
            ? PrimitiveTypes.Boolean
            : Visit(expression.Condition, token);

        if (conditionType is null)
            return null;

        if (conditionType != PrimitiveTypes.Boolean)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Condition!, conditionType, PrimitiveTypes.Boolean));
            return null;
        }

        return Visit(expression.Body, token);
    }

    public override Type? VisitForTo(ForToExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var varType = Visit(expression.Variable, token);

        if (varType is null)
            return null;

        if (varType != PrimitiveTypes.Double)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Variable, varType, PrimitiveTypes.Double));
            return null;
        }

        var countType = Visit(expression.Count, token);

        if (countType is null)
            return null;

        if (countType != PrimitiveTypes.Double)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Count, varType, PrimitiveTypes.Double));
            return null;
        }

        return Visit(expression.Body, token);
    }

    public override Type? VisitForIn(ForInExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var varType = Visit(expression.Variable, token);

        if (varType is null)
            return null;

        var arrayType = Visit(expression.Collection, token);
        if (arrayType is null)
            return null;

        if (arrayType.PrimitiveType == PrimitiveType.Array)
        {
            _errors.Add(new CompilerException("Expected array with specified data type", expression.Collection.Range));
            return null;
        }

        if (arrayType.GenericArgument is null)
        {
            _errors.Add(new UnhandledCompilerException(new Exception("Array with no generic type")));
            return null;
        }

        if (arrayType.GenericArgument != varType)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Variable, varType, arrayType.GenericArgument));
            return null;
        }

        return Visit(expression.Body, token);
    }

    public override Type? VisitIf(IfExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var conditionType = Visit(expression.Condition, token);

        if (conditionType is null)
            return null;

        if (conditionType != PrimitiveTypes.Boolean)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Condition, conditionType, PrimitiveTypes.Boolean));
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
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        if (expression.Function is not ConstantExpression constant)
        {
            _errors.Add(new CompilerException("expected function name", expression.Function.Range));
            return null;
        }

        var argTypes = new List<Type>();

        foreach (var argument in expression.Arguments)
        {
            var type = Visit(argument, token);

            if (type is null)
                return null;

            argTypes.Add(type);
        }

        var function = GetFunction(expression, constant.Lexeme, argTypes);

        if (function is not null)
        {
            if (function.ReturnType is null)
            {
                _errors.Add(new CompilerException($"{function.Name} return type is null", expression.Range));
                return null;
            }

            return function.ReturnType;
        }

        _errors.Add(new UndeclaredFunctionException(constant.Lexeme, constant.Range));
        return null;
    }

    public override Type? VisitParenthesized(ParenthesizedExpression expression, CancellationToken token)
    {
        return Visit(expression.Expression, token);
    }

    public override Type? VisitRepeat(RepeatExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var countType = Visit(expression.CountExpression, token);

        if (countType is null)
            return null;

        if (countType != PrimitiveTypes.Double)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.CountExpression, countType, PrimitiveTypes.Double));
            return null;
        }

        return Visit(expression.Body, token);
    }

    public override Type? VisitScope(ScopeExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var type = PrimitiveTypes.Empty;

        foreach (var innerExpression in expression.InnerExpressions.OrderBy(e =>
                     e is FunctionDeclarationExpression ? 0 : 1))
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
            _errors.Add(new CompilerException("Operation was cancelled", default));
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
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var conditionType = Visit(expression.Condition, token);

        if (conditionType is null)
            return null;

        if (conditionType != PrimitiveTypes.Boolean)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Condition, conditionType, PrimitiveTypes.Boolean));
            return null;
        }

        return Visit(expression.Body, token);
    }

    public override Type? VisitPrefixUnary(PrefixUnaryExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var type = Visit(expression.Operand, token);

        if (type is null)
            return null;

        if (type == PrimitiveTypes.Double)
            return PrimitiveTypes.Double;

        _errors.Add(new CompilerException("Left operand of prefix expression must be number", expression.Range));
        return null;
    }

    public override Type? VisitPostfixUnary(PostfixUnaryExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var variable = GetVariable(expression, expression.Operand.Lexeme);

        if (variable is null)
        {
            _errors.Add(new UndeclaredVariableException(expression.Operand.Lexeme, expression.Range));
            return null;
        }

        if (variable.Type == PrimitiveTypes.Double)
            return PrimitiveTypes.Double;

        _errors.Add(new CompilerException("Right operand of postfix expression must be number", expression.Range));
        return null;
    }

    public override Type? VisitElementAccess(ElementAccessExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var type = Visit(expression.Expression, token);

        if (type is null)
            return null;

        if (expression.IsArrayDeclaration || expression.IsArrayConstructor)
            return PrimitiveTypes.Array with { GenericArgument = type };

        if (expression.Elements.Length != 1)
        {
            _errors.Add(new CompilerException("Array indexer must contain only 1 index", expression.Range));
            return null;
        }

        var indexType = Visit(expression.Elements[0], token);

        if (indexType == PrimitiveTypes.Double)
            return PrimitiveTypes.Array with { GenericArgument = PrimitiveTypes.Double };

        _errors.Add(new CompilerException("Array indexer must be number", expression.Elements[0].Range));
        return null;
    }

    public override Type? VisitArrayInitialization(ArrayInitializationExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        if (expression.Elements.IsDefaultOrEmpty)
            return PrimitiveTypes.Empty;

        var type = Visit(expression.Elements[0], token);

        foreach (var element in expression.Elements)
        {
            if (type == Visit(element, token))
                continue;

            _errors.Add(new CompilerException("Array cannot contain elements of different types",
                element.Range));

            return null;
        }

        return PrimitiveTypes.Array with { GenericArgument = PrimitiveTypes.Double };
    }

    public override Type? VisitReturn(ReturnExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var type = expression.ReturnValue is null ? PrimitiveTypes.Empty : Visit(expression.ReturnValue, token);

        if (type is null)
            return null;

        if (_functionBuilder is null)
        {
            _errors.Add(new CompilerException("return cannot be used outside function body", expression.Range));
            return null;
        }

        try
        {
            _functionBuilder?.SetReturnType(type);
        }
        catch
        {
            _errors.Add(new CompilerException("function must have only 1 return type", expression.Range));
            return null;
        }

        return type;
    }

    public override Type? VisitFunctionDeclaration(FunctionDeclarationExpression expression, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _errors.Add(new CompilerException("Operation was cancelled", default));
            return null;
        }

        var functionBuilder = _functionBuilder;

        var parameters = new List<Type>();

        foreach (var parameter in expression.Parameters)
        {
            var type = Visit(parameter, token);
            if (type is null)
                return null;

            parameters.Add(type);
        }

        _functionBuilder = GetFunction(expression, expression.NameToken.Lexeme, parameters);

        try
        {
            var type = Visit(expression.Body, token);
            if (type is null)
                return null;

            try
            {
                _functionBuilder!.SetReturnType(type);
            }
            catch (Exception e)
            {
                _errors.Add(new CompilerException(e.Message, expression.Range));
            }

            return PrimitiveTypes.Empty;
        }
        finally
        {
            _functionBuilder = functionBuilder;
        }
    }

    private Variable? GetVariable(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _rootScope.Scope);

        var node = _rootScope.FindDescendant(node => node.Scope == scope);

        var variable = node?.GetVariableIncludingAncestors(name);

        return variable;
    }

    private FunctionBuilder? GetFunction(ExpressionBase expression, string name, IEnumerable<Type> parameters)
    {
        var scope = ScopeResolver.FindScope(expression, _rootScope.Scope);

        var node = _rootScope.FindDescendant(node => node.Scope == scope);

        var function = node?.GetFunctionIncludingAncestors(name, parameters);

        return function;
    }
}