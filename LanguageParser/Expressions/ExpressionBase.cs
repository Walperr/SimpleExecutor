using System.Collections.Immutable;
using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public abstract class ExpressionBase : ISyntaxElement
{
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

    public StringRange Range => GetAllElements().Aggregate(r => r.Range, (acc, v) => acc.Union(v.Range));
    public SyntaxKind Kind { get; }
    public bool IsExpression => true;
    public abstract IEnumerable<ISyntaxElement> GetAllElements();
    public abstract void Visit(ExpressionVisitor visitor);
    public abstract T Visit<T>(ExpressionVisitor<T> visitor);
}