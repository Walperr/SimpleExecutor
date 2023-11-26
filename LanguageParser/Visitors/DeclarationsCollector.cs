using LanguageParser.Common;
using LanguageParser.Expressions;

namespace LanguageParser.Visitors;

public sealed class DeclarationsCollector : ExpressionWalker
{
    private readonly ExpressionBase _expression;
    private ScopeNode? _currentNode;
    private ScopeNode? _root;

    private DeclarationsCollector(ExpressionBase expression)
    {
        _expression = expression;
    }

    public static ScopeNode? Collect(ExpressionBase expression)
    {
        return new DeclarationsCollector(expression).Collect();
    }

    public ScopeNode? Collect()
    {
        Visit(_expression);

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
                    SyntaxKind.Number => new Variable(name, typeof(double)),
                    SyntaxKind.String => new Variable(name, typeof(string)),
                    SyntaxKind.Bool => new Variable(name, typeof(bool)),
                    _ => throw new UnexpectedTokenException(constant.Token)
                };

                if (!(_currentNode?.AddVariable(variable) ?? true))
                    throw new VariableAlreadyDeclaredException(name, expression.Range);
                break;
            }
            case ElementAccessExpression { IsArrayDeclaration: true } arrayDeclaration: // is array
            {
                Variable variable;
                if (arrayDeclaration.Expression is ElementAccessExpression { IsArrayDeclaration: true }) // is array of arrays
                {
                    variable = new Variable(name, typeof(Array[]));
                }
                else
                {
                    variable = arrayDeclaration.Expression.Kind switch
                    {
                        SyntaxKind.Number => new Variable(name, typeof(double[])),
                        SyntaxKind.String => new Variable(name, typeof(string[])),
                        SyntaxKind.Bool => new Variable(name, typeof(bool[])),
                        _ => throw new UnexpectedTokenException(((ConstantExpression)arrayDeclaration.Expression).Token)
                    };
                }
                
                if (!(_currentNode?.AddVariable(variable) ?? true))
                    throw new VariableAlreadyDeclaredException(name, expression.Range);
                break;
            }
            default:
                throw new UnexpectedExpressionException(expression.TypeExpression);
        }

        base.VisitVariable(expression);
    }
}