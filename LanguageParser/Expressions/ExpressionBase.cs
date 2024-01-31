using System.Collections.Immutable;
using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public abstract class ExpressionBase : ISyntaxElement
{
    private object? _type;

    private protected ExpressionBase(SyntaxKind kind)
    {
        Kind = kind;
    }

    public ImmutableArray<Token> LeadingTrivia
    {
        get
        {
            var element = GetAllElements().FirstOrDefault();

            return element switch
            {
                ExpressionBase expression => expression.LeadingTrivia,
                null => ImmutableArray<Token>.Empty,
                _ => ((Token)element).LeadingTrivia
            };
        }
    }

    public ImmutableArray<Token> TrailingTrivia
    {
        get
        {
            var element = GetAllElements().LastOrDefault();

            return element switch
            {
                ExpressionBase expression => expression.TrailingTrivia,
                null => ImmutableArray<Token>.Empty,
                _ => ((Token)element).TrailingTrivia
            };
        }
    }

    //todo: should be Type
    public object? Type
    {
        get => _type;
        set
        {
            if (_type is not null && _type != value)
                throw new InvalidOperationException("Cannot change type of expression");
            
            _type = value;
        }
    }

    public StringRange Range => GetAllElements().Aggregate(r => r.Range, (acc, v) => acc.Union(v.Range));
    public SyntaxKind Kind { get; }
    public bool IsExpression => true;
    public abstract IEnumerable<ISyntaxElement> GetAllElements();
    public abstract void Visit(ExpressionVisitor visitor);
    public abstract T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state);
}