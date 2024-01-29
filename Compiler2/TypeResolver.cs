using System.Diagnostics;
using Compiler.Common;
using Compiler2.Exceptions;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

namespace Compiler2;

public class TypeResolver : ExpressionVisitor<Type?, ValueTuple>
{
    private readonly Scope _rootScope;
    private Function _function;
    private readonly List<SyntaxException> _errors = new();

    public TypeResolver(Scope rootScope)
    {
        _rootScope = rootScope;
        _function = rootScope.Context;
    }

    public static Result<SyntaxException, Type> Resolve(Scope root) => new TypeResolver(root).ResolveTypes();

    private Result<SyntaxException, Type> ResolveTypes()
    {
        try
        {
            var type = Visit(_rootScope.Expression, default);

            if (type is null)
                return _errors.First();

            return type;
        }
        catch (Exception e)
        {
            return new UnhandledCompilerException(e);
        }
    }
    
    private Variable? GetVariable(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _rootScope.Expression);

        var node = _rootScope.FindDescendant(node => node.Expression == scope);

        var variable = node?.GetVariableIncludingAncestors(name);

        return variable;
    }

    private Function? GetFunction(ExpressionBase expression, string name, IEnumerable<Type> parameters)
    {
        var scope = ScopeResolver.FindScope(expression, _rootScope.Expression);

        var node = _rootScope.FindDescendant(node => node.Expression == scope);

        var function = node?.GetFunctionIncludingAncestors(name, parameters);

        return function;
    }

    public override Type? VisitBinary(BinaryExpression expression, ValueTuple state)
    {
        var leftType = Visit(expression.Left, state);

        if (leftType is null)
            return null;

        var rightType = Visit(expression.Right, state);

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
                    
                    Debug.Assert(variable.Type != PrimitiveTypes.None);

                    if (variable.Type != rightType/* || variable.Type.GenericArgument != rightType.GenericArgument*/)
                    {
                        _errors.Add(new ExpectedOtherTypeException(expression.Right, rightType, variable.Type));
                        return null;
                    }

                    return variable.Type;
                }

                if (expression.Left is ElementAccessExpression {IsElementAccessor: true, Elements.Length: 1})
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

    public override Type? VisitConstant(ConstantExpression expression, ValueTuple state)
    {
        if (expression.IsBool || expression.Kind == SyntaxKind.Bool)
            return PrimitiveTypes.Boolean;
        if (expression.IsNumber || expression.Kind == SyntaxKind.Number)
            return PrimitiveTypes.Double;
        if (expression.IsString || expression.Kind == SyntaxKind.String)
            return PrimitiveTypes.String;

        var variable = GetVariable(expression, expression.Lexeme);

        Debug.Assert(variable?.Type != PrimitiveTypes.None);
        
        if (variable is not null)
            return variable.Type;

        _errors.Add(new UndeclaredVariableException(expression.Lexeme, expression.Range));
        return null;
    }

    public override Type? VisitFor(ForExpression expression, ValueTuple state)
    {
        if (expression.Initialization is not null)
        {
            var initializeType = Visit(expression.Initialization, state);
            if (initializeType is null)
                return null;
        }

        var conditionType = expression.Condition is null 
            ? PrimitiveTypes.Boolean 
            : Visit(expression.Condition, state);

        if (conditionType is null)
            return null;

        if (conditionType != PrimitiveTypes.Boolean)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Condition!, conditionType, PrimitiveTypes.Boolean));
            return null;
        }

        if (expression.Step is not null)
        {
            var stepType = Visit(expression.Step, state);
            if (stepType is null)
                return null;
        }

        return Visit(expression.Body, state);
    }

    public override Type? VisitForTo(ForToExpression expression, ValueTuple state)
    {
        var varType = Visit(expression.Variable, state);

        if (varType is null)
            return null;
        
        Debug.Assert(varType != PrimitiveTypes.None);

        if (varType != PrimitiveTypes.Double)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Variable, varType, PrimitiveTypes.Double));
            return null;
        }

        var countType = Visit(expression.Count, state);

        if (countType is null)
            return null;

        if (countType != PrimitiveTypes.Double)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Count, varType, PrimitiveTypes.Double));
            return null;
        }

        return Visit(expression.Body, state);
    }

    public override Type? VisitForIn(ForInExpression expression, ValueTuple state)
    {
        var varType = Visit(expression.Variable, state);

        if (varType is null)
            return null;
        
        Debug.Assert(varType != PrimitiveTypes.None);

        var arrayType = Visit(expression.Collection, state);
        if (arrayType is null)
            return null;

        if (arrayType.PrimitiveType != PrimitiveType.Array)
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

        return Visit(expression.Body, state);
    }

    public override Type? VisitIf(IfExpression expression, ValueTuple state)
    {
        var conditionType = Visit(expression.Condition, state);

        if (conditionType is null)
            return null;

        if (conditionType != PrimitiveTypes.Boolean)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Condition, conditionType, PrimitiveTypes.Boolean));
            return null;
        }

        var type = Visit(expression.ThenBranch, state);

        if (type is null)
            return null;

        var elseType = expression.ElseBranch is not null
            ? Visit(expression.ElseBranch, state)
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

    public override Type? VisitInvocation(InvocationExpression expression, ValueTuple state)
    {
        if (expression.Function is not ConstantExpression constant)
        {
            _errors.Add(new CompilerException("expected function name", expression.Function.Range));
            return null;
        }
        
        var argTypes = new List<Type>();
        
        foreach (var argument in expression.Arguments)
        {
            var type = Visit(argument, state);

            if (type is null)
                return null;

            argTypes.Add(type);
        }
        
        var function = GetFunction(expression, constant.Lexeme, argTypes);
        
        if (function is not null)
        {
            if (function.ReturnType == PrimitiveTypes.None)
            {
                _errors.Add(new CompilerException($"{function.Name} return type is missing", expression.Range));
                return null;
            }

            return function.ReturnType;
        }
        
        _errors.Add(new UndeclaredFunctionException(constant.Lexeme, constant.Range));
        return null;
    }

    public override Type? VisitParenthesized(ParenthesizedExpression expression, ValueTuple state)
    {
        return Visit(expression.Expression, state);
    }

    public override Type? VisitRepeat(RepeatExpression expression, ValueTuple state)
    {
        var countType = Visit(expression.CountExpression, state);

        if (countType is null)
            return null;

        if (countType != PrimitiveTypes.Double)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.CountExpression, countType, PrimitiveTypes.Double));
            return null;            
        }

        return Visit(expression.Body, state);
    }

    public override Type? VisitScope(ScopeExpression expression, ValueTuple state)
    {
        var type = PrimitiveTypes.Empty;

        foreach (var innerExpression in expression.InnerExpressions.OrderBy(e =>
                     e is FunctionDeclarationExpression ? 0 : 1))
        {
            var result = Visit(innerExpression, state);

            if (result is null)
                return null;

            type = result;
        }

        return type;
    }

    public override Type? VisitVariable(VariableExpression expression, ValueTuple state)
    {
        var name = expression.NameToken.Lexeme;

        var variable = GetVariable(expression, name);

        if (variable is null)
        {
            _errors.Add(new UndeclaredVariableException(name, expression.Range));
            return null;
        }

        if (variable.Type == PrimitiveTypes.None)
        {
            Type? type;
            switch (expression.TypeExpression)
            {
                case ConstantExpression {IsTypeKeyword: true} constant:
                    type = constant.Kind switch
                    {
                        SyntaxKind.Number => PrimitiveTypes.Double,
                        SyntaxKind.String => PrimitiveTypes.String,
                        SyntaxKind.Bool => PrimitiveTypes.Boolean,
                        _ => throw new UnexpectedTokenException(constant.Token)
                    };

                    variable.Type = type;
                    break;

                case ElementAccessExpression {IsArrayDeclaration: true} array:
                    var genericType = array.Expression.Kind switch
                    {
                        SyntaxKind.Number => PrimitiveTypes.Double,
                        SyntaxKind.String => PrimitiveTypes.String,
                        SyntaxKind.Bool => PrimitiveTypes.Boolean,
                        _ => throw new UnexpectedTokenException(array.OpenBracket)
                    };
                    type = PrimitiveTypes.Array with {GenericArgument = genericType};
                    variable.Type = type;
                    break;

                default:
                    throw new UnexpectedExpressionException(expression.TypeExpression);
            }
        }

        var assignmentType = variable.Type;

        if (expression.AssignmentExpression is not null)
        {
            var result = Visit(expression.AssignmentExpression, state);

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
    
    public override Type? VisitWhile(WhileExpression expression, ValueTuple state)
    {
        var conditionType = Visit(expression.Condition, state);

        if (conditionType is null)
            return null;

        if (conditionType != PrimitiveTypes.Boolean)
        {
            _errors.Add(new ExpectedOtherTypeException(expression.Condition, conditionType, PrimitiveTypes.Boolean));
            return null;
        }

        return Visit(expression.Body, state);
    }

    public override Type? VisitPrefixUnary(PrefixUnaryExpression expression, ValueTuple state)
    {
        var type = Visit(expression.Operand, state);

        if (type is null)
            return null;

        if (type == PrimitiveTypes.Double)
            return PrimitiveTypes.Double;

        _errors.Add(new CompilerException("Left operand of prefix expression must be number", expression.Range));
        return null;
    }

    public override Type? VisitPostfixUnary(PostfixUnaryExpression expression, ValueTuple state)
    {
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

    public override Type? VisitElementAccess(ElementAccessExpression expression, ValueTuple state)
    {
        var type = Visit(expression.Expression, state);

        if (type is null)
            return null;

        if (expression.IsArrayDeclaration || expression.IsArrayConstructor)
            return PrimitiveTypes.Array with {GenericArgument = type};

        if (expression.Elements.Length != 1)
        {
            _errors.Add(new CompilerException("Array indexer must contain only 1 index", expression.Range));
            return null;
        }
        
        var indexType = Visit(expression.Elements[0], state);

        if (indexType == PrimitiveTypes.Double)
            return PrimitiveTypes.Array with {GenericArgument = PrimitiveTypes.Double};
        
        _errors.Add(new CompilerException("Array indexer must be number", expression.Elements[0].Range));
        return null;
    }

    public override Type? VisitArrayInitialization(ArrayInitializationExpression expression, ValueTuple state)
    {
        if (expression.Elements.IsDefaultOrEmpty)
        {
            _errors.Add(new CompilerException("Expected elements of array", expression.Range));
            return null;
        }
        
        var type = Visit(expression.Elements[0], state);

        foreach (var element in expression.Elements)
        {
            if (type == Visit(element, state))
                continue;

            _errors.Add(new CompilerException("Array cannot contain elements of different types",
                element.Range));

            return null;
        }

        return PrimitiveTypes.Array with { GenericArgument = type };
    }

    public override Type? VisitReturn(ReturnExpression expression, ValueTuple state)
    {
        var type = expression.ReturnValue is null ? PrimitiveTypes.Empty : Visit(expression.ReturnValue, state);

        if (type is null)
            return null;

        try
        {
            _function.SetReturnType(type);
        }
        catch
        {
            _errors.Add(new CompilerException("function must return values of the same type", expression.Range));
            return null;
        }

        return type;
    }

    public override Type? VisitFunctionDeclaration(FunctionDeclarationExpression expression, ValueTuple state)
    {
        var function = _function;

        var parameters = new List<Type>();

        foreach (var parameter in expression.Parameters)
        {
            var type = Visit(parameter, state);
            
            if (type is null)
                return null;

            parameters.Add(type);
        }

        _function = GetFunction(expression, expression.NameToken.Lexeme, parameters) ??
                    throw new UnhandledCompilerException(
                        new Exception($"Function not found {expression.NameToken.Lexeme}"));

        try
        {
            var type = Visit(expression.Body, state);
            if (type is null)
                return null;

            try
            {
                _function.SetReturnType(type);
            }
            catch
            {
                _errors.Add(new CompilerException("function must return values of the same type", expression.Range));
                return null;
            }

            return PrimitiveTypes.Empty;
        }
        finally
        {
            _function = function;
        }
    }
}