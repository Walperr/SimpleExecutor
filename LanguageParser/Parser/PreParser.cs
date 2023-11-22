using LanguageParser.Common;
using LanguageParser.Lexer;

namespace LanguageParser.Parser;

internal sealed class PreParser
{
    private readonly TokenStream _tokenStream;
    private SyntaxException? _error;
    private readonly Stack<int> _targetPlacement = new();

    public PreParser(TokenStream tokenStream)
    {
        _tokenStream = tokenStream;
    }

    internal static Result<SyntaxException, TokenStream> PreParse(TokenStream stream) => new PreParser(stream).PreParse();
    
    private Result<SyntaxException, TokenStream> PreParse()
    {
        if (_tokenStream.Tokens[0].Kind is SyntaxKind.OpenBrace &&
            _tokenStream.Tokens[^1].Kind is SyntaxKind.CloseBrace)
            return _tokenStream;

        if (_tokenStream.Tokens[0].Kind is SyntaxKind.OpenBrace)
            return new ExpectedOtherTokenException(_tokenStream.EOF, SyntaxKind.CloseBrace);
        
        var stream = new TokenStream(GetTokensWithBraces());

        if (_error is not null)
            return _error;
            
        return stream;
    }

    private IEnumerable<Token> GetTokensWithBraces()
    {
        var stream = new TokenStream(_tokenStream.Tokens);
        ExpressionsParser? parser = null;
        
        yield return new Token(SyntaxKind.OpenBrace, "", 0);
        while (stream.Index < stream.Length)
        {
            if (stream.Current.Kind is SyntaxKind.For)
            {
                parser ??= new ExpressionsParser(stream);
                
                yield return new Token(SyntaxKind.OpenBrace, "", stream.Current.Range.Start);

                var index = stream.Index;

                var target = parser.ParseExpression()?.Range.End;

                while (stream.Index > index) 
                    stream.Recede();

                if (target is null)
                    _error = parser.Error;
                else
                    _targetPlacement.Push(target.Value);
            }

            yield return stream.Current;

            if (_targetPlacement.TryPeek(out var currentTarget) && currentTarget == stream.Current.Range.End)
                yield return new Token(SyntaxKind.CloseBrace, "", _targetPlacement.Pop());

            stream.Advance();
        }
        
        yield return new Token(SyntaxKind.CloseBrace, "", _tokenStream.EOF.Range.Start);
    }
}