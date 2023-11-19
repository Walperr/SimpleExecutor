using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageInterpreter;
using LanguageParser;
using LanguageParser.Common;
using ReactiveUI;

namespace SimpleExecutor.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string _code = string.Empty;
    private string _output = string.Empty;

    private readonly IInterpreter _interpreter;

    public MainViewModel()
    {
        var printFunction = new Function("print", new[] {new Variable("text", typeof(object))}, args =>
        {
            Output += args.First().ToString();
        });
        
        var printLineFunction = new Function("printLine", new[] {new Variable("text", typeof(object))}, args =>
        {
            Output += "\n" + args.First();
        });

        var sqrtFunction = new Function<double>("sqrt", new[] {new Variable("d", typeof(double))},
            args => Math.Sqrt((double) args.First()));

        var absFunction = new Function<double>("abs", new[] {new Variable("d", typeof(double))},
            args => Math.Abs((double) args.First()));

        _interpreter = InterpreterBuilder.CreateBuilder()
            .WithPredefinedFunction(printFunction)
            .WithPredefinedFunction(printLineFunction)
            .WithPredefinedFunction(sqrtFunction)
            .WithPredefinedFunction(absFunction)
            .Build();
    }

    public string Code
    {
        get => _code;
        set => this.RaiseAndSetIfChanged(ref _code, value);
    }

    public string Output
    {
        get => _output;
        set => this.RaiseAndSetIfChanged(ref _output, value);
    }

    public ICommand? Interpret => ReactiveCommand.Create(() =>
    {
        Output = string.Empty;
        
        _interpreter.Initialize(_code);

        if (_interpreter.HasErrors)
        {
            Output = _interpreter.Error.Message + _interpreter.Error.Range;
            return;
        }

        var value = _interpreter.Interpret();

        if (value.IsError)
            Output += "\n Error:\n" + value.Error.Message + value.Error.Range;
        else
            Output += "\n Result:\n" + value.Value;
    });
}