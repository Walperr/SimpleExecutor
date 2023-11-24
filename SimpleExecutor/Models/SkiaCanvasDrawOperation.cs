using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace SimpleExecutor.Models;

internal sealed class SkiaCanvasDrawOperation : ICustomDrawOperation
{
    public Action<SKCanvas, SKSurface>? OnRender;

    public bool Equals(ICustomDrawOperation? other)
    {
        return other == this;
    }

    public void Dispose()
    {
        // do nothing
    }

    public bool HitTest(Point p)
    {
        return true;
    }

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null)
            return;

        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;

        Render(canvas, lease.SkSurface!);
    }

    public Rect Bounds { get; set; }

    private void Render(SKCanvas canvas, SKSurface surface)
    {
        OnRender?.Invoke(canvas, surface);
    }
}
