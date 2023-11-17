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
    public void Test()
    {
        Assert.True(false);
    }

    [Fact]
    public void CanTokenizeSimpleString()
    {
        const string text = "print(\"Hello, world!\")";
        
        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("tokens:");

        var stream = Tokenizer.Tokenizer.Tokenize(text);
        
        Assert.NotNull(stream);
        Assert.True(stream.CanAdvance);
        
        while (stream.CanAdvance)
        {
            _testOutputHelper.WriteLine(stream.Current.ToString());
            stream.Advance();
        }
    }

    [Fact]
    public void CanTokenizeString()
    {
        const string text = "print(\"Hello, world!\"); print(34.53); 5 + 3; a = 2 - 3.43; b * c == t; a / b; -34.43; -a; n || c' t && d";
        
        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("tokens:");

        var stream = Tokenizer.Tokenizer.Tokenize(text);
        
        Assert.NotNull(stream);
        Assert.True(stream.CanAdvance);

        while (stream.CanAdvance)
        {
            _testOutputHelper.WriteLine(stream.Current.ToString());
            stream.Advance();
        }
    }

    [Fact]
    public void CanTokenizeComplexString()
    {
        const string text = "number a;\nnumber b;\n\nstring arg1;\n\nstring arg2 = \"some string\";\n\nbool boolValue = false; \n    \nsomeFunction(arg1, arg2);\n\nif (boolValue)\n    arg1 = someFunction2(arg);\n\nif (false)\n    a = someFunction3(arg);\nelse\n    b = someFunction4(arg);\n\nif (predicat())\n    print(\"Hello, world\");\n\nwhile(true)\n{\n    Function1();\n    Function2();\n    Function3();\n    number c = (4 + 3) * 2.5 - 38 / 2;\n}\n\nfor (;;)\n{\n    ForBody();\n}\n\nrepeat Expression() until (true)\n\nfor (i = 0; i <= 23; i = i + 1)\n    ForBody();\n\nfor (;i > 0;)\n{}\n\n{\n    Expression1();\n    Expression2();\n\n    a + b + 4;\n}\n\nif (1)\n    if (2)\n        DoSomething();\n    else\n        if (3)\n            doOther();\n        else\n            print(\"String\");\n\n\nif (1)\n{\n    if (2)\n        DoSomething();\n    else\n        if (3)\n            doOther();\n}\nelse\n    print(\"String\");";
        
        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("tokens:");

        var stream = Tokenizer.Tokenizer.Tokenize(text);
        
        Assert.NotNull(stream);
        Assert.True(stream.CanAdvance);

        while (stream.CanAdvance)
        {
            _testOutputHelper.WriteLine(stream.Current.ToString());
            stream.Advance();
        }
    }

    [Fact]
    public void CanParseConstantExpressions()
    {
        var texts = new[]
        {
            "word", "'number'", "\"string\"", "4.54", "8", "true", "false", "word;", "'number';", "\"string\";",
            "4.54;", "8;", "true;", "false;"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
    }

    [Fact]
    public void CanParseBinaryExpressions()
    {
        var texts = new[]
        {
            "1 + 1", "a + b", "4 - 2.4", "2 * b", "2 == b", "a = b", "c == d", "1 == 2", "45 / 0.5", "true or false", "a and b", "1 + 1;", "a + b;",
            "4 - 2.4;", "2 * b;", "2 == b;", "a = b;", "c == d;", "1 == 2;", "45 / 0.5;", "true or false;", "a and b;"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
    }

    [Fact]
    public void CanParseScopeExpressions()
    {
        var texts = new[]
        {
            "{}", "{ 1 + 2 expr1()}", "{{} {}}", "{{} 1 * 2 string B;}", "{}", "{ 1 + 2; expr1()}", "{{}; {}}",
            "{{}; 1 * 2; string B;}"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
    }
    
    [Fact]
    public void CanParseForExpressions()
    {
        var texts = new[]
        {
            "for (;;) {}", "for (number i = 0; i <= 20; i = i + 1) callFunc()", "for (; true; step()) {}",
            "for (;;) {};", "for (number i = 0; i <= 20; i = i + 1) callFunc();", "for (; true; step()) {};"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
    }

    [Fact]
    public void CanParseIfExpressions()
    {
        var texts = new[]
        {
            "if (true) expr()", "if (check()) expr() else expr2()", "if (true) {}", "if (false) {} else {}",
            "if (true) expr();", "if (check()) expr(); else expr2();", "if (true) {};", "if (false) {}; else {};",
            "{if (a) b(); else c();}"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
    }

    [Fact]
    public void CanParseInvocationExpressions()
    {
        var texts = new[]
        {
            "invoke()", "call(1, 2, 'string')", "foo(arg1, 5.4, \"string\")", "invoke();", "call(1, 2, 'string');",
            "foo(arg1, 5.4, \"string\");"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
    }

    [Fact]
    public void CanParseParenthesizedExpressions()
    {
        var texts = new[]
        {
            "(invoke())", "(call(1, 2, 'string'))", "(10 - 2)", "(2*4)", "({})", "('string')", "(invoke());",
            "(call(1, 2, 'string'));", "(10 - 2);", "(2*4);", "({});", "('string');"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
    }

    [Fact]
    public void CanParseRepeatExpression()
    {
        var texts = new[]
        {
            "repeat expression() until (true)", "repeat { expression() foo(e, a) 5 - 8 } until (true)",
            "repeat expression(); until (true);", "repeat { expression(); foo(e, a); 5 - 8; } until (true);"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
    }
    
    [Fact]
    public void CanParseVariableDeclarationExpressions()
    {
        var texts = new[]
        {
            "number c = 0", "number a;", "string s = 'asfafw'", "string s = \"fewfwef\"", "string s;", "bool b = false",
            "bool b =  true", "bool b;",
            "number c = 0;", "string s = 'asfafw';", "string s = \"fewfwef\";", "bool b = false;", "bool b =  true;"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
    }

    [Fact]
    public void CanParseWhileExpression()
    {
        var texts = new[]
        {
            "while(true) {}", "while (a < b) do()", "while (check()) {a() i = i + 1 invoke()}",
            "while(true) {};", "while (a < b) do();", "while (check()) {a(); i = i + 1; invoke();};"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
    }
    
    [Fact]
    public void CanParseExpressionChain()
    {
        var texts = new[]
        {
            "1 + 2 + 3 + 4", "1 + 2 * 3 / 4", "(1 + 2) - 3 * 4", "(((12/6)-2)+5) < 0 || b() and check() == true",
            "1 + 2 + 3 + 4;", "1 + 2 * 3 / 4;", "(1 + 2) - 3 * 4;", "(((12/6)-2)+5) < 0 || b() and check() == true;"
        };

        foreach (var text in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);
            
            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);
        }
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
            "number a;\nnumber b;\n\nstring arg1;\n\nstring arg2 = \"some string;\"\n\nbool boolValue = false \n    \nsomeFunction(arg1, arg2)\n\nif (boolValue)\n    arg1 = someFunction2(arg)\n\nif (false)\n    a = someFunction3(arg)\nelse\n    b = someFunction4(arg)\n\nif (predicat())\n    print(\"Hello, world\")\n\nwhile(true)\n{\n    Function1()\n    Function2()\n    Function3()\n    number c = (4 + 3) * 2.5 - 38 / 2\n}\n\nfor (;;)\n{\n    ForBody()\n}\n\nrepeat Expression() until (true)\n\nfor (i = 0; i <= 23; i = i + 1)\n    ForBody()\n\nfor (;i > 0;)\n{}\n\n{\n    Expression1()\n    Expression2()\n\n    a + b + 4\n}\n\nif (1)\n    if (2)\n        DoSomething()\n    else\n        if (3)\n            doOther()\n        else\n            print(\"String\")\n\n\nif (1)\n{\n    if (2)\n        DoSomething()\n    else\n        if (3)\n            doOther()\n}\nelse\n    print(\"String\")";
        
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