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
            _ => throw new InvalidEnumArgumentException()
        };
    }
}