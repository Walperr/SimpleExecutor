using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Indentation;
using SimpleExecutor.Models;
using SimpleExecutor.ViewModels;

namespace SimpleExecutor.Views;

public partial class ExecutorTabView : UserControl
{
    private ExpressionSyntaxColorizer? _syntaxColorizer;
    private TokensSyntaxColorizer? _tokensColorizer;
    private ExecutorTabViewModel? _viewModel;

    private int _syncLocks;

    public ExecutorTabView()
    {
        InitializeComponent();

        Editor.ContextMenu = new ContextMenu
        {
            ItemsSource = new List<MenuItem>
            {
                new() {Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control)},
                new() {Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control)},
                new() {Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control)}
            }
        };
        Editor.Options.ShowBoxForControlCharacters = true;
        Editor.Options.ColumnRulerPositions = new[] {80, 100};
        Editor.Options.HighlightCurrentLine = true;
        Editor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
        Editor.TextChanged += EditorOnTextChanged;
    }

    private void EditorOnTextChanged(object? sender, EventArgs e)
    {
        try
        {
            if (Interlocked.Increment(ref _syncLocks) > 1)
                return;
        
            if (DataContext is ExecutorTabViewModel mainViewModel)
                mainViewModel.Code = Editor.Text;
        }
        finally
        {
            Interlocked.Decrement(ref _syncLocks);
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        var lineTransformers = Editor.TextArea.TextView.LineTransformers;

        if (_viewModel is not null)
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;

        _viewModel = null;

        if (DataContext is ExecutorTabViewModel viewModel)
        {
            lineTransformers.Remove(_syntaxColorizer);
            lineTransformers.Remove(_tokensColorizer);
            _syntaxColorizer = viewModel.ExpressionSyntaxColorizer;
            _tokensColorizer = viewModel.TokensSyntaxColorizer;
            lineTransformers.Add(_syntaxColorizer);
            lineTransformers.Add(_tokensColorizer);
            Editor.Text = viewModel.Code;
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            _viewModel = viewModel;
        }

        base.OnDataContextChanged(e);
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (Interlocked.Increment(ref _syncLocks) > 1)
                return;
        
            if (e.PropertyName is nameof(ExecutorTabViewModel.Code))
                Editor.Text = _viewModel!.Code;
        }
        finally
        {
            Interlocked.Decrement(ref _syncLocks);
        }
    }
}