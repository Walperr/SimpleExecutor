using LanguageParser.Expressions;

namespace Compiler;

public sealed class ScopeNode
{
    private readonly Dictionary<string, List<FunctionBuilder>> _functions = new();
    private readonly Dictionary<string, Variable> _variables = new(StringComparer.InvariantCultureIgnoreCase);

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
        return _variables.GetValueOrDefault(variableName);
    }

    public FunctionBuilder? GetFunction(string functionName, IEnumerable<Type> parameters)
    {
        return _functions.GetValueOrDefault(functionName)
            ?.FirstOrDefault(f => f.Parameters.Select(p => p.Type).SequenceEqual(parameters));
    }

    public Variable? GetVariableIncludingAncestors(string variableName)
    {
        if (!_variables.TryGetValue(variableName, out var variable))
            variable = Parent?.GetVariableIncludingAncestors(variableName);

        return variable;
    }

    public bool AddVariable(Type type, string name)
    {
        return _variables.TryAdd(name, new Variable(type, name));
    }

    public FunctionBuilder? GetFunctionIncludingAncestors(string functionName, IEnumerable<Type> parameters)
    {
        return !_functions.TryGetValue(functionName, out var functions)
            ? Parent?.GetFunctionIncludingAncestors(functionName, parameters)
            : functions.FirstOrDefault(f => f.Parameters.Select(p => p.Type).SequenceEqual(parameters));
    }

    public bool AddFunction(FunctionBuilder function)
    {
        if (!_functions.TryGetValue(function.Name, out var functions))
            _functions[function.Name] = functions = new List<FunctionBuilder>();

        if (functions.Any(f => f.Parameters.SequenceEqual(function.Parameters)))
            return false;

        functions.Add(function);

        return true;
    }
}