using System;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using LanguageParser.Parser;
using SimpleExecutor.ViewModels;

namespace SimpleExecutor.Models;

public sealed class ExpressionSyntaxColorizer : DocumentColorizingTransformer
{
    private readonly MainViewModel _viewModel;

    public ExpressionSyntaxColorizer(MainViewModel viewModel)
    {
        _viewModel = viewModel;
    }
    
    protected override void ColorizeLine(DocumentLine line)
    {
        try
        {
            var result = ExpressionsParser.Parse(_viewModel.Code);
        
            if (result.IsError)
                return;
            
            ExpressionsColorizer.Colorize(ChangeLinePart, line, result.Value);
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}