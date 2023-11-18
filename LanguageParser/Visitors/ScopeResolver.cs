using LanguageParser.Expressions;

namespace LanguageParser.Visitors;

public sealed class ScopeResolver : ExpressionWalker
{
    private readonly ExpressionBase _root;
    private ScopeExpression? _currentScope;
    private ScopeExpression? _targetScope;
    private ExpressionBase? _target;
    private bool _found;

    public ScopeResolver(ExpressionBase expression)
    {
        _root = expression;
    }

    public static ScopeExpression? FindScope(ExpressionBase targetExpression, ExpressionBase root)
    {
        return new ScopeResolver(root).FindScope(targetExpression);
    }

    public ScopeExpression? FindScope(ExpressionBase targetExpression)
    {
        _found = false;
        _target = targetExpression;

        Visit(_root);

        return _targetScope;
    }

    public override void Visit(ExpressionBase expression)
    {
        if (_found)
            return;
        
        if (expression == _target)
        {
            _found = true;
            _targetScope = _currentScope;
            return;
        }

        base.Visit(expression);
    }

    public override void VisitScope(ScopeExpression expression)
    {
        var prevScope = _currentScope;
        _currentScope = expression;
        base.VisitScope(expression);
        _currentScope = prevScope;
    }
}