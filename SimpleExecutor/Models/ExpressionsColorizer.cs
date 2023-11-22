using System;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Lexer;
using LanguageParser.Visitors;
using static SimpleExecutor.Models.SyntaxColors;

namespace SimpleExecutor.Models;

public sealed class ExpressionsColorizer : ExpressionVisitor
{
    private readonly Action<int, int, Action<VisualLineElement>> _changeLinePart;
    private readonly DocumentLine _line;

    private ExpressionsColorizer(Action<int, int, Action<VisualLineElement>> changeLinePart, DocumentLine line)
    {
        _changeLinePart = changeLinePart;
        _line = line;
    }

    public static void Colorize(Action<int, int, Action<VisualLineElement>> changeLinePart, DocumentLine line,
        ExpressionBase expression)
    {
        var colorizer = new ExpressionsColorizer(changeLinePart, line);
        colorizer.Visit(expression);

        foreach (var trivia in expression.TrailingTrivia) 
            colorizer.VisitTrivia(trivia);
    } 

    public override void VisitTrivia(Token trivia)
    {
        if (trivia.IsComment)
            SetForeground(trivia, CommentBrush);
    }

    public override void VisitBinary(BinaryExpression expression)
    {
        Visit(expression.Left);
        SetForeground(expression.Operator.Token, OperatorBrush);
        Visit(expression.Right);
    }

    public override void VisitConstant(ConstantExpression expression)
    {
        SetForeground(expression.Token, GetTokenBrush(expression.Token));
    }

    public override void VisitFor(ForExpression expression)
    {
        SetForeground(expression.ForToken, KeywordBrush);
        if (expression.Initialization is not null)
            Visit(expression.Initialization);

        if (expression.Condition is not null)
            Visit(expression.Condition);

        if (expression.Step is not null)
            Visit(expression.Step);

        Visit(expression.Body);
    }

    public override void VisitIf(IfExpression expression)
    {
        SetForeground(expression.IfToken, KeywordBrush);
        Visit(expression.Condition);

        Visit(expression.ThenBranch);

        if (expression.ElseToken is not null)
            SetForeground(expression.ElseToken, KeywordBrush);

        if (expression.ElseBranch is not null)
            Visit(expression.ElseBranch);
    }

    public override void VisitInvocation(InvocationExpression expression)
    {
        if (expression.Function is ConstantExpression constant)
            SetForeground(constant.Token, FunctionBrush);
        else
            Visit(expression.Function);

        foreach (var argument in expression.Arguments)
            Visit(argument);
    }

    public override void VisitParenthesized(ParenthesizedExpression expression)
    {
        Visit(expression.Expression);
    }

    public override void VisitRepeat(RepeatExpression expression)
    {
        SetForeground(expression.RepeatToken, KeywordBrush);

        Visit(expression.Body);

        SetForeground(expression.UntilToken, KeywordBrush);

        Visit(expression.Condition);
    }

    public override void VisitScope(ScopeExpression expression)
    {
        foreach (var innerExpression in expression.InnerExpressions)
            Visit(innerExpression);
    }

    public override void VisitVariable(VariableExpression expression)
    {
        SetForeground(expression.TypeToken, KeywordBrush);

        SetForeground(expression.NameToken, GetTokenBrush(expression.NameToken));

        if (expression.AssignmentExpression is not null)
            Visit(expression.AssignmentExpression);
    }

    public override void VisitWhile(WhileExpression expression)
    {
        SetForeground(expression.WhileToken, KeywordBrush);

        Visit(expression.Condition);
        Visit(expression.Body);
    }

    private void WalkLeadingTrivia(Token token)
    {
        foreach (var trivia in token.LeadingTrivia)
            VisitTrivia(trivia);
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
        SetForeground(token.Range, brush);
    }

    private static IImmutableBrush GetTokenBrush(Token token)
    {
        if (token.IsNumber)
            return NumberBrush;

        if (token.IsKeyWord)
            return KeywordBrush;

        if (token.IsString)
            return StringBrush;

        return ForegroundBrush;
    }
}