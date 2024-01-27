using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

namespace Compiler;

public sealed class DeclarationsCollector : ExpressionWalker
{
    private readonly ExpressionBase _expression;
    private ScopeNode? _currentNode;
    private FunctionBuilder _functionBuilder;
    private ScopeNode? _root;
    private ushort _totalFunctions;

    private DeclarationsCollector(ExpressionBase expression)
    {
        _functionBuilder = new FunctionBuilder(".main", 0);
        _functionBuilder.SetID(_totalFunctions++);
        _functionBuilder.SetReturnType(PrimitiveTypes.Empty);
        _expression = expression;
    }

    public static ScopeNode? Collect(ExpressionBase expression)
    {
        return new DeclarationsCollector(expression).Collect();
    }

    public ScopeNode? Collect()
    {
        Visit(_expression);
        _root!.AddFunction(_functionBuilder);
        return _root;
    }

    public override void VisitScope(ScopeExpression expression)
    {
        var node = new ScopeNode(expression, _currentNode);

        _currentNode?.Children.Add(node);

        var currentNode = _currentNode;

        _currentNode = node;

        _root ??= _currentNode;

        base.VisitScope(expression);

        _currentNode = currentNode;
    }

    public override void VisitVariable(VariableExpression expression)
    {
        var name = expression.NameToken.Lexeme;

        switch (expression.TypeExpression)
        {
            case ConstantExpression { IsTypeKeyword: true } constant: // is simple variable
            {
                var variable = constant.Kind switch
                {
                    SyntaxKind.Number => PrimitiveTypes.Double,
                    SyntaxKind.String => PrimitiveTypes.String,
                    SyntaxKind.Bool => PrimitiveTypes.Boolean,
                    _ => throw new UnexpectedTokenException(constant.Token)
                };

                if (!(_currentNode?.AddVariable(variable, name) ?? true))
                    throw new VariableAlreadyDeclaredException(name, expression.Range);

                _functionBuilder.AddVariable(variable, name);
                break;
            }
            case ElementAccessExpression { IsArrayDeclaration: true }: // is array
            {
                var variable = PrimitiveTypes.Array;

                if (!(_currentNode?.AddVariable(variable, name) ?? true))
                    throw new VariableAlreadyDeclaredException(name, expression.Range);
                _functionBuilder.AddVariable(variable, name);
                break;
            }
            default:
                throw new UnexpectedExpressionException(expression.TypeExpression);
        }

        base.VisitVariable(expression);
    }

    public override void VisitFunctionDeclaration(FunctionDeclarationExpression expression)
    {
        var functionName = expression.FunctionToken.Lexeme;

        var functionBuilder = _functionBuilder;

        _functionBuilder = new FunctionBuilder(functionName, expression.Parameters.Count());
        _functionBuilder.SetID(_totalFunctions++);

        foreach (var parameter in expression.Parameters)
        {
            if (parameter is not VariableExpression variable)
                throw new UnexpectedExpressionException(parameter);

            VisitVariable(variable);
        }

        Visit(expression.Body);

        _currentNode?.AddFunction(_functionBuilder);
        _functionBuilder = functionBuilder;
    }
}