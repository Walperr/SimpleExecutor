using System.ComponentModel;
using LanguageParser.Common;

namespace LanguageParser.Tokenizer;

internal static class Syntax
{
    public static string GetLexemeForToken(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.OpenParenthesis => "(",
            SyntaxKind.CloseParenthesis => ")",
            SyntaxKind.Comma => ",",
            SyntaxKind.Semicolon => ";",
            SyntaxKind.Word => "word",
            SyntaxKind.Quote => "'",
            SyntaxKind.DoubleQuote => "\"",
            SyntaxKind.Plus => "+",
            SyntaxKind.Minus => "-",
            SyntaxKind.Asterisk => "*",
            SyntaxKind.Slash => "/",
            SyntaxKind.EqualsSign => "=",
            _ => throw new InvalidEnumArgumentException()
        };
    }

    public static bool IsBinaryExpression(SyntaxKind tokenKind)
    {
        return ConvertToBinaryExpression(tokenKind) != SyntaxKind.None;
    }

    public static SyntaxKind ConvertToBinaryExpression(SyntaxKind tokenKind)
    {
        return tokenKind switch
        {
            SyntaxKind.Plus => SyntaxKind.AddExpression,
            SyntaxKind.Minus => SyntaxKind.SubtractExpression,
            SyntaxKind.Asterisk => SyntaxKind.MultiplyExpression,
            SyntaxKind.Slash => SyntaxKind.MultiplyExpression,
            SyntaxKind.EqualityOperator => SyntaxKind.EqualityExpression,
            SyntaxKind.ConditionalAndOperator => SyntaxKind.AndExpression,
            SyntaxKind.ConditionalOrOperator => SyntaxKind.OrExpression,
            SyntaxKind.Operator => SyntaxKind.RelationalExpression,
            _ => SyntaxKind.None
        };
    }
}