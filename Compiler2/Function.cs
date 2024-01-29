namespace Compiler2;

public class Function
{
    private readonly int _parametersCount;
    private readonly SortedList<ushort, Variable> _variables = new();

    public Function(string name, ushort id, int parametersCount)
    {
        Name = name;
        ID = id;
        _parametersCount = parametersCount;
    }

    public string Name { get; }
    public ushort ID { get; }

    public IEnumerable<Variable> Parameters => _variables.Take(_parametersCount).Select(p => p.Value);
    public IEnumerable<Variable> Variables => _variables.Values;
    public Type? ReturnType { get; private set; }

    public ushort GetVariableID(Variable variable)
    {
        return _variables.GetKeyAtIndex(_variables.IndexOfValue(variable));
    }

    public void AddVariable(Variable variable)
    {
        _variables.Add((ushort) _variables.Count, variable);
    }

    public void SetReturnType(Type type)
    {
        ReturnType ??= type;

        if (ReturnType != type)
            throw new Exception($"Wrong return type for function {Name}"); //todo: introduce special exception
    }
}