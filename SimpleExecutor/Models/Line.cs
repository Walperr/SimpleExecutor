using System.Collections.Generic;
using SkiaSharp;

namespace SimpleExecutor.Models;

public abstract class Shape
{
    protected Shape(SKColor color, int thickness)
    {
        Color = color;
        Thickness = thickness;
    }

    public SKColor Color { get; }
    
    public int Thickness { get; }
}

public class Line : Shape
{
    public Line(SKPoint start, SKPoint end, SKColor color, int thickness) : base(color, thickness)
    {
        Start = start;
        End = end;
    }

    public SKPoint Start { get; }
    public SKPoint End { get; }
}

public class Polygon : Shape
{
    public Polygon(SKColor fillColor, SKColor color, int thickness) : base(color, thickness)
    {
        FillColor = fillColor;
    }

    public bool IsCompleted { get; private set; }

    public List<SKPoint> Points { get; } = new();
    
    public SKColor FillColor { get; }

    public void Complete()
    {
        IsCompleted = true;
    }
}