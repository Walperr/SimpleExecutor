using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using LanguageInterpreter;
using LanguageParser;
using LanguageParser.Common;
using ReactiveUI;
using SimpleExecutor.Models;

namespace SimpleExecutor.ViewModels;

public class MainViewModel : ViewModelBase
{
    private CancellationTokenSource? _cancellation;

    private readonly IInterpreter _interpreter;
    private string _code = string.Empty;
    private ICommand? _interpretCommand;
    private string _output = string.Empty;
    private ICommand? _stopCommand;

    public MainViewModel()
    {
        var printFunction = new Function("print", new[] { new Variable("text", typeof(object)) },
            args => Output += args[0].ToString());

        var printLineFunction = new Function("printLine", new[] { new Variable("text", typeof(object)) },
            args => Output += "\n" + args[0]);

        var sqrtFunction = new Function<double>("sqrt", new[] { new Variable("d", typeof(double)) },
            args => Math.Sqrt((double)args[0]));

        var absFunction = new Function<double>("abs", new[] { new Variable("d", typeof(double)) },
            args => Math.Abs((double)args[0]));

        var moveFunction = new Function("move", new[] { new Variable("length", typeof(double)) },
            args =>
            {
                Executor.Move((double)args[0]);
                Task.Delay(StepDuration).Wait();
            });

        var rotateFunction = new Function("rotate", new[] { new Variable("angle", typeof(double)) },
            args =>
            {
                Executor.Rotate((double)args[0]);
                Task.Delay(StepDuration).Wait();
            });

        var resetFunction = new Function("reset", Array.Empty<Variable>(),
            _ =>
            {
                Executor.Reset();
                Task.Delay(StepDuration).Wait();
            });

        var jumpFunction = new Function("jump",
            new[] { new Variable("x", typeof(double)), new Variable("y", typeof(double)) },
            args =>
            {
                Executor.Jump((double)args[0], (double)args[1]);
                Task.Delay(StepDuration).Wait();
            });

        var delayFunction = new Function("delay", new[] { new Variable("milliseconds", typeof(double)) }, args =>
        {
            var time = (double)args[0];
            Task.Delay((int)time).Wait();
        });

        var setStepDurationFunction = new Function("setStepDuration",
            new[] { new Variable("milliseconds", typeof(double)) },
            args => { StepDuration = (int)(double)args[0]; });

        var setColorFunction = new Function("setColor", new[] { new Variable("color", typeof(string)) }, args =>
        {
            var brush = Brush.Parse((string)args[0]);

            Executor.TraceColor = brush;
        });

        _interpreter = InterpreterBuilder.CreateBuilder()
            .WithPredefinedFunction(printFunction)
            .WithPredefinedFunction(printLineFunction)
            .WithPredefinedFunction(sqrtFunction)
            .WithPredefinedFunction(absFunction)
            .WithPredefinedFunction(moveFunction)
            .WithPredefinedFunction(rotateFunction)
            .WithPredefinedFunction(resetFunction)
            .WithPredefinedFunction(jumpFunction)
            .WithPredefinedFunction(delayFunction)
            .WithPredefinedFunction(setStepDurationFunction)
            .WithPredefinedFunction(setColorFunction)
            .Build();
    }

    private int StepDuration { get; set; } = 100;

    public Executor Executor { get; } = new();

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
                    _interpreter.Initialize(_code);

                    if (!_interpreter.HasErrors)
                        return _interpreter.Interpret(token);

                    Output = _interpreter.Error.Message + _interpreter.Error.Range;
                    return Task.CompletedTask;
                }
            }, token);

            if (value.IsError)
                Output += "\n Error:\n" + value.Error.Message + value.Error.Range;
            else
                Output += "\n Result:\n" + value.Value;
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
}

// setColor("Transparent")
// jump(350,310)
//
// for(number i = 1; i < 1000; i = i + 1)
// {
//     rotate(15)
//     move(i * 0.2)
//
//     if (i % 3 == 0)
//         setColor("red")
//     else if (i % 2 == 0)
//         setColor("blue")
//     else
//         setColor("green")
// }

// setColor('transparent')
// jump(350,310)
//
// setColor('blue')
//
// for (number size =6; size <= 146; size = size + 20)
// {
//     for (number i = 1; i < 4; i = i + 1)
//     {
//         move(size)
//         rotate(90)
//         size = size + 5
//     }
// }