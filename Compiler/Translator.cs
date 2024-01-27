using System.Globalization;
using Compiler.Common;
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
        ushort id;
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