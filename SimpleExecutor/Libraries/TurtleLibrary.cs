using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Skia;
using Avalonia.Threading;
using LanguageParser.Common;
using SimpleExecutor.Models;

namespace SimpleExecutor.Libraries;

public sealed class TurtleLibrary
{
    private readonly Dispatcher _dispatcher = Dispatcher.UIThread;
    private readonly Executor _turtle;
    public int Delay { get; private set; } = 100;

    public TurtleLibrary(Executor turtle)
    {
        _turtle = turtle;
    }

    public IEnumerable<FunctionBase> GetFunctions()
    {
        yield return FunctionBase.Create("setBackground", args =>
        {
            var color = args[0].ToString();

            _dispatcher.Invoke(() => _turtle.Background = Color.Parse(color!).ToSKColor());
        }, typeof(string));

        yield return FunctionBase.Create("setColor", args =>
        {
            var color = args[0].ToString();

            _dispatcher.Invoke(() => _turtle.LineColor = Color.Parse(color!).ToSKColor());
        }, typeof(string));

        yield return FunctionBase.Create("setFillColor", args =>
        {
            var color = args[0].ToString();

            _dispatcher.Invoke(() => _turtle.FillColor = Color.Parse(color!).ToSKColor());
        }, typeof(string));

        yield return FunctionBase.Create("beginPolygon", _ => _turtle.BeginPolygon());

        yield return FunctionBase.Create("completePolygon", _ => _turtle.CompletePolygon());

        yield return FunctionBase.Create("move", args =>
        {
            _dispatcher.Invoke(() => _turtle.Move((double) args[0]));
            Task.Delay(Delay).Wait();
        }, typeof(double));

        yield return FunctionBase.Create("moveTo", args =>
        {
            _dispatcher.Invoke(() => _turtle.MoveTo((double) args[0], (double) args[1]));
            Task.Delay(Delay).Wait();
        }, typeof(double), typeof(double));

        yield return FunctionBase.Create("jump", args =>
        {
            _dispatcher.Invoke(() => _turtle.Jump((double) args[0], (double) args[1]));
            Task.Delay(Delay).Wait();
        }, typeof(double), typeof(double));

        yield return FunctionBase.Create("reset", _ => _dispatcher.Invoke(() => _turtle.Reset()));

        yield return FunctionBase.Create("rotate", args =>
            {
                _dispatcher.Invoke(() => _turtle.Rotate((double) args[0]));
                Task.Delay(Delay).Wait();
            },
            typeof(double));

        yield return FunctionBase.Create("setStep", args =>
        {
            Delay = (int) (double) args[0];
        }, typeof(double));

        yield return FunctionBase.Create("getWidth", _ => _turtle.PixelWidth);

        yield return FunctionBase.Create("getHeight", _ => _turtle.PixelHeight);

        yield return FunctionBase.Create("getPosX", _ => _turtle.Position.X);
        
        yield return FunctionBase.Create("getPosY", _ => _turtle.Position.Y);

        yield return FunctionBase.Create("setWidth",
            args =>
            {
                _dispatcher.Invoke(() => _turtle.Thickness = (int) (double) args[0]);
            }, typeof(double));
    }
}