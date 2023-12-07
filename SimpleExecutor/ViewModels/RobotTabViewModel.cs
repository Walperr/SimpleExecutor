using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Skia;
using Avalonia.Threading;
using LanguageInterpreter;
using LanguageParser.Common;
using SimpleExecutor.Models;

namespace SimpleExecutor.ViewModels;

public sealed class RobotTabViewModel : TabBase
{
    private readonly Dispatcher _dispatcher = Dispatcher.UIThread;

    public Robot Robot { get; } = new();

    private int StepDelay { get; set; } = 100;

    protected override void OnInterpreterConfigured(InterpreterBuilder builder)
    {
        builder.WithPredefinedFunctions(GetFunctions());

        base.OnInterpreterConfigured(builder);
    }

    public IEnumerable<FunctionBase> GetFunctions()
    {
        yield return FunctionBase.Create("setSize",
            args => _dispatcher.Invoke(() => Robot.SetSize((int)(double)args[0], (int)(double)args[1])), typeof(double),
            typeof(double));

        yield return FunctionBase.Create("move", args =>
        {
            _dispatcher.Invoke(() => Robot.Move((int)(double)args[0]));
            Task.Delay(StepDelay).Wait();
        }, typeof(double));

        yield return FunctionBase.Create("turnLeft", _ =>
        {
            Robot.TurnLeft();
            Task.Delay(StepDelay).Wait();
        });

        yield return FunctionBase.Create("turnRight", _ =>
        {
            Robot.TurnRight();
            Task.Delay(StepDelay).Wait();
        });

        yield return FunctionBase.Create("setColor",
            args => { Robot.Color = Color.Parse(args[0].ToString() ?? string.Empty).ToSKColor(); }, typeof(string));

        yield return FunctionBase.Create("setBackground",
            args =>
            {
                Robot.Background = Color.Parse(args[0].ToString() ?? string.Empty).ToSKColor();
            }, typeof(string));

        yield return FunctionBase.Create("getWidth", _ => Robot.Width);

        yield return FunctionBase.Create("getHeight", _ => Robot.Height);

        yield return FunctionBase.Create("setStep", args => { StepDelay = (int)(double)args[0]; }, typeof(double));

        yield return FunctionBase.Create("reset", args => _dispatcher.Invoke(() => Robot.Reset()));
    }
}