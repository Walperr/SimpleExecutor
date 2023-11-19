using System;
using Avalonia;
using ReactiveUI;
using SimpleExecutor.ViewModels;

namespace SimpleExecutor.Models;

public sealed class Executor : ViewModelBase
{
    private Point _position;
    private double _angle;

    public Point Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value);
    }
    
    public double Angle
    {
        get => _angle;
        private set => this.RaiseAndSetIfChanged(ref _angle, value);
    }

    public void Move(double length)
    {
        var direction = new Vector(-Math.Sin(Angle * Math.PI / 180), Math.Cos(Angle * Math.PI / 180));

        Position += direction * length;
    }

    public void Rotate(double angle)
    {
        Angle += angle;
        if (Angle >= 360)
            Angle -= 360;
    }

    public void Reset()
    {
        Position = default;
        Angle = 0;
    }

    public void Jump(double x, double y)
    {
        Position = new Point(x, y);
    }
}