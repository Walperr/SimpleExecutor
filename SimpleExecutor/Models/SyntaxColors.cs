using Avalonia.Media;

namespace SimpleExecutor.Models;

public static class SyntaxColors
{
    public static readonly IImmutableBrush FunctionBrush =
        new SolidColorBrush(Color.FromRgb(21, 139, 134)).ToImmutable();

    public static readonly IImmutableBrush KeywordBrush = new SolidColorBrush(Color.FromRgb(80, 125, 233)).ToImmutable();

    public static readonly IImmutableBrush StringBrush =
        new SolidColorBrush(Color.FromRgb(196, 158, 107)).ToImmutable();

    public static readonly IImmutableBrush NumberBrush =
        new SolidColorBrush(Color.FromRgb(237, 148, 192)).ToImmutable();

    public static readonly IImmutableBrush OperatorBrush =
        new SolidColorBrush(Color.FromRgb(50,50,50)).ToImmutable();

    public static readonly IImmutableBrush CommentBrush =
        new SolidColorBrush(Color.FromRgb(81, 115, 66)).ToImmutable();

    public static readonly IImmutableBrush ForegroundBrush =
        new SolidColorBrush(Color.FromRgb(50,50,50)).ToImmutable();
}