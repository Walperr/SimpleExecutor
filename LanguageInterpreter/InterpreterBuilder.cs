using LanguageParser;
using LanguageParser.Common;

namespace LanguageInterpreter;

public sealed class InterpreterBuilder
{
    private Interpreter _interpreter = new();

    public InterpreterBuilder WithPredefinedVariable(Variable variable)
    {
        _interpreter.PredefinedVariables.Add(variable);

        return this;
    }

    public static InterpreterBuilder CreateBuilder() => new();

    public InterpreterBuilder WithPredefinedVariables(IEnumerable<Variable> variables)
    {
        foreach (var variable in variables) 
            _interpreter.PredefinedVariables.Add(variable);
        
        return this;
    }

    public InterpreterBuilder WithPredefinedFunction(FunctionBase function)
    {
        _interpreter.PredefinedFunctions.Add(function);

        return this;
    }

    public InterpreterBuilder WithPredefinedFunctions(IEnumerable<FunctionBase> functions)
    {
        foreach (var function in functions) 
            _interpreter.PredefinedFunctions.Add(function);

        return this;
    }

    public IInterpreter Build()
    {
        var interpreter = _interpreter;
        _interpreter = new();
        return interpreter;
    }
}