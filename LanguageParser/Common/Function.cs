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
    private readonly Func<IList<object>, T> _body;
    public IEnumerable<Variable> Arguments { get; }

    public Function(string name, Variable[] arguments, Func<IList<object>,T> body)  : base(name, arguments.Select(o => o.Type))
    {
        _body = body;
        Arguments = arguments;
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