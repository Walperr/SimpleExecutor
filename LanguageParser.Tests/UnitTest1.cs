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
    public void CanTokenizeStringWithKeyWords()
    {
        var text =
            "print(\"Hello, world!\"); if true then false else 3453; repeat i++ until(true); for (342;i=0;expression); number; string; bool; print(34.53); 5 + 3; a = 2 - 3.43; b * c == t; a / b; -34.43; -a; n || c' t && d";
        
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
    public void CanParseVariableDeclaration()
    {
        var text = "number a = func(34)";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);
        
        if (expression.Error is not null)
            _testOutputHelper.WriteLine(expression.Error.Message);
        
        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);
    }

    [Fact]
    public void CanParseEqualsOperatorExpression()
    {
        var text = "a == b";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);
        
        if (expression.Error is not null)
            _testOutputHelper.WriteLine(expression.Error.Message);
        
        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);
    }
    
    [Fact]
    public void CanParseAssignmentOperatorExpression()
    {
        var text = "a = b";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);
        
        if (expression.Error is not null)
            _testOutputHelper.WriteLine(expression.Error.Message);
        
        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);
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
        var text = "{Function(5 + 4.565 == 45 != 0); Func(34); 0-34; a == 3 * 2.522;}";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);
        
        if (expression.Error is not null)
            _testOutputHelper.WriteLine(expression.Error.Message);
        
        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);
    }

    [Fact]
    public void CanParseComplexString()
    {
        var text =
            "    someFunction(arg1, arg2);\n\n    if (true)\n        someFunction2(arg);\n\n    if (false)\n        someFunction3(arg);\n    else\n        someFunction4(arg);\n\n    if (predicat())\n        print(\"Hello, world\");\n\n    while(true)\n    {\n        Function1();\n        Function2();\n        Function3();\n    }\n\n    for (;;)\n    {\n        ForBody();\n    }\n\n    repeat Expression() until (true)\n\n    for (init(); i <= 23; i + 1)\n        ForBody();\n\n    for (;i > 0;)\n    {}\n\n    {\n        Expression1();\n        Expression2();\n\n        a + b + 4;\n    }\n\n    if (1)\n        if (2)\n            DoSomething();\n        else\n            if (3)\n                doOther();\n            else\n                print(\"String\");\n\n\n    if (1)\n    {\n        if (2)\n            DoSomething();\n        else\n            if (3)\n                doOther();\n    }\n    else\n        print(\"String\");";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);
        
        if (expression.Error is not null)
        {
            _testOutputHelper.WriteLine(expression.Error.Message);
            _testOutputHelper.WriteLine(expression.Error.Range.ToString());
        }
        
        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);
    }
    
    [Fact]
    public void CanParseComplexStringWithSemicolons()
    {
        var text =
            "number a;\nnumber b;\n\nstring arg1;\n\nstring arg2 = \"some string\";\n\nbool boolValue = false; \n    \nsomeFunction(arg1, arg2);\n\nif (boolValue)\n    arg1 = someFunction2(arg);\n\nif (false)\n    a = someFunction3(arg);\nelse\n    b = someFunction4(arg);\n\nif (predicat())\n    print(\"Hello, world\");\n\nwhile(true)\n{\n    Function1();\n    Function2();\n    Function3();\n    number c = (4 + 3) * 2.5 - 38 / 2;\n}\n\nfor (;;)\n{\n    ForBody();\n}\n\nrepeat Expression() until (true)\n\nfor (i = 0; i <= 23; i = i + 1)\n    ForBody();\n\nfor (;i > 0;)\n{}\n\n{\n    Expression1();\n    Expression2();\n\n    a + b + 4;\n}\n\nif (1)\n    if (2)\n        DoSomething();\n    else\n        if (3)\n            doOther();\n        else\n            print(\"String\");\n\n\nif (1)\n{\n    if (2)\n        DoSomething();\n    else\n        if (3)\n            doOther();\n}\nelse\n    print(\"String\");";
        
        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);
        
        if (expression.Error is not null)
        {
            _testOutputHelper.WriteLine(expression.Error.Message);
            _testOutputHelper.WriteLine(expression.Error.Range.ToString());
        }
        
        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);
    }
}