using LanguageInterpreter;
using LanguageParser.Expressions;
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
    public void CanParseSimpleString()
    {
        var text = "print(\"Hello, world!\")";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text).Value;

        _testOutputHelper.WriteLine(expression?.ToString());
    }

    [Fact]
    public void CanExecuteHelloWorld()
    {
        var text = "print(\"Hello, World!\", 'This is my new language', 34, 436.65,0,43)";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text).Value;
        
        Evaluator.InvokeExpression((InvocationExpression)expression!.Expression, _testOutputHelper);
    }
}