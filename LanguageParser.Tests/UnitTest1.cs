using LanguageParser.Parser;
using Xunit.Abstractions;

namespace LanguageParser.Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CanTokenizeSimpleString()
    {
        var text = "print(\"Hello, world!\")";
        
        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("tokens:");

        var stream = Tokenizer.Tokenizer.Tokenize(text);

        while (stream.CanAdvance)
        {
            _testOutputHelper.WriteLine(stream.Current.ToString());
            stream.Advance();
        }
    }

    [Fact]
    public void CanTokenizeString()
    {
        var text =
            "print(\"Hello, world!\"); print(34.53); 5 + 3; a = 2 - 3.43; b * c == t; a / b; -34.43; -a; n || c' t && d";
        
        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("tokens:");

        var stream = Tokenizer.Tokenizer.Tokenize(text);

        while (stream.CanAdvance)
        {
            _testOutputHelper.WriteLine(stream.Current.ToString());
            stream.Advance();
        }
    }

    [Fact]
    public void CanParseSimpleString()
    {
        var text = "print(\"Hello, world!\");";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);
        
        if (expression.Error is not null)
            _testOutputHelper.WriteLine(expression.Error.Message);
        
        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);
    }

    [Fact]
    public void CanParseNestedExpressions()
    {
        var text = "5 + 4.565 == 45 != 0;";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);
        
        if (expression.Error is not null)
            _testOutputHelper.WriteLine(expression.Error.Message);
        
        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);
    }

    [Fact]
    public void CanParseSequenceOfExpressions()
    {
        var text = "Function(5 + 4.565 == 45 != 0); Func(34); 0-34; a == 3 * 2.522;";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);
        
        if (expression.Error is not null)
            _testOutputHelper.WriteLine(expression.Error.Message);
        
        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);
    }
}