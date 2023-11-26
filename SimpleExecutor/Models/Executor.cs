using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using ReactiveUI;
using SimpleExecutor.ViewModels;

namespace SimpleExecutor.Models;

public sealed class Executor : ViewModelBase
{
    private double _angle;
    private double _pixelHeight;
    private double _pixelWidth;
    private Point _position;
    private IImmutableSolidColorBrush _background = Brushes.White;
    public List<(Point point, IImmutableSolidColorBrush brush)> Trace { get; } = new();

    public double PixelWidth
    {
        get => _pixelWidth;
        set => this.RaiseAndSetIfChanged(ref _pixelWidth, value);
    }

    public double PixelHeight
    {
        get => _pixelHeight;
        set => this.RaiseAndSetIfChanged(ref _pixelHeight, value);
    }

    public IImmutableSolidColorBrush Background
    {
        get => _background;
        set => this.RaiseAndSetIfChanged(ref _background, value);
    }

    public IImmutableSolidColorBrush TraceColor { get; set; } = Brushes.Blue;

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
        
        Trace.Add((new Point(Position.X, Position.Y), TraceColor));
    }

    public void Rotate(double angle)
    {
        Angle += angle;
        if (Angle >= 360)
            Angle -= 360;
    }

    public void Reset()
    {
        Trace.Clear();
        Position = default;
        Angle = 0;
    }

    public void Jump(double x, double y)
    {
        Position = new Point(x, y);
        
        Trace.Add((new Point(Position.X, Position.Y), Brushes.Transparent));
    }
}