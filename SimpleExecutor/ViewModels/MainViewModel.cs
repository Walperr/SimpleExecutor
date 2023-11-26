using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Threading;
using LanguageInterpreter;
using LanguageParser.Common;
using ReactiveUI;
using SimpleExecutor.Models;

namespace SimpleExecutor.ViewModels;

public class MainViewModel : ViewModelBase
{
    private CancellationTokenSource? _cancellation;
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
                Dispatcher.UIThread.Invoke(() => Executor.Move((double)args[0]));
                Task.Delay(StepDuration).Wait();
            });

        var rotateFunction = new Function("rotate", new[] { new Variable("angle", typeof(double)) },
            args =>
            {
                Dispatcher.UIThread.Invoke(() => Executor.Rotate((double)args[0]));
                Task.Delay(StepDuration).Wait();
            });

        var resetFunction = new Function("reset", Array.Empty<Variable>(),
            _ =>
            {
                Dispatcher.UIThread.Invoke(() => Executor.Reset());
                Task.Delay(StepDuration).Wait();
            });

        var jumpFunction = new Function("jump",
            new[] { new Variable("x", typeof(double)), new Variable("y", typeof(double)) },
            args =>
            {
                Dispatcher.UIThread.Invoke(() => Executor.Jump((double)args[0], (double)args[1]));
                Task.Delay(StepDuration).Wait();
            });

        var widthFunction = new Function<double>("getWidth", Array.Empty<Variable>(), args => Executor.PixelWidth);

        var heightFunction = new Function<double>("getHeight", Array.Empty<Variable>(), args => Executor.PixelHeight);

        var timeFunction =
            new Function<double>("getTime", Array.Empty<Variable>(), args => DateTime.UtcNow.Millisecond);
        
        var delayFunction = new Function("delay", new[] { new Variable("milliseconds", typeof(double)) }, args =>
        {
            var time = (double)args[0];
            Task.Delay((int)time).Wait();
        });

        var setStepFunction = new Function("setStep",
            new[] { new Variable("milliseconds", typeof(double)) },
            args => { StepDuration = (int)(double)args[0]; });

        var setColorFunction = new Function("setColor", new[] { new Variable("color", typeof(string)) }, args =>
        {
            var brush = (ISolidColorBrush)Brush.Parse((string)args[0]);

            Dispatcher.UIThread.Invoke(() => Executor.TraceColor = (IImmutableSolidColorBrush) brush.ToImmutable());
        });

        var setBackgroundFunction = new Function("setBackground", new[] {new Variable("color", typeof(string))}, args =>
        {
            var brush = (ISolidColorBrush)Brush.Parse((string)args[0]);

            Dispatcher.UIThread.Invoke(() => Executor.Background = (IImmutableSolidColorBrush) brush.ToImmutable());
        });

        TokensSyntaxColorizer = new TokensSyntaxColorizer(this);
        ExpressionSyntaxColorizer = new ExpressionSyntaxColorizer(this);

        Interpreter = InterpreterBuilder.CreateBuilder()
            .WithPredefinedFunction(printFunction)
            .WithPredefinedFunction(printLineFunction)
            .WithPredefinedFunction(sqrtFunction)
            .WithPredefinedFunction(absFunction)
            .WithPredefinedFunction(moveFunction)
            .WithPredefinedFunction(rotateFunction)
            .WithPredefinedFunction(resetFunction)
            .WithPredefinedFunction(jumpFunction)
            .WithPredefinedFunction(delayFunction)
            .WithPredefinedFunction(setStepFunction)
            .WithPredefinedFunction(setColorFunction)
            .WithPredefinedFunction(setBackgroundFunction)
            .WithPredefinedFunction(widthFunction)
            .WithPredefinedFunction(heightFunction)
            .WithPredefinedFunction(timeFunction)
            .Build();
    }

    public IInterpreter Interpreter { get; }

    private int StepDuration { get; set; } = 100;

    public Executor Executor { get; } = new();

    public ExpressionSyntaxColorizer ExpressionSyntaxColorizer { get; }
    public TokensSyntaxColorizer TokensSyntaxColorizer { get; }

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
// for(number i = 1; i < 1000; i++)
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
//     for (number i = 1; i < 4; i++)
//     {
//         move(size)
//         rotate(90)
//         size = size + 5
//     }
// }

// reset();
// number sunX = getWidth() / 2;
// number sunY = getHeight() / 2;
//
// // Этот странный код для того, чтобы нарисовать "солнце"
// setColor('transparent');
// jump(sunX - 0.5, sunY - 0.5);
//
// setColor('orange');
// for(number i = 0; i < 60; i++)
// {
//     move(1);
//     rotate(6)
// }
// setColor('transparent');
// move(0);
//
// // Проинициализировали параметры
// number sunWeight = 100;
//
// number x = sunX;
// number y = 20;
//
// number planetWeight = 1;
//
// number velocityX = 0-0.25;
// number velocityY = 0-0.1;
//
// // Поместили "планету" на место
// setColor('transparent');
// jump(x,y);
//
// number deltaTime = 10;
//
// setStep(deltaTime);
// setColor('blue');
//
// // Основной цикл, тут все считается
// while(true)
// {
//     number rX = x - sunX;
//     number rY = y - sunY;
// 	
//     number r = sqrt(rX * rX + rY * rY);
// 	    	
//     velocityX = velocityX - (sunWeight  * rX / (r * r * r)) * deltaTime;
//     velocityY = velocityY - (sunWeight * rY / (r * r * r)) * deltaTime;
// 		
//     x = x + velocityX * deltaTime;
//     y = y + velocityY * deltaTime;
// 	
//     jump(x, y);
// }

// reset()
// setBackground('black')
// setStep(10)
// setColor('transparent')
// jump(getWidth() * 0.5, getHeight() * 0.5)
//
// for (number i = 0; i <= 500; i = i + 1)
// {
//     if (i % 6 == 0)
//         setColor('yellow')
//     else if (i % 5 == 0)
//         setColor('orange')
//     else if (i % 4 == 0)
//         setColor('green')
//     else if (i % 3 == 0)
//         setColor('blue')
//     else if (i % 2 == 0)
//         setColor('purple')
//     else setColor('red')
// 	
//     move(i)
//     rotate(59)	
// }

// reset()
// setStep(1)
// setColor('red')
// jump(200,200)
// number la=30
// number ta=360/la
//
// repeat la times
// {
//     move(50)
//     rotate(-ta)
// }
// jump(330,100)
//
// repeat la times
// {
//     move(10)
//     rotate(-ta)
// }
//
// jump(450,100)
//
// repeat la times
// {
//     move(10)
//     rotate(-ta)
// }
//
// jump(410,250)
// rotate(-90)
//
// repeat 3 times
// {
//     move(50)
//     rotate(-120)
// }
//
// jump(410,300)
// number f=20
// number k=360/f
//
// rotate(90)
// repeat f / 2 times
// {   move(10)
//     rotate(-k)
// }