namespace Compiler;

public class Function
{
    private readonly int _parametersCount;
    private readonly SortedList<int, Variable> _variables = new();

    public Function(string name, int id, int parametersCount)
    {
        Name = name;
        ID = id;
        _parametersCount = parametersCount;
    }

    public string Name { get; }
    public int ID { get; }

    public IEnumerable<Variable> Parameters => _variables.Take(_parametersCount).Select(p => p.Value);
    public IEnumerable<Variable> Variables => _variables.Values;
    public Type? ReturnType { get; private set; }

    public int ParametersCount => _parametersCount;
    public int VariablesCount => _variables.Count;

    public int GetVariableID(Variable variable)
    {
        return _variables.GetKeyAtIndex(_variables.IndexOfValue(variable));
    }

    public void AddVariable(Variable variable)
    {
        _variables.Add(_variables.Count, variable);
    }

    public void SetReturnType(Type type)
    {
        ReturnType ??= type;

        if (ReturnType != type)
            throw new Exception($"Wrong return type for function {Name}"); //todo: introduce special exception
    }
}