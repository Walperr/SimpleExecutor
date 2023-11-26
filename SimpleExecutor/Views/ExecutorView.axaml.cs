using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Skia;
using Avalonia.Threading;
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
            _executor.PropertyChanged += ExecutorOnPropertyChanged;

        base.OnDataContextChanged(e);
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

        paint.Color = _executor.Background.Color.ToSKColor();

        canvas.DrawRect(Bounds.ToSKRect(), paint);
        
        paint.IsAntialias = true;
        
        paint.StrokeWidth = 2;
        paint.PathEffect = SKPathEffect.CreateDash(Array.Empty<float>(), 0);
        paint.Color = _executor.TraceColor.Color.ToSKColor();
        paint.Style = SKPaintStyle.Stroke;

        var trace = _executor.Trace;
        
        for (var i = 1; i < trace.Count; i++)
        {
            paint.Color = trace[i].brush.Color.ToSKColor();
            canvas.DrawLine(trace[i - 1].point.ToSKPoint(), trace[i].point.ToSKPoint(), paint);
        }
        
        paint.Style = SKPaintStyle.StrokeAndFill;
        paint.Color = SKColors.Red;

        canvas.RotateDegrees((float)_executor.Angle, (float)_executor.Position.X, (float)_executor.Position.Y);
        
        canvas.DrawRect((float)_executor.Position.X - 5, (float)_executor.Position.Y - 5, 10, 10, paint);
        
        paint.Color = SKColors.Green;
        canvas.DrawCircle((float)_executor.Position.X, (float)_executor.Position.Y + 5, 2, paint);
        
        canvas.RotateDegrees(-(float)_executor.Angle, (float)_executor.Position.X, (float)_executor.Position.Y);
    }
    
    public override void Render(DrawingContext context)
    {
        _drawOperation.Bounds = Bounds;

        context.Custom(_drawOperation);
    }
}