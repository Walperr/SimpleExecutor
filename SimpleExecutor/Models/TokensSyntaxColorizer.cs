using System;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using SimpleExecutor.ViewModels;

namespace SimpleExecutor.Models;

public sealed class TokensSyntaxColorizer : DocumentColorizingTransformer
{
    private readonly MainViewModel _viewModel;

    public TokensSyntaxColorizer(MainViewModel viewModel)
    {
        _viewModel = viewModel;
    }
    
    protected override void ColorizeLine(DocumentLine line)
    {
        try
        {
            TokensColorizer.Colorize(_viewModel.Code, ChangeLinePart, line);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}