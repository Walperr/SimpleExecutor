using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Skia;
using ReactiveUI;
using SimpleExecutor.ViewModels;
using SkiaSharp;

namespace SimpleExecutor.Models;

public sealed class Turtle : ViewModelBase
{
    private readonly List<Shape> _shapes = new();
    private double _angle;
    private SKColor _background = SKColors.White;
    private double _pixelHeight;
    private double _pixelWidth;
    private Polygon? _polygon;
    private Point _position;
    private int _thickness = 2;

    public SKColor LineColor { get; set; } = SKColors.Blue;
    public SKColor FillColor { get; set; } = SKColors.Blue;

    public IReadOnlyList<Shape> Shapes => _shapes;

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

    public SKColor Background
    {
        get => _background;
        set => this.RaiseAndSetIfChanged(ref _background, value);
    }

    public Point Position
    {
        get => _position;
        private set => this.RaiseAndSetIfChanged(ref _position, value);
    }

    public double Angle
    {
        get => _angle;
        private set => this.RaiseAndSetIfChanged(ref _angle, value);
    }

    public int Thickness
    {
        get => _thickness;
        set => this.RaiseAndSetIfChanged(ref _thickness, value);
    }

    public void BeginPolygon()
    {
        _polygon ??= new Polygon(FillColor, LineColor, Thickness);

        _shapes.Add(_polygon);
    }

    public void CompletePolygon()
    {
        if (_polygon is null)
            throw new InvalidOperationException("Cannot complete polygon. No polygons started");

        _polygon.Complete();
        _polygon = null;
        this.RaisePropertyChanged(nameof(Shapes));
    }

    public void Move(double length)
    {
        var direction = new Vector(-Math.Sin(Angle * Math.PI / 180), Math.Cos(Angle * Math.PI / 180));

        var prevPos = Position;

        Position += direction * length;

        DrawTrace(prevPos, Position);
    }

    private void DrawTrace(Point previous, Point current)
    {
        if (_polygon is not null)
        {
            if (_polygon.Points.Count == 0)
                _polygon.Points.Add(previous.ToSKPoint());

            _polygon.Points.Add(current.ToSKPoint());
        }

        _shapes.Add(new Line(previous.ToSKPoint(), current.ToSKPoint(), LineColor, Thickness));
    }

    public void Rotate(double angle)
    {
        Angle += angle;
        if (Angle >= 360)
            Angle -= 360;
    }

    public void Reset()
    {
        _shapes.Clear();
        Background = SKColors.White;
        FillColor = SKColors.Blue;
        LineColor = SKColors.Blue;
        Position = default;
        Angle = 0;
    }

    public void Jump(double x, double y)
    {
        Position = new Point(x, y);
    }

    public void MoveTo(double x, double y)
    {
        var prevPos = Position;

        Position = new Point(x, y);

        DrawTrace(prevPos, Position);
    }
}