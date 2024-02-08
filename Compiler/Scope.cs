using LanguageParser.Expressions;

namespace Compiler;

public class Scope
{
    private readonly Dictionary<string, HashSet<Function>> _accessibleFunctions = new();
    private readonly Dictionary<string, Variable> _accessibleVariables = new();

    private readonly List<Scope> _children = new();

    public Scope(Scope? parent, Function context, ExpressionBase expression)
    {
        Parent = parent;
        Context = context;
        Expression = expression;
    }

    public Scope? Parent { get; }

    public IEnumerable<Scope> Children => _children;
    public ExpressionBase Expression { get; }
    public Function Context { get; }

    public bool AddVariable(Variable variable)
    {
        return _accessibleVariables.TryAdd(variable.Name, variable);
    }

    public bool AddFunction(Function function)
    {
        if (!_accessibleFunctions.TryGetValue(function.Name, out var functions))
            _accessibleFunctions[function.Name] = functions = new HashSet<Function>();

        if (functions.Any(f => f.Parameters.SequenceEqual(function.Parameters)))
            return false;

        functions.Add(function);

        return true;
    }

    public Function? GetFunctionIncludingAncestors(string functionName, IEnumerable<Type> parameters)
    {
        return !_accessibleFunctions.TryGetValue(functionName, out var functions)
            ? Parent?.GetFunctionIncludingAncestors(functionName, parameters)
            : functions.FirstOrDefault(f => f.Parameters.Select(v => v.Type).SequenceEqual(parameters));
    }

    public Variable? GetVariableIncludingAncestors(string variableName)
    {
        if (!_accessibleVariables.TryGetValue(variableName, out var variable))
            variable = Parent?.GetVariableIncludingAncestors(variableName);

        return variable;
    }

    public void AddScope(Scope child)
    {
        _children.Add(child);
    }
}