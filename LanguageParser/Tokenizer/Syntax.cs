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
            SyntaxKind.OpenBrace => "{",
            SyntaxKind.CloseBrace => "}",
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
            SyntaxKind.If => "if",
            SyntaxKind.Else => "else",
            SyntaxKind.Repeat => "repeat",
            SyntaxKind.Until => "until",
            SyntaxKind.While => "while",
            SyntaxKind.For => "for",
            SyntaxKind.Number => "number",
            SyntaxKind.String => "string",
            SyntaxKind.Bool => "bool",
            SyntaxKind.True => "true",
            SyntaxKind.False => "false",
            SyntaxKind.Or => "or",
            SyntaxKind.And => "and",
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
            SyntaxKind.Slash => SyntaxKind.DivideExpression,
            SyntaxKind.EqualsSign => SyntaxKind.AssignmentOperator,
            SyntaxKind.EqualityOperator => SyntaxKind.EqualityExpression,
            SyntaxKind.ConditionalAndOperator or SyntaxKind.And => SyntaxKind.AndExpression,
            SyntaxKind.ConditionalOrOperator or SyntaxKind.Or => SyntaxKind.OrExpression,
            SyntaxKind.AssignmentOperator => SyntaxKind.AssignmentExpression,
            SyntaxKind.Operator => SyntaxKind.RelationalExpression,
            _ => SyntaxKind.None
        };
    }
}