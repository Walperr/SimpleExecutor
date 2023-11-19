namespace LanguageParser.Common;

//todo: refactor this
public abstract class FunctionBase
{
    public FunctionBase(string name, IEnumerable<Type> args)
    {
        Name = name;
        ArgumentTypes = args.ToArray();
    }
    
    public string Name { get; }
    public abstract Type ReturnType { get; }

    public Type[] ArgumentTypes { get; protected set; }
}

public sealed class Function<T> : FunctionBase
{
    private readonly Func<IEnumerable<object>, T> _body;
    public IEnumerable<Variable> Arguments { get; }

    public Function(string name, Variable[] arguments, Func<IEnumerable<object>,T> body)  : base(name, arguments.Select(o => o.GetType()))
    {
        _body = body;
        Arguments = arguments;
    }

    public override Type ReturnType => typeof(T);

    public T Invoke(IEnumerable<object> arguments)
    {
        return _body.Invoke(arguments);
    }
}

public sealed class Function : FunctionBase
{
    private readonly Action<IEnumerable<object>> _body;

    public Function(string name, IEnumerable<Variable> arguments, Action<IEnumerable<object>> body) : base(name, arguments.Select(o => o.GetType()))
    {
        _body = body;
    }

    public override Type ReturnType => typeof(Empty);

    public void Invoke(IEnumerable<object> arguments)
    {
        _body.Invoke(arguments);
    }
}