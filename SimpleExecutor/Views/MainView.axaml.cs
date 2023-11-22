using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit.Indentation;
using DynamicData;
using SimpleExecutor.Models;
using SimpleExecutor.ViewModels;

namespace SimpleExecutor.Views;

public partial class MainView : UserControl
{
    private ExpressionSyntaxColorizer? _syntaxColorizer;
    private TokensSyntaxColorizer? _tokensColorizer;
    
    public MainView()
    {
        InitializeComponent();

        Editor.ContextMenu = new ContextMenu
        {
            ItemsSource = new List<MenuItem>
            {
                new () {Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control)},
                new () {Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control)},
                new () {Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control)},
            }
        };
        Editor.Options.ShowBoxForControlCharacters = true;
        Editor.Options.ColumnRulerPositions = new[] { 80, 100 };
        Editor.Options.HighlightCurrentLine = true;
        Editor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
        Editor.TextChanged += EditorOnTextChanged;
    }

    private void EditorOnTextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel mainViewModel)
            mainViewModel.Code = Editor.Text;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        var lineTransformers = Editor.TextArea.TextView.LineTransformers;

        if (DataContext is MainViewModel viewModel)
        {
            lineTransformers.Remove(_syntaxColorizer);
            lineTransformers.Remove(_tokensColorizer);
            _syntaxColorizer = viewModel.ExpressionSyntaxColorizer;
            _tokensColorizer = viewModel.TokensSyntaxColorizer;
            lineTransformers.Add(_syntaxColorizer);
            lineTransformers.Add(_tokensColorizer);
        }
        
        base.OnDataContextChanged(e);
    }
}