using System.Diagnostics;
using System.Globalization;
using Compiler.Exceptions;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Parser;
using LanguageParser.Visitors;

namespace Compiler;

public class Compiler : ExpressionWalker
{
    private readonly AssemblyBuilder _builder = new("Default assembly");
    private readonly List<SyntaxException> _errors = new();
    private readonly Scope _root;
    private Function _context;
    private ExpressionBase? _currentScope;

    private Compiler(Scope scope)
    {
        _root = scope;
        _context = scope.Context;
    }

    public static void Compile(string code, Stream stream, bool useNewFormat = true)
    {
        var result = ExpressionsParser.Parse(code);

        if (result.IsError)
        {
            Console.WriteLine(result.Error.Message);
            return;
        }

        var scope = DeclarationsCollector.Collect(result.Value);

        if (scope is null)
        {
            Console.WriteLine("Compile failed");
            return;
        }

        var type = TypeResolver.Resolve(scope);

        if (type.IsError)
        {
            Console.WriteLine(type.Error.Message);
            return;
        }

        new Compiler(scope).Compile(stream, useNewFormat);
    }

    public static string GenerateIntermediateView(string code)
    {
        var result = ExpressionsParser.Parse(code);

        if (result.IsError)
        {
            return result.Error.Message;
        }

        var scope = DeclarationsCollector.Collect(result.Value);

        if (scope is null)
        {
            return "Compile failed";
        }

        var type = TypeResolver.Resolve(scope);

        if (type.IsError)
        {
            Console.WriteLine(type.Error.Message);
            return type.Error.Message;
        }

        return new Compiler(scope).CompileIntermediateView();
    }

    private string CompileIntermediateView()
    {
        Visit(_root.Expression);

        using var _ = _builder.SetContext(_context);

        _builder.AddOpReturn();

        return _builder.Build(true);
    }

    private void Compile(Stream stream, bool useNewFormat = true)
    {
        Visit(_root.Expression);

        using var _ = _builder.SetContext(_context);

        _builder.AddOpReturn();

        if (useNewFormat)
            _builder.BuildNew(stream, true);
        else
            _builder.Build(stream);
    }

    public override void VisitConstant(ConstantExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        if (expression.IsBool)
            _builder.AddOpLoadConstant(bool.Parse(expression.Lexeme));
        else if (expression.IsNumber)
            _builder.AddOpLoadConstant(
                double.Parse(expression.Lexeme, NumberStyles.Float, CultureInfo.InvariantCulture));
        else if (expression.IsString)
            _builder.AddOpLoadConstant(expression.Lexeme);
        else
            _builder.AddOpLoadVariable(expression.Lexeme);
    }

    public override void VisitBinary(BinaryExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        if (expression.Kind is SyntaxKind.AssignmentExpression)
        {
            switch (expression.Left)
            {
                case ConstantExpression constant:
                    Visit(expression.Right);
                    _builder.AddOpSetVariable(constant.Lexeme);
                    break;

                case ElementAccessExpression indexer when indexer.Elements.Length != 1:
                    _errors.Add(new CompilerException("Too many arguments in array indexer", indexer.Range));
                    return;

                case ElementAccessExpression indexer:
                    Visit(indexer.Expression); // вычислили адресс массива
                    Visit(indexer.Elements[0]); // вычислили индекс
                    _builder.AddOpCast(PrimitiveTypes.Double, PrimitiveTypes.Int32); // перевели к int32

                    Debug.Assert(indexer.Expression.Type is not null);

                    var type = (Type)indexer.Expression.Type;

                    Debug.Assert(type?.PrimitiveType == PrimitiveType.Array);
                    Debug.Assert(type.GenericArgument is not null);

                    _builder.AddOpLoadConstant(type.GenericArgument.Size);
                    _builder.AddOpMultiply();
                    _builder.AddOpAdd();
                    // на стеке находится адрес массива
                    Visit(expression.Right); // загружаем значение

                    // устанавливаем значение в поле 3, т.к. у массива всего 2 поля, то с учетом ранее вычисленных сдвигов получим адрес нужного элемента
                    _builder.AddOpSetField(type, 3);
                    break;
            }
        }
        else
        {
            Debug.Assert(expression.Left.Type is not null);
            Debug.Assert(expression.Right.Type is not null);

            var leftType = (Type)expression.Left.Type;
            var rightType = (Type)expression.Right.Type;

            Visit(expression.Left);
            Visit(expression.Right);

            switch (expression.Kind)
            {
                case SyntaxKind.DivideExpression:
                    _builder.AddOpDivide();
                    break;

                case SyntaxKind.RemainderExpression:
                    throw new NotImplementedException();

                case SyntaxKind.MultiplyExpression:
                    _builder.AddOpMultiply();
                    break;

                case SyntaxKind.SubtractExpression:
                    _builder.AddOpSubtract();
                    break;

                case SyntaxKind.AddExpression:
                    if (leftType == PrimitiveTypes.Double && rightType == PrimitiveTypes.Double)
                        _builder.AddOpAdd();
                    else
                        _builder.AddOpCallFunction("stringConcat", new[] { leftType, rightType },
                            _root.FindDescendant(s =>
                                s.Expression == _currentScope)); // should be function for string concatenation 
                    break;

                case SyntaxKind.RelationalExpression:
                    _builder.AddOpCompare(expression.Operator.Lexeme);
                    break;

                case SyntaxKind.AndExpression:
                    _builder.AddOpAnd();
                    break;

                case SyntaxKind.OrExpression:
                    _builder.AddOpOr();
                    break;

                case SyntaxKind.EqualityExpression:
                    Debug.Assert(leftType == rightType);
                    _builder.AddOpEquals(leftType);
                    break;
            }
        }
    }

    public override void VisitFor(ForExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        if (expression.Initialization is not null)
            Visit(expression.Initialization);

        using var marker = _builder.GetMarker(); // mark location for jump instruction

        using var bodyMarker = _builder.GetMarker();

        Visit(expression.Body);

        if (expression.Step is not null)
            Visit(expression.Step);

        using var conditionMarker = _builder.GetMarker();

        marker.SetOperation(Opcodes.OpJump, conditionMarker.Value + 1);

        if (expression.Condition is not null)
            Visit(expression.Condition);
        else
            _builder.AddOpLoadConstant(true);

        _builder.AddOpJumpTrue(bodyMarker.Value);
    }

    public override void VisitForTo(ForToExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        var variableName = expression.Variable.NameToken.Lexeme;

        // convert to traditional c-like for loop

        Visit(expression.Variable);

        using var marker = _builder.GetMarker();

        using var bodyMarker = _builder.GetMarker();

        Visit(expression.Body);

        _builder.AddOpLoadVariable(variableName);
        _builder.AddOpLoadConstant(1);

        if (expression.DownToken is null)
            _builder.AddOpAdd();
        else
            _builder.AddOpSubtract();

        _builder.AddOpSetVariable(variableName);

        using var conditionMarker = _builder.GetMarker();

        marker.SetOperation(Opcodes.OpJump, conditionMarker.Value + 1);

        _builder.AddOpLoadVariable(variableName);
        Visit(expression.Count);
        _builder.AddOpCompare(expression.DownToken is null ? "<" : ">");
        _builder.AddOpJumpTrue(bodyMarker.Value);
    }

    public override void VisitForIn(ForInExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        throw new NotImplementedException();
    }

    public override void VisitIf(IfExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        Visit(expression.Condition);

        using var thenMarker = _builder.GetMarker();

        Visit(expression.ThenBranch);

        using var elseMarker = _builder.GetMarker();

        thenMarker.SetOperation(Opcodes.OpJumpIfFalse, elseMarker.Value + 1);

        if (expression.ElseBranch is not null)
            Visit(expression.ElseBranch);
    }

    public override void VisitInvocation(InvocationExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        if (expression.Function is not ConstantExpression constant)
        {
            _errors.Add(new CompilerException("Expected function name", expression.Function.Range));
            return;
        }

        var argumentTypes = new List<Type>();
        foreach (var arg in expression.Arguments)
        {
            Visit(arg);

            if (arg.Type is null)
                throw new CompilerException("Some arguments has unresolved types", expression.Range);

            argumentTypes.Add((Type)arg.Type);
        }

        _builder.AddOpCallFunction(constant.Lexeme, argumentTypes,
            _root.FindDescendant(s => s.Expression == _currentScope));
    }

    public override void VisitParenthesized(ParenthesizedExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        Visit(expression.Expression);
    }

    public override void VisitRepeat(RepeatExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        base.VisitRepeat(expression);
    }

    public override void VisitScope(ScopeExpression expression)
    {
        var prevScope = _currentScope;
        _currentScope = expression;

        using var _ = _builder.SetContext(_context);

        foreach (var innerExpression in expression.InnerExpressions) Visit(innerExpression);

        _currentScope = prevScope;
    }

    public override void VisitVariable(VariableExpression expression)
    {
        if (expression.AssignmentExpression is null)
            return;

        using var _ = _builder.SetContext(_context);

        Visit(expression.AssignmentExpression);
    }

    public override void VisitWhile(WhileExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        using var marker = _builder.GetMarker(); // mark location for jump instruction

        using var bodyMarker = _builder.GetMarker();

        Visit(expression.Body);

        using var conditionMarker = _builder.GetMarker();

        marker.SetOperation(Opcodes.OpJump, conditionMarker.Value + 1);

        Visit(expression.Condition);
    }

    public override void VisitPrefixUnary(PrefixUnaryExpression expression)
    {
        throw new NotImplementedException();
    }

    public override void VisitPostfixUnary(PostfixUnaryExpression expression)
    {
        throw new NotImplementedException();
    }

    public override void VisitElementAccess(ElementAccessExpression expression)
    {
        throw new NotImplementedException();
    }

    public override void VisitArrayInitialization(ArrayInitializationExpression expression)
    {
        throw new NotImplementedException();
    }

    public override void VisitReturn(ReturnExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        if (expression.ReturnValue is not null)
            Visit(expression.ReturnValue);

        _builder.AddOpReturn();
    }

    public override void VisitFunctionDeclaration(FunctionDeclarationExpression expression)
    {
        var context = _context;

        var functionName = expression.NameToken.Lexeme;
        
        var prevScope = _currentScope;
        _currentScope = expression;

        var parameters = expression.Parameters.Select(p => p.Type).Cast<Type>().ToArray();

        if (parameters.Any(p => p is null))
            throw new CompilerException("Some parameters types are unresolved", expression.NameToken.Range);

        _context = _root.FindDescendant(s => s.Expression == _currentScope)
                       ?.GetFunctionIncludingAncestors(functionName, parameters)
                   ?? throw new CompilerException(
                       $"Cannot resolve function {functionName}, takes {parameters.Length} arguments in this scope",
                       expression.NameToken.Range);

        foreach (var parameter in expression.Parameters)
            Visit(parameter);

        Visit(expression.Body);

        _context = context;
        _currentScope = prevScope;
    }
}