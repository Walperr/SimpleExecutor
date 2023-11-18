using System.Diagnostics.CodeAnalysis;

namespace LanguageParser;

public class Variable
{
    public Variable(string name, Type type)
    {
        Name = name;
        Type = type;
    }
    
    public string Name { get; }
    
    public object? Value { get; private set; }

    public void UnsetValue()
    {
        Value = null;
        IsSet = false;
    }

    [MemberNotNull(nameof(Value))]
    public void SetValue(object value)
    {
        if (value.GetType() != Type)
            throw new InvalidOperationException($"Cannot set  {value.GetType()} value to variable of type {Type}");
        
        Value = value;
        IsSet = true;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSet { get; private set; }

    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsUnset => !IsSet;
    
    public bool IsDeclared { get; set; }
    
    public Type Type { get; }
}