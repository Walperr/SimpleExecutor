using System.Diagnostics.CodeAnalysis;
using LanguageInterpreter.Execution;
using LanguageParser;
using LanguageParser.Common;
using LanguageParser.Parser;
using LanguageParser.Visitors;

namespace LanguageInterpreter;

internal sealed class Interpreter : IInterpreter
{
    private ScopeNode? _root;

    internal List<Variable> PredefinedVariables { get; } = new();
    internal List<FunctionBase> PredefinedFunctions { get; } = new();
    
    public SyntaxException? Error { get; private set; }

    public void Initialize(string source)
    {
        Error = null;
        var root = ExpressionsParser.Parse(source);

        if (root.IsError)
        {
            Error = root.Error;
            return;
        }

        _root = DeclarationsCollector.Collect(root.Value) ??
                throw new InvalidOperationException("Cannot collect variables of non scope expression");

        foreach (var variable in PredefinedVariables) 
            _root.AddVariable(variable);

        foreach (var function in PredefinedFunctions) 
            _root.AddFunction(function);

        var type = TypeResolver.Resolve(_root);

        if (type.IsError)
            Error = type.Error;
    }

    public Result<SyntaxException, object> Interpret(CancellationToken? token)
    {
        if (_root is null)
            throw new InvalidOperationException("Interpreter is uninitialized");

        if (((IInterpreter)this).HasErrors)
            throw new InvalidOperationException("Cannot interpret invalid expression");

        var result = ExpressionEvaluator.Evaluate(_root, token);

        return result.IsError ? result.Error : result.Value;
    }
}