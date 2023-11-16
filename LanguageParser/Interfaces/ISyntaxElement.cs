using LanguageParser.Common;

namespace LanguageParser.Interfaces;

public interface ISyntaxElement
{
    StringRange Range { get; }
    
    SyntaxKind Kind { get; }

    public bool IsToken => false;

    public bool IsKeyWord => false;

    public bool IsExpression => false;

    public bool IsString => false;

    public bool IsNumber => false;
}