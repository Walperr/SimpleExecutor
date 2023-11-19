using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Tokenizer;

namespace LanguageParser.Parser;

internal sealed class PreParser
{
    private readonly TokenStream _tokenStream;

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

        if (_tokenStream.Tokens[0].Kind is not SyntaxKind.OpenBrace)
            return new TokenStream(GetTokensWithBraces());

        return new ExpectedOtherTokenException(_tokenStream.EOF, SyntaxKind.CloseBrace);
    }

    private IEnumerable<Token> GetTokensWithBraces()
    {
        yield return new Token(SyntaxKind.OpenBrace, "", 0);
        foreach (var token in _tokenStream.Tokens)
            yield return token;
        yield return new Token(SyntaxKind.CloseBrace, "", _tokenStream.EOF.Range.Start);
    }
}