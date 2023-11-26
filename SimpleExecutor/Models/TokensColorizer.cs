using System;
using System.Linq;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using static SimpleExecutor.Models.SyntaxColors;
using Tokenizer = LanguageParser.Lexer.Tokenizer;

namespace SimpleExecutor.Models;

public sealed class TokensColorizer
{
    private readonly Action<int, int, Action<VisualLineElement>> _changeLinePart;
    private readonly DocumentLine _line;
    private readonly IStream<Token> _tokenStream;

    public TokensColorizer(IStream<Token> tokenStream, Action<int, int, Action<VisualLineElement>> changeLinePart,
        DocumentLine line)
    {
        _tokenStream = tokenStream;
        _changeLinePart = changeLinePart;
        _line = line;
    }

    public static void Colorize(string code, Action<int, int, Action<VisualLineElement>> changeLinePart, DocumentLine line)
    {
        var tokens = Tokenizer.Tokenize(code);

        new TokensColorizer(tokens, changeLinePart, line).Colorize();
    }

    private void Colorize()
    {
        IImmutableBrush? brush;
        while (_tokenStream.CanAdvance)
        {
            WalkLeadingTrivia(_tokenStream.Current);
            brush = GetTokenBrush(_tokenStream.Current);
            if (brush is not null)
                SetForeground(_tokenStream.Current, brush);
            _tokenStream.Advance();
        }
        
        WalkLeadingTrivia(_tokenStream.Current);
        brush = GetTokenBrush(_tokenStream.Current);
        if (brush is not null)
            SetForeground(_tokenStream.Current, brush);
    }

    private void WalkLeadingTrivia(Token token)
    {
        foreach (var trivia in token.LeadingTrivia.Where(trivia => trivia.IsComment))
            SetForeground(trivia, CommentBrush);
    }

    private void SetForeground(StringRange range, IBrush brush)
    {
        if (new StringRange(_line.Offset, _line.Length).Intersection(range) is { } intersection)
            _changeLinePart.Invoke(intersection.Start, intersection.End,
                e => e.TextRunProperties.SetForegroundBrush(brush));
    }

    private void SetForeground(Token token, IImmutableBrush brush)
    {
        WalkLeadingTrivia(token);
        var tokenRange = token.Range;

        if (token.IsString)
            tokenRange = new StringRange(tokenRange.Start - 1, tokenRange.Length + 2);
        
        SetForeground(tokenRange, brush);
    }

    private static IImmutableBrush? GetTokenBrush(Token token)
    {
        if (token.IsNumber)
            return NumberBrush;

        if (token.IsKeyword)
            return KeywordBrush;

        if (token.IsString)
            return StringBrush;

        if (token.IsComment)
            return CommentBrush;

        return null;
    }
}