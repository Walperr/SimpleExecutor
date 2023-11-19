using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using SimpleExecutor.Models;

namespace SimpleExecutor.Views;

public partial class DrawingCanvas : UserControl
{
    private Executor? _executor;
    public DrawingCanvas()
    {
        InitializeComponent();
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

    private Point _currentPosition;

    private void ExecutorOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Executor.Position))
            return;

        Dispatcher.UIThread.Invoke(() =>
        {
            var line = new Line
            {
                Stroke = _executor!.TraceColor, StrokeThickness = 2, StartPoint = _currentPosition,
                EndPoint = _executor!.Position
            };
            
            Canvas.Children.Add(line);
        });
        
        _currentPosition = _executor!.Position;
    }
}