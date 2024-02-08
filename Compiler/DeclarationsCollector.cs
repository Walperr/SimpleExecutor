using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

namespace Compiler;

public class DeclarationsCollector : ExpressionWalker
{
    private readonly ExpressionBase _expression;
    private Function _currentFunction;
    private Scope? _currentScope;
    private Scope? _root;
    private readonly List<SyntaxException> _errors = new();
    private int _totalFunctions;

    private DeclarationsCollector(ExpressionBase expression)
    {
        _expression = expression;
        _currentFunction = new Function(".main", _totalFunctions++, 0);
        _currentFunction.SetReturnType(PrimitiveTypes.Empty);
    }

    public static Scope? Collect(ExpressionBase expression) => new DeclarationsCollector(expression).Collect();

    private Scope? Collect()
    {
        Visit(_expression);

        if (_errors.Count > 0)
            return null;

        return _root;
    }

    public override void VisitScope(ScopeExpression expression)
    {
        var scope = new Scope(_currentScope, _currentFunction, expression);

        _currentScope?.AddScope(scope);

        var currentScope = _currentScope;

        _currentScope = scope;
        _root ??= _currentScope;
        
        base.VisitScope(expression);

        _currentScope = currentScope;
    }

    public override void VisitVariable(VariableExpression expression)
    {
        var name = expression.NameToken.Lexeme;

        var variable = new Variable(PrimitiveTypes.None, name);
        if (!_currentScope?.AddVariable(variable) ?? true)
        {
            _errors.Add(new VariableAlreadyDeclaredException(name, expression.NameToken.Range));
            return;
        }
        _currentFunction.AddVariable(variable);
        
        base.VisitVariable(expression);
    }

    public override void VisitFunctionDeclaration(FunctionDeclarationExpression expression)
    {
        var name = expression.NameToken.Lexeme;
        var function = _currentFunction;
        _currentFunction = new Function(name, _totalFunctions++, expression.Parameters.Count());
        
        if (!_currentScope?.AddFunction(_currentFunction) ?? true)
        {
            _errors.Add(new FunctionAlreadyDeclaredException(name, expression.NameToken.Range));
            return;
        }
        
        var scope = new Scope(_currentScope, _currentFunction, expression);

        _currentScope?.AddScope(scope);

        var currentScope = _currentScope;

        _currentScope = scope;
        _root ??= _currentScope;

        base.VisitFunctionDeclaration(expression);

        _currentFunction = function;
        _currentScope = currentScope;
    }
}