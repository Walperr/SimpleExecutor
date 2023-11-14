namespace LanguageParser.Common;

// print("Hello, world!");
// print('Hello, world!');
public enum SyntaxKind
{
    /// <summary>
    /// None
    /// </summary>
    None,
    /// <summary>
    /// End of file
    /// </summary>
    EOF,
    /// <summary>
    /// Sequence of whitespace symbols (space, tab, etc.)
    /// </summary>
    WhiteSpace,
    /// <summary>
    /// '
    /// </summary>
    Quote,
    /// <summary>
    /// "
    /// </summary>
    DoubleQuote,
    /// <summary>
    /// (
    /// </summary>
    OpenParenthesis,
    /// <summary>
    /// )
    /// </summary>
    CloseParenthesis,
    /// <summary>
    /// ,
    /// </summary>
    Comma,
    /// <summary>
    /// ;
    /// </summary>
    Semicolon,
    /// <summary>
    /// and number
    /// </summary>
    Number,
    /// <summary>
    /// any non-empty text
    /// </summary>
    Word,
    /// <summary>
    /// any text in quotes
    /// </summary>
    String,
    /// <summary>
    /// f(x)
    /// </summary>
    InvocationExpression,
    
    NewLine,
    
    Error
}