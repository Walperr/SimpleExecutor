using LanguageParser.Expressions;

namespace LanguageParser.Common;

public sealed class ScopeNode
{
    private readonly Dictionary<string, Variable> _variables = new (StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, List<FunctionBase>> _functions = new();

    public ScopeNode(ScopeExpression scope, ScopeNode? parent)
    {
        Scope = scope;
        Parent = parent;
    }
    
    public ScopeExpression Scope { get; }
    
    public ScopeNode? Parent { get; }

    public List<ScopeNode> Children { get; } = new();

    public Variable? GetVariable(string variableName)
    {
        return !_variables.TryGetValue(variableName, out var variable) 
            ? null 
            : variable;
    }

    public Variable? GetVariableIncludingAncestors(string variableName)
    {
        if (!_variables.TryGetValue(variableName, out var variable))
            variable = Parent?.GetVariableIncludingAncestors(variableName);

        return variable;
    }

    public bool AddVariable(Variable variable)
    {
        return _variables.TryAdd(variable.Name, variable);
    }
    
    public FunctionBase? GetFunctionIncludingAncestors(string functionName, IEnumerable<Type> parameters)
    {
        return !_functions.TryGetValue(functionName, out var functions)
            ? Parent?.GetFunctionIncludingAncestors(functionName, parameters)
            : functions.FirstOrDefault(f => TypeEquals(f.ArgumentTypes, parameters.ToArray()));
    }

    public bool AddFunction(FunctionBase function)
    {
        if (!_functions.TryGetValue(function.Name, out var functions))
            _functions[function.Name] = functions = new List<FunctionBase>();

        if (functions.Any(f => TypeEquals(f.ArgumentTypes, function.ArgumentTypes)))
            return false;
        
        functions.Add(function);
        
        return true;
    }

    private static bool TypeEquals(Type[] a, Type[] b)
    {
        if (a.Length != b.Length)
            return false;

        return !a.Where((t, i) => !b[i].IsAssignableTo(t)).Any();
    }
}