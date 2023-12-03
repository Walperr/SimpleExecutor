using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageInterpreter;
using LanguageParser.Common;
using ReactiveUI;
using SimpleExecutor.Libraries;
using SimpleExecutor.Models;

namespace SimpleExecutor.ViewModels;

public class ExecutorTabViewModel : ViewModelBase, IDisposable
{
    private CancellationTokenSource? _cancellation;
    private string _code = string.Empty;
    private ICommand? _interpretCommand;
    private string _output = string.Empty;
    private ICommand? _stopCommand;
    private string _name = "New Turtle";

    public ExecutorTabViewModel()
    {
        var printFunction = new Function("print", new[] {new Variable("text", typeof(object))},
            args => Output += args[0].ToString());

        var printLineFunction = new Function("printLine", new[] {new Variable("text", typeof(object))},
            args => Output += "\n" + args[0]);

        var delayFunction = new Function("delay", new[] {new Variable("milliseconds", typeof(double))}, args =>
        {
            var time = (double) args[0];
            Task.Delay((int) time).Wait();
        });

        var turtle = new TurtleLibrary(Executor);

        TokensSyntaxColorizer = new TokensSyntaxColorizer(this);
        ExpressionSyntaxColorizer = new ExpressionSyntaxColorizer(this);

        Interpreter = InterpreterBuilder.CreateBuilder()
            .WithPredefinedFunction(printFunction)
            .WithPredefinedFunction(printLineFunction)
            .WithPredefinedFunction(delayFunction)
            .WithPredefinedFunctions(ArrayLibrary.GetFunctions())
            .WithPredefinedFunctions(MathLibrary.GetMathFunctions())
            .WithPredefinedFunctions(turtle.GetFunctions())
            .Build();
    }

    private IInterpreter Interpreter { get; }

    public Executor Executor { get; } = new();

    public ExpressionSyntaxColorizer ExpressionSyntaxColorizer { get; }
    public TokensSyntaxColorizer TokensSyntaxColorizer { get; }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
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

    public ICommand InterpretCommand => _interpretCommand ??= ReactiveCommand.Create(async () =>
    {
        Output = string.Empty;

        try
        {
            var cancellation = new CancellationTokenSource();

            Interlocked.Exchange(ref _cancellation, cancellation)?.Cancel();

            var token = cancellation.Token;

            var value = await Task.Run(() =>
            {
                lock (cancellation)
                {
                    Interpreter.Initialize(Code);

                    if (!Interpreter.HasErrors)
                        return Interpreter.Interpret(token);

                    Output = Interpreter.Error.Message + Interpreter.Error.Range;
                    return Task.CompletedTask;
                }
            }, token);

            if (value.IsError)
                Output += "\n Error:\n" + value.Error.Message + value.Error.Range;
            else
                Output += "\n" + value.Value;
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    });

    public ICommand StopInterpreterCommand => _stopCommand ??= ReactiveCommand.Create(() =>
    {
        try
        {
            Interlocked.Exchange(ref _cancellation, null)?.Cancel();
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    });

    public void Dispose()
    {
        _cancellation?.Cancel();
        _cancellation?.Dispose();
    }
}