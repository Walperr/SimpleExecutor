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

        var variable = expression.TypeToken.Kind switch
        {
            SyntaxKind.Number => new Variable(name, typeof(double)),
            SyntaxKind.String => new Variable(name, typeof(string)),
            SyntaxKind.Bool => new Variable(name, typeof(bool)),
            _ => throw new UnexpectedTokenException(expression.TypeToken)
        };

        if (!(_currentNode?.AddVariable(variable) ?? true))
            throw new VariableAlreadyDeclaredException(name, expression.Range);

        base.VisitVariable(expression);
    }
}