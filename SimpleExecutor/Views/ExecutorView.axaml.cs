using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Skia;
using SimpleExecutor.Models;
using SkiaSharp;

namespace SimpleExecutor.Views;

public partial class ExecutorView : UserControl
{
    private readonly SkiaCanvasDrawOperation _drawOperation = new();
    private Executor? _executor;

    public ExecutorView()
    {
        InitializeComponent();

        _drawOperation.OnRender += OnRender;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_executor is not null)
            _executor.PropertyChanged -= ExecutorOnPropertyChanged;

        _executor = DataContext as Executor;

        if (_executor is not null)
        {
            _executor.PropertyChanged += ExecutorOnPropertyChanged;
            _executor.PixelWidth = Bounds.Width;
            _executor.PixelHeight = Bounds.Height;
        }

        base.OnDataContextChanged(e);
        
        InvalidateVisual();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        if (_executor is not null)
        {
            _executor.PixelWidth = Bounds.Width;
            _executor.PixelHeight = Bounds.Height;
        }

        base.OnSizeChanged(e);
    }

    private void ExecutorOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Executor.PixelWidth) && e.PropertyName != nameof(Executor.PixelHeight))
            InvalidateVisual();
    }

    private void OnRender(SKCanvas canvas, SKSurface surface)
    {
        if (_executor is null)
            return;

        using var paint = new SKPaint();

        paint.Color = _executor.Background;
        canvas.DrawRect(Bounds.ToSKRect(), paint);
        paint.IsAntialias = true;

        foreach (var shape in _executor.Shapes)
            switch (shape)
            {
                case Line line:
                    paint.StrokeWidth = line.Thickness;
                    paint.Color = line.Color;
                    canvas.DrawLine(line.Start, line.End, paint);
                    break;
                case Polygon {IsCompleted: true} polygon:
                    var fill = new SKPath();
                    fill.MoveTo(polygon.Points[0]);

                    foreach (var point in polygon.Points.Skip(1)) 
                        fill.LineTo(point);

                    paint.Style = SKPaintStyle.Fill;
                    paint.StrokeWidth = polygon.Thickness;
                    paint.Color = polygon.FillColor;
                    canvas.DrawPath(fill, paint);
                    break;
            }

        // draw turtle

        paint.Style = SKPaintStyle.StrokeAndFill;
        paint.Color = SKColors.Red;

        canvas.RotateDegrees((float) _executor.Angle, (float) _executor.Position.X, (float) _executor.Position.Y);

        canvas.DrawRect((float) _executor.Position.X - 5, (float) _executor.Position.Y - 5, 10, 10, paint);

        paint.Color = SKColors.Green;
        canvas.DrawCircle((float) _executor.Position.X, (float) _executor.Position.Y + 5, 2, paint);

        canvas.RotateDegrees(-(float) _executor.Angle, (float) _executor.Position.X, (float) _executor.Position.Y);
    }

    public override void Render(DrawingContext context)
    {
        _drawOperation.Bounds = Bounds;

        context.Custom(_drawOperation);
    }
}