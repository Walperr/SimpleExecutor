using System.Globalization;
using Compiler.Common;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

namespace Compiler;

public sealed class Translator : ExpressionVisitor
{
    private readonly ScopeNode _root;
    private readonly ConstantsWriter _constantsWriter = new();
    private FunctionBuilder _currentContext;

    private readonly List<Type> _definedTypes = new()
    {
        PrimitiveTypes.Struct, PrimitiveTypes.Boolean, PrimitiveTypes.Byte, PrimitiveTypes.Int16, PrimitiveTypes.Int32,
        PrimitiveTypes.Int32, PrimitiveTypes.Int64, PrimitiveTypes.Single, PrimitiveTypes.Double, PrimitiveTypes.Array,
        PrimitiveTypes.String, PrimitiveTypes.Empty
    };

    public Translator(ScopeNode root)
    {
        _root = root;
        _currentContext = root.GetFunction(".main", Enumerable.Empty<Type>()) ??
                          throw new Exception("Entry point not found!");
    }

    private void Translate()
    {
        Visit(_root.Scope);
    }

    public override void VisitConstant(ConstantExpression expression)
    {
        var codeWriter = _currentContext.CodeWriter;
        
        if (expression.IsBool)
        {
            var boolean = bool.Parse(expression.Lexeme);
            codeWriter.Write(Opcodes.OpLoadConst);
            codeWriter.Write(_constantsWriter.AddConstant(boolean));
        }
        else if (expression.IsNumber)
        {
            var number = double.Parse(expression.Lexeme, NumberStyles.Float, CultureInfo.InvariantCulture);
            codeWriter.Write(Opcodes.OpLoadConst);
            codeWriter.Write(_constantsWriter.AddConstant(number));
        }
        else if (expression.IsString)
        {
            codeWriter.Write(Opcodes.OpLoadConst);
            codeWriter.Write(_constantsWriter.AddConstant(_constantsWriter.AddConstant(expression.Lexeme)));
        }
        else
        {
            var variable = GetVariable(expression, expression.Lexeme);
            if (variable is null)
                throw new CompilerException($"Undeclared variable {expression.Lexeme}", expression.Range);
            
            codeWriter.Write(Opcodes.OpLocalLoad);
            codeWriter.Write(_currentContext.GetVariableID(variable));
        }
    }

    public override void VisitBinary(BinaryExpression expression)
    {
        if (expression.Kind is SyntaxKind.AssignmentExpression)
        {
            switch (expression.Left)
            {
                case ConstantExpression constant:
                    var variable = GetVariable(expression, constant.Lexeme);
                    if (variable is null)
                        throw new CompilerException($"Undeclared variable {constant.Lexeme}", expression.Range);
                    
                    Visit(expression.Right);
                    _currentContext.CodeWriter.Write(Opcodes.OpLocalSet);
                    _currentContext.CodeWriter.Write(_currentContext.GetVariableID(variable));
                    break;
                
                case ElementAccessExpression indexer when indexer.Elements.Length != 1:
                    throw new CompilerException("Wrong arguments count in array indexer", indexer.Range);
                case ElementAccessExpression indexer:
                    throw new NotImplementedException("Arrays are not supported yet");
            }
        }
        
        Visit(expression.Left);
        Visit(expression.Right);

        var typeLeft = TypeResolver.Resolve(_root, expression.Left, CancellationToken.None);
        var typeRight = TypeResolver.Resolve(_root, expression.Right, CancellationToken.None);

        if (typeLeft.Value is null)
            throw typeLeft.Error!;
        if (typeRight.Value is null)
            throw typeRight.Error!;
        
        switch (expression.Kind)
        {
            case SyntaxKind.AddExpression:
                if (typeLeft.Value == PrimitiveTypes.Double && typeRight.Value == PrimitiveTypes.Double)
                {
                    _currentContext.CodeWriter.Write(Opcodes.OpFloatAdd2);
                }
                else
                {
                    throw new NotImplementedException("No numbers addition not supported yet");
                }
                break;
            case SyntaxKind.SubtractExpression:
                _currentContext.CodeWriter.Write(Opcodes.OpFloatSubtract2);
                break;
            case SyntaxKind.MultiplyExpression:
                _currentContext.CodeWriter.Write(Opcodes.OpFloatMultiply2);
                break;
            case SyntaxKind.DivideExpression:
                _currentContext.CodeWriter.Write(Opcodes.OpFloatDivide2); 
                break;
            case SyntaxKind.RemainderExpression:
                throw new NotImplementedException("Remainder expressions are not supported yet");
            case SyntaxKind.RelationalExpression:
                switch (expression.Operator.Lexeme)
                {
                    case ">":
                        _currentContext.CodeWriter.Write(Opcodes.OpCompareGreaterF2);
                        break;
                    case "<":
                        _currentContext.CodeWriter.Write(Opcodes.OpCompareLessF2);
                        break;
                    case ">=":
                    case "<=":
                        throw new NotImplementedException();
                }
                break;
            case SyntaxKind.AndExpression:
                _currentContext.CodeWriter.Write(Opcodes.OpAnd);
                break;
            case SyntaxKind.OrExpression:
                _currentContext.CodeWriter.Write(Opcodes.OpOr);
                break;
            case SyntaxKind.EqualityExpression:
                _currentContext.CodeWriter.Write(Opcodes.OpCompareEquals8);
                break;
        }
    }

    public override void VisitFor(ForExpression expression)
    {
        var codeWriter = _currentContext.CodeWriter;
        if (expression.Initialization is not null)
        {
            Visit(expression.Initialization);
        }

        codeWriter.Write(Opcodes.OpJump);
        var position = (ushort)codeWriter.BaseStream.Position;
        codeWriter.Write((ushort) 0);

        var bodyAddress = (ushort)codeWriter.BaseStream.Position;
        Visit(expression.Body);
        if (expression.Step is not null)
            Visit(expression.Step);

        var conditionAddress = (ushort)codeWriter.BaseStream.Position;

        codeWriter.BaseStream.Position = position;
        codeWriter.Write(conditionAddress);
        
        if (expression.Condition is not null)
            Visit(expression.Condition);
        else
        {
            codeWriter.Write(Opcodes.OpLoadConst);
            codeWriter.Write(_constantsWriter.AddConstant(true));
        }

        codeWriter.Write(Opcodes.OpJumpIfTrue);
        codeWriter.Write(bodyAddress);
    }

    private Variable? GetVariable(ExpressionBase expression, string name)
    {
        var scope = ScopeResolver.FindScope(expression, _root.Scope);

        var node = _root.FindDescendant(node => node.Scope == scope);

        var variable = node?.GetVariableIncludingAncestors(name);

        return variable;
    }

    private FunctionBuilder? GetFunction(ExpressionBase expression, string name, IEnumerable<Type> parameters)
    {
        var scope = ScopeResolver.FindScope(expression, _root.Scope);

        var node = _root.FindDescendant(node => node.Scope == scope);

        var function = node?.GetFunctionIncludingAncestors(name, parameters);

        return function;
    }
}