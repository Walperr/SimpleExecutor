using LanguageParser.Common;
using LanguageParser.Expressions;

namespace LanguageParser;

public sealed class ScopeNode
{
    private readonly Dictionary<string, Variable> _variables = new (StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, FunctionBase> _functions = new(StringComparer.InvariantCultureIgnoreCase);

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
    
    public FunctionBase? GetFunctionIncludingAncestors(string functionName)
    {
        if (!_functions.TryGetValue(functionName, out var function))
            function = Parent?.GetFunctionIncludingAncestors(functionName);

        return function;
    }

    public bool AddFunction(FunctionBase function)
    {
        return _functions.TryAdd(function.Name, function);
    }
}