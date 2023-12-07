using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Skia;
using Avalonia.Threading;
using LanguageInterpreter;
using LanguageParser.Common;
using SimpleExecutor.Models;

namespace SimpleExecutor.ViewModels;

public sealed class TurtleTabViewModel : TabBase
{
    private readonly Dispatcher _dispatcher = Dispatcher.UIThread;

    private int StepDelay { get; set; } = 100;
    public Turtle Turtle { get; } = new();

    protected override void OnInterpreterConfigured(InterpreterBuilder builder)
    {
        builder.WithPredefinedFunctions(GetFunctions());

        base.OnInterpreterConfigured(builder);
    }

    private IEnumerable<FunctionBase> GetFunctions()
    {
        yield return FunctionBase.Create("setBackground", args =>
        {
            var color = args[0].ToString();

            _dispatcher.Invoke(() => Turtle.Background = Color.Parse(color!).ToSKColor());
        }, typeof(string));

        yield return FunctionBase.Create("setColor", args =>
        {
            var color = args[0].ToString();

            _dispatcher.Invoke(() => Turtle.LineColor = Color.Parse(color!).ToSKColor());
        }, typeof(string));

        yield return FunctionBase.Create("setFillColor", args =>
        {
            var color = args[0].ToString();

            _dispatcher.Invoke(() => Turtle.FillColor = Color.Parse(color!).ToSKColor());
        }, typeof(string));

        yield return FunctionBase.Create("beginPolygon", _ => Turtle.BeginPolygon());

        yield return FunctionBase.Create("completePolygon", _ => _dispatcher.Invoke(() => Turtle.CompletePolygon()));

        yield return FunctionBase.Create("move", args =>
        {
            _dispatcher.Invoke(() => Turtle.Move((double)args[0]));
            Task.Delay(StepDelay).Wait();
        }, typeof(double));

        yield return FunctionBase.Create("moveTo", args =>
        {
            _dispatcher.Invoke(() => Turtle.MoveTo((double)args[0], (double)args[1]));
            Task.Delay(StepDelay).Wait();
        }, typeof(double), typeof(double));

        yield return FunctionBase.Create("jump", args =>
        {
            _dispatcher.Invoke(() => Turtle.Jump((double)args[0], (double)args[1]));
            Task.Delay(StepDelay).Wait();
        }, typeof(double), typeof(double));

        yield return FunctionBase.Create("reset", _ => _dispatcher.Invoke(() => Turtle.Reset()));

        yield return FunctionBase.Create("rotate", args =>
            {
                _dispatcher.Invoke(() => Turtle.Rotate((double)args[0]));
                Task.Delay(StepDelay).Wait();
            },
            typeof(double));

        yield return FunctionBase.Create("setStep", args => { StepDelay = (int)(double)args[0]; }, typeof(double));

        yield return FunctionBase.Create("getWidth", _ => Turtle.PixelWidth);

        yield return FunctionBase.Create("getHeight", _ => Turtle.PixelHeight);

        yield return FunctionBase.Create("getPosX", _ => Turtle.Position.X);

        yield return FunctionBase.Create("getPosY", _ => Turtle.Position.Y);

        yield return FunctionBase.Create("setWidth",
            args => { _dispatcher.Invoke(() => Turtle.Thickness = (int)(double)args[0]); }, typeof(double));
    }
}