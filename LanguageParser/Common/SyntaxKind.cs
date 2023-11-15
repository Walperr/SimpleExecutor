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
    /// ==
    /// </summary>
    EqualityOperator,
    /// <summary>
    /// &&
    /// </summary>
    ConditionalAndOperator,
    /// <summary>
    /// ||
    /// </summary>
    ConditionalOrOperator,
    /// <summary>
    /// +
    /// </summary>
    Plus,
    /// <summary>
    /// -
    /// </summary>
    Minus,
    /// <summary>
    /// *
    /// </summary>
    Asterisk,
    /// <summary>
    /// /
    /// </summary>
    Slash,
    /// <summary>
    /// =
    /// </summary>
    EqualsSign,
    /// <summary>
    /// ;
    /// </summary>
    Semicolon,
    /// <summary>
    /// Non predefined operator
    /// </summary>
    Operator,
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
    /// a * b
    /// </summary>
    MultiplyExpression,
    /// <summary>
    /// a / b
    /// </summary>
    DivideExpression,
    /// <summary>
    /// a + b
    /// </summary>
    AddExpression,
    /// <summary>
    /// a - b
    /// </summary>
    SubtractExpression,
    /// <summary>
    /// a == b
    /// </summary>
    EqualityExpression,
    /// <summary>
    /// a && b
    /// </summary>
    AndExpression,
    /// <summary>
    ///  a || b
    /// </summary>
    OrExpression,
    /// <summary>
    ///  a < b a > b a <= b a >= b
    /// </summary>
    RelationalExpression,
    /// <summary>
    /// f(x)
    /// </summary>
    InvocationExpression,
    /// <summary>
    /// (expression)
    /// </summary>
    ParenthesizedExpression,
    
    NewLine,
    
    Error
}