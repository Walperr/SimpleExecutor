using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Skia;
using SimpleExecutor.Models;
using SkiaSharp;

namespace SimpleExecutor.Views;

public sealed partial class RobotView : UserControl
{
    private readonly SkiaCanvasDrawOperation _drawOperation = new();

    private Robot? _executor;
    
    public RobotView()
    {
        InitializeComponent();
        
        _drawOperation.OnRender += OnRender;
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_executor is not null)
            _executor.PropertyChanged -= ExecutorOnPropertyChanged;

        _executor = DataContext as Robot;

        if (_executor is not null)
            _executor.PropertyChanged += ExecutorOnPropertyChanged;

        base.OnDataContextChanged(e);
        
        InvalidateVisual();
    }

    private void ExecutorOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }
    
    private void OnRender(SKCanvas canvas, SKSurface surface)
    {
        if (_executor is null)
            return;

        using var paint = new SKPaint();

        paint.Color = _executor.Background;

        var bounds = Bounds.ToSKRect();

        var tileWidth = bounds.Width / _executor.Width;
        var tileHeight = bounds.Height / _executor.Height;

        for (var i = 0; i < _executor.Width; i++)
        for (var j = 0; j < _executor.Height; j++)
        {
            paint.Color = _executor.Tiles[i, j] ?? _executor.Background;
            canvas.DrawRect(tileWidth * i, tileHeight * j, tileWidth, tileHeight, paint);
        }

        paint.Color = SKColors.DimGray;
        canvas.DrawRect(tileWidth * _executor.Position.X, tileHeight * _executor.Position.Y, tileWidth, tileHeight, paint);
    }
    
    public override void Render(DrawingContext context)
    {
        _drawOperation.Bounds = Bounds;

        context.Custom(_drawOperation);
    }
}