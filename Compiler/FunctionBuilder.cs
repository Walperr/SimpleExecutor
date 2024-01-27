namespace Compiler;

public class FunctionBuilder
{
    private readonly BinaryWriter _writer;
    private readonly MemoryStream _byteCode = new();
    private readonly int _parametersCount;
    private readonly SortedList<ushort, Variable> _variables = new();
    private ushort _id;

    public FunctionBuilder(string name, int parametersCount)
    {
        Name = name;
        _parametersCount = parametersCount;
        _writer = new BinaryWriter(_byteCode);
    }

    public IEnumerable<Variable> Parameters => _variables.Take(_parametersCount).Select(p => p.Value);
    public IEnumerable<Variable> Variables => _variables.Values;
    public string Name { get; }
    public Type? ReturnType { get; private set; }

    public BinaryWriter CodeWriter => _writer;

    public ushort GetVariableID(Variable variable)
    {
        return _variables.GetKeyAtIndex(_variables.IndexOfValue(variable));
    }

    public void AddVariable(Type type, string name)
    {
        _variables.Add((ushort)_variables.Count, new Variable(type, name));
    }

    public void SetReturnType(Type type)
    {
        ReturnType ??= type;

        if (ReturnType != type)
            throw new Exception($"Wrong return type for function {Name}"); //todo: introduce special exception
    }

    public void SetID(ushort id)
    {
        _id = id;
    }

    public Function Build()
    {
        return new Function
        {
            Name = Name,
            ID = _id,
            Variables = _variables,
            Parameters = _variables.Take(_parametersCount).Select(p => p.Value).ToArray(),
            ReturnType = ReturnType ?? throw new Exception("Function should have return type"),
            Instructions = _byteCode.GetBuffer()
        };
    }
}

public record Variable(Type Type, string Name);