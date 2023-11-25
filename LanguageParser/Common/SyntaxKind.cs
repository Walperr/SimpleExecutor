namespace LanguageParser.Common;

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
    /// {
    /// </summary>
    OpenBrace,
    /// <summary>
    /// }
    /// </summary>
    CloseBrace,
    /// <summary>
    /// ,
    /// </summary>
    Comma,
    /// <summary>
    /// =
    /// </summary>
    AssignmentOperator,
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
    /// %
    /// </summary>
    Percent,
    /// <summary>
    /// ++
    /// </summary>
    PlusPlus,
    /// <summary>
    /// --
    /// </summary>
    MinusMinus,
    /// <summary>
    /// =
    /// </summary>
    EqualsSign,
    /// <summary>
    /// ;
    /// </summary>
    Semicolon,
    
    /// <summary>
    /// if keyword
    /// </summary>
    If,
    /// <summary>
    /// else keyword
    /// </summary>
    Else,
    /// <summary>
    /// repeat keyword
    /// </summary>
    Repeat,
    /// <summary>
    /// times keyword
    /// </summary>
    Times,
    /// <summary>
    /// for keyword
    /// </summary>
    For,
    /// <summary>
    /// while keyword
    /// </summary>
    While,
    /// <summary>
    /// or keyword
    /// </summary>
    Or,
    /// <summary>
    /// and keyword
    /// </summary>
    And,
    /// <summary>
    /// number keyword (double type)
    /// </summary>
    Number,
    /// <summary>
    /// string keyword (string type)
    /// </summary>
    String,
    /// <summary>
    /// bool keyword (boolean type)
    /// </summary>
    Bool,
    /// <summary>
    /// true keyword
    /// </summary>
    True,
    /// <summary>
    /// false keyword
    /// </summary>
    False,
    /// <summary>
    /// Non predefined operator
    /// </summary>
    Operator,
    /// <summary>
    /// and number
    /// </summary>
    NumberLiteral,
    /// <summary>
    /// any non-empty text
    /// </summary>
    Word,
    /// <summary>
    /// any text in quotes
    /// </summary>
    StringLiteral,
    /// <summary>
    /// a * b
    /// </summary>
    MultiplyExpression,
    /// <summary>
    /// a / b
    /// </summary>
    DivideExpression,
    /// <summary>
    /// a % b
    /// </summary>
    RemainderExpression,
    /// <summary>
    /// a + b
    /// </summary>
    AddExpression,
    /// <summary>
    /// ++i
    /// </summary>
    PreIncrementExpression,
    /// <summary>
    /// i++
    /// </summary>
    PostIncrementExpression,
    /// <summary>
    /// +i
    /// </summary>
    UnaryPlusExpression,
    /// <summary>
    /// -i
    /// </summary>
    UnaryMinusExpression,
    /// <summary>
    /// --i
    /// </summary>
    PreDecrementExpression,
    /// <summary>
    /// i--
    /// </summary>
    PostDecrementExpression,
    /// <summary>
    /// a - b
    /// </summary>
    SubtractExpression,
    /// <summary>
    /// a == b
    /// </summary>
    EqualityExpression,
    /// <summary>
    /// a = b
    /// </summary>
    AssignmentExpression,
    /// <summary>
    /// string str;
    /// </summary>
    VariableExpression,
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
    /// <summary>
    /// if a then b else c
    /// </summary>
    IfExpression,
    /// <summary>
    /// while (true) expression
    /// </summary>
    WhileExpression,
    /// <summary>
    /// repeat expression until (b)
    /// </summary>
    RepeatExpression,
    /// <summary>
    /// for (a; b; c) expression
    /// </summary>
    ForExpression,
    /// <summary>
    /// { expression; expression; ... }
    /// </summary>
    ScopeExpression,
    
    Comment,
    
    NewLine,
    
    Error
}