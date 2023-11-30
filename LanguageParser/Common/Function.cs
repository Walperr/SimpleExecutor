namespace LanguageParser.Common;

//todo: refactor this
public abstract class FunctionBase
{
    public FunctionBase(string name, IEnumerable<Type> parameters)
    {
        Name = name;
        ArgumentTypes = parameters.ToArray();
    }
    
    public string Name { get; }
    public abstract Type ReturnType { get; }

    public Type[] ArgumentTypes { get; protected set; }

    public static FunctionBase Create(string name, Action<IList<object>> body, params Type[] parameterTypes)
    {
        return new Function(name, parameterTypes.Select((t, i) => new Variable($"arg_{i}", t)), body);
    }
    
    public static FunctionBase Create<T>(string name, Func<IList<object>, T> body, params Type[] parameterTypes)
    {
        return new Function<T>(name, parameterTypes.Select((t, i) => new Variable($"arg_{i}", t)), body);
    }
}

public sealed class Function<T> : FunctionBase
{
    private readonly Func<IList<object>, T> _body;

    public Function(string name, IEnumerable<Variable> arguments, Func<IList<object>,T> body)  : base(name, arguments.Select(o => o.Type))
    {
        _body = body;
    }

    public override Type ReturnType => typeof(T);

    public T Invoke(IList<object> arguments)
    {
        return _body.Invoke(arguments);
    }
}

public sealed class Function : FunctionBase
{
    private readonly Action<IList<object>> _body;

    public Function(string name, IEnumerable<Variable> arguments, Action<IList<object>> body) : base(name, arguments.Select(o => o.Type))
    {
        _body = body;
    }

    public override Type ReturnType => typeof(Empty);

    public void Invoke(IList<object> arguments)
    {
        _body.Invoke(arguments);
    }
}