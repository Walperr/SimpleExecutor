using LanguageInterpreter;
using LanguageInterpreter.Execution;
using LanguageParser.Common;
using LanguageParser.Lexer;
using LanguageParser.Parser;
using LanguageParser.Visitors;
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
        const string text = "print(\"Hello, world!\")";

        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("tokens:");

        var stream = Tokenizer.Tokenize(text);

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
        const string text =
            "print(\"Hello, world!\"); print(34.53); 5 + 3; a = 2 - 3.43; b * c == t; a / b; -34.43; -a; n || c' t && d";

        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("tokens:");

        var stream = Tokenizer.Tokenize(text);

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
        const string text =
            "number a;\nnumber b;\n\nstring arg1;\n\nstring arg2 = \"some string\";\n\nbool boolValue = false; \n    \nsomeFunction(arg1, arg2);\n\nif (boolValue)\n    arg1 = someFunction2(arg);\n\nif (false)\n    a = someFunction3(arg);\nelse\n    b = someFunction4(arg);\n\nif (predicat())\n    print(\"Hello, world\");\n\nwhile(true)\n{\n    Function1();\n    Function2();\n    Function3();\n    number c = (4 + 3) * 2.5 - 38 / 2;\n}\n\nfor (;;)\n{\n    ForBody();\n}\n\nrepeat Expression() until (true)\n\nfor (i = 0; i <= 23; i = i + 1)\n    ForBody();\n\nfor (;i > 0;)\n{}\n\n{\n    Expression1();\n    Expression2();\n\n    a + b + 4;\n}\n\nif (1)\n    if (2)\n        DoSomething();\n    else\n        if (3)\n            doOther();\n        else\n            print(\"String\");\n\n\nif (1)\n{\n    if (2)\n        DoSomething();\n    else\n        if (3)\n            doOther();\n}\nelse\n    print(\"String\");";

        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("tokens:");

        var stream = Tokenizer.Tokenize(text);

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
            "1 + 1", "a + b", "4 - 2.4", "2 * b", "2 == b", "a = b", "c == d", "1 == 2", "45 / 0.5", "true or false",
            "a and b", "1 + 1;", "a + b;",
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
    public void CanParseUnaryExpressions()
    {
        var texts = new[] { "i++", "i--", "++i", "--i", "+a", "-a", "-0", "+0" };

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
            "for string i in [1,2,3] {}", "for number i = 0 to 100 {}", "for number i = 100 down to 10 {}",
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
            "repeat count() times expression()", "repeat  50 times { expression() foo(e, a) 5 - 8 }",
            "repeat count(); times expression();", "repeat 50; times { expression(); foo(e, a); 5 - 8; };"
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
            "number c = 0", "number a", "string s = 'asfafw'", "string s = \"fewfwef\"", "string s;", "bool b = false",
            "bool b =  true", "bool b;",
            "number[] f", "string[] arr", "bool[]", "number[] numbers = [1,2,3,4,5]", "string[] ss = ['s','ss','sss']",
            "number[] ns = number[10]",
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
            "number a;\nnumber b;\n\nstring arg1;\n\nstring arg2 = \"some string;\"\n\nbool boolValue = false \n    \nsomeFunction(arg1, arg2)\n\nif (boolValue)\n    arg1 = someFunction2(arg)\n\nif (false)\n    a = someFunction3(arg)\nelse\n    b = someFunction4(arg)\n\nif (predicat())\n    print(\"Hello, world\")\n\nwhile(true)\n{\n    Function1()\n    Function2()\n    Function3()\n    number c = (4 + 3) * 2.5 - 38 / 2\n}\n\nfor (;;)\n{\n    ForBody()\n}\n\nrepeat 100 times Expression()\n\nfor (i = 0; i <= 23; i = i + 1)\n    ForBody()\n\nfor (;i > 0;)\n{}\n\n{\n    Expression1()\n    Expression2()\n\n    a + b + 4\n}\n\nif (1)\n    if (2)\n        DoSomething()\n    else\n        if (3)\n            doOther()\n        else\n            print(\"String\")\n\n\nif (1)\n{\n    if (2)\n        DoSomething()\n    else\n        if (3)\n            doOther()\n}\nelse\n    print(\"String\")";

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
            "number a;\nnumber b;\n\nstring arg1;\n\nstring arg2 = \"some string\";\n\nbool boolValue = false; \n    \nsomeFunction(arg1, arg2);\n\nif (boolValue)\n    arg1 = someFunction2(arg);\n\nif (false)\n    a = someFunction3(arg);\nelse\n    b = someFunction4(arg);\n\nif (predicat())\n    print(\"Hello, world\");\n\nwhile(true)\n{\n    Function1();\n    Function2();\n    Function3();\n    number c = (4 + 3) * 2.5 - 38 / 2;\n}\n\nfor (;;)\n{\n    ForBody();\n}\n\nrepeat 100 times Expression()\n\nfor (i = 0; i <= 23; i = i + 1)\n    ForBody();\n\nfor (;i > 0;)\n{}\n\n{\n    Expression1();\n    Expression2();\n\n    a + b + 4;\n}\n\nif (1)\n    if (2)\n        DoSomething();\n    else\n        if (3)\n            doOther();\n        else\n            print(\"String\");\n\n\nif (1)\n{\n    if (2)\n        DoSomething();\n    else\n        if (3)\n            doOther();\n}\nelse\n    print(\"String\");";

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
    public void CanCollectVariables()
    {
        var text =
            "number a;\nnumber b = 0;\nnumber c = b;\n\nstring s = 'spgjwgjwe';\n\nbool isSomething;\n\nbool isSomethingElse = true;\n\n{\n    number f = 0.023;\n\n    string s1 = \"jwgeogjw\";\n\n    string s2;\n\n    if (false)\n        b = 4;\n    else\n        a = 34.3453 + b;\n\n    if (true)\n        b = 4;\n    else if (5 < 54)\n        a = 34.3453 + b;\n\n    for (number i = 0; i <= 23; i = i + 1)\n    {\n        number j = 0;\n        s2 = \"2ada\";\n        j = j + 1;\n    }\n\n    for (i = 0; false; i = i + 1)\n    {\n        number j = 0;\n        s2 = \"2ada\" + s2;\n        j = j + 1;\n    }\n\n    number j = 0;\n\n    while (isSomethingElse)\n    {\n        s = \"2ada\" + s2;\n        j = j + 1;\n        bool t = false;\n\n        if (t == false)\n            {\n                t = true;\n            }\n\n        if (j == 10)\n            isSomethingElse = false;\n    }\n}\n\n{\n    number f = 3123.0232;\n    number g;\n    string str = \"rref\";\n    bool bebra = false;\n}";

        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);

        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);

        var scopeNode = DeclarationsCollector.Collect(expression.Value);

        Assert.NotNull(scopeNode);
    }

    [Fact]
    public void CanResolveTypes()
    {
        var text =
            "number a;\nnumber b = 0;\nnumber c = b;\n\nstring s = 'spgjwgjwe';\n\nbool isSomething;\n\nbool isSomethingElse = true;\n\n{\n    number f = 0.023;\n\n    string s1 = \"jwgeogjw\";\n\n    string s2;\n\n    if (false)\n        b = 4;\n    else\n        a = 34.3453 + b;\n\n    if (true)\n        b = 4;\n    else if (5 < 54)\n        a = 34.3453 + b;\n\n    for (number i = 0; i <= 23; i = i + 1)\n    {\n        number j = 0;\n        s2 = \"2ada\";\n        j = j + 1;\n    }\n\n    for (i = 0; false; i = i + 1)\n    {\n        number j = 0;\n        s2 = \"2ada\" + s2;\n        j = j + 1;\n    }\n\n    number j = 0;\n\n    while (isSomethingElse)\n    {\n        s = \"2ada\" + s2;\n        j = j + 1;\n        bool t = false;\n\n        if (t == false)\n            {\n                t = true;\n            }\n\n        if (j == 10)\n            isSomethingElse = false;\n    }\n}\n\n{\n    number f = 3123.0232;\n    number g;\n    string str = \"rref\";\n    bool bebra = false;\n}";

        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);

        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);

        var scopeNode = DeclarationsCollector.Collect(expression.Value);

        Assert.NotNull(scopeNode);

        var type = TypeResolver.Resolve(scopeNode);

        if (type.IsError)
        {
            _testOutputHelper.WriteLine(type.Error.Message);
            _testOutputHelper.WriteLine(type.Error.Range.ToString());
        }

        Assert.NotNull(type.Value);
        Assert.Null(type.Error);

        _testOutputHelper.WriteLine(type.ToString());
    }

    [Fact]
    public void CanEvaluateExpression()
    {
        var text =
            "number a;\nnumber b = 0;\nnumber c = b;\n\nstring s = 'spgjwgjwe';\n\nbool isSomething;\n\nbool isSomethingElse = true;\n\n{\n    number f = 0.023;\n\n    string s1 = \"jwgeogjw\";\n\n    string s2;\n\n    if (false)\n        b = 4;\n    else\n        a = 34.3453 + b;\n\n    if (true)\n        b = 4;\n    else if (5 < 54)\n        a = 34.3453 + b;\n\n    for (number i = 0; i <= 23; i = i + 1)\n    {\n        number j = 0;\n        s2 = \"2ada\";\n        j = j + 1;\n    }\n\n    for (number i = 0; false; i = i + 1)\n    {\n        number j = 0;\n        s2 = \"2ada\" + s2;\n        j = j + 1;\n    }\n\n    number j = 0;\n\n    while (isSomethingElse)\n    {\n        s = \"2ada\" + s2;\n        j = j + 1;\n        bool t = false;\n\n        if (t == false)\n            {\n                t = true;\n            }\n\n        if (j == 10)\n            isSomethingElse = false;\n    }\n}\n\n{\n    number f = 3123.0232;\n    number g;\n    string str = \"rref\";\n    bool bebra = false;\n}";

        _testOutputHelper.WriteLine(text);

        var expression = ExpressionsParser.Parse(text);

        Assert.NotNull(expression.Value);
        Assert.Null(expression.Error);

        var scopeNode = DeclarationsCollector.Collect(expression.Value);

        Assert.NotNull(scopeNode);

        var type = TypeResolver.Resolve(scopeNode);

        if (type.IsError)
        {
            _testOutputHelper.WriteLine(type.Error.Message);
            _testOutputHelper.WriteLine(type.Error.Range.ToString());
        }

        Assert.NotNull(type.Value);
        Assert.Null(type.Error);

        _testOutputHelper.WriteLine(type.ToString());

        var value = ExpressionEvaluator.Evaluate(scopeNode);

        Assert.NotNull(value.Value);
        Assert.Null(value.Error);

        _testOutputHelper.WriteLine(value.Value.ToString());
    }

    [Fact]
    public void CanEvaluateBinaryExpressions()
    {
        (string text, object result)[] texts =
        {
            (text: "1 + 1", result: 1.0 + 1.0),
            (text: "number a = 142 number b = 23.2523 a + b", result: 142 + 23.2523),
            (text: "4 - 2.4", result: 4 - 2.4),
            (text: "number b = 23.2523 2 * b", result: 2 * 23.2523),
            (text: "number b = 2 2 == b", result: true),
            (text: "number a = 1.0002 number b = 1.002 a = b", result: 1.002),
            (text: "1 == 2", result: false),
            (text: "45 / 0.5", result: 45 / 0.5),
            (text: "true or false", true || false),
            (text: "true and false", result: true && false)
        };

        foreach (var (text, result) in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);

            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);

            var scopeNode = DeclarationsCollector.Collect(expression.Value);

            Assert.NotNull(scopeNode);

            var type = TypeResolver.Resolve(scopeNode);

            if (type.IsError)
            {
                _testOutputHelper.WriteLine(type.Error.Message);
                _testOutputHelper.WriteLine(type.Error.Range.ToString());
            }

            Assert.NotNull(type.Value);
            Assert.Null(type.Error);

            _testOutputHelper.WriteLine(type.ToString());

            var value = ExpressionEvaluator.Evaluate(scopeNode);

            Assert.NotNull(value.Value);
            Assert.Null(value.Error);

            Assert.Equivalent(result, value.Value);
            _testOutputHelper.WriteLine(value.ToString());
        }
    }

    [Fact]
    public void CanEvaluateUnaryExpressions()
    {
        (string text, object result)[] texts =
        {
            (text: "number i = 0; i++", result: 0.0),
            (text: "number i = 0; ++i", result: 1.0),
            (text: "number i = 0; i--", result: 0.0),
            (text: "number i = 0; --i", result: -1.0),
            (text: "number i = 1; -i", result: -1.0),
            (text: "number i = 1; +i", result: 1.0),
            (text: "number i = -(5 - 6); -i", result: - -(5.0 - 6.0)),
            (text: "number i = -(5 - 6); i", result: -(5.0 - 6.0)),
            (text: "-5", result: -5.0),
            (text: "5", result: 5.0)
        };

        foreach (var (text, result) in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);

            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);

            var scopeNode = DeclarationsCollector.Collect(expression.Value);

            Assert.NotNull(scopeNode);

            var type = TypeResolver.Resolve(scopeNode);

            if (type.IsError)
            {
                _testOutputHelper.WriteLine(type.Error.Message);
                _testOutputHelper.WriteLine(type.Error.Range.ToString());
            }

            Assert.NotNull(type.Value);
            Assert.Null(type.Error);

            _testOutputHelper.WriteLine(type.ToString());

            var value = ExpressionEvaluator.Evaluate(scopeNode);

            Assert.NotNull(value.Value);
            Assert.Null(value.Error);

            Assert.Equivalent(result, value.Value);
            _testOutputHelper.WriteLine(value.ToString());
        }
    }

    [Fact]
    public void CanEvaluateExpressionChains()
    {
        (string text, object result)[] texts =
        {
            (text: "(1 + 1) * 5.65 - 23.5 / 5", result: (1 + 1) * 5.65 - 23.5 / 5),
            (text: "number a = 142\nnumber b = 23.2523\na + b * 53 - 6.23", result: 142 + 23.2523 * 53 - 6.23),
            (text: "4 - 2.4 / 2 * 6 - 48.023 * (234 + ((52 + 56) - 28))",
                result: 4 - 2.4 / 2 * 6 - 48.023 * (234 + (52 + 56 - 28))),
            (text: "4 * 28 - 45 * (2 - 36) < 528 * 14.65 && 32 >= 16 || 10 == 5 * (1 / 0.5)",
                (4 * 28 - 45 * (2 - 36) < 528 * 14.65 && 32 >= 16) || 10 == 5 * (1 / 0.5)),
            (text: "4 * 28 - 45 > 0 || ((2 - 36) < 528 * 14.65) && (32 >= 16 || 10 == 5 * (1 / 0.5))",
                result: 4 * 28 - 45 > 0 || (2 - 36 < 528 * 14.65 && (32 >= 16 || 10 == 5 * (1 / 0.5))))
        };

        foreach (var (text, result) in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);

            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);

            var scopeNode = DeclarationsCollector.Collect(expression.Value);

            Assert.NotNull(scopeNode);

            var type = TypeResolver.Resolve(scopeNode);

            if (type.IsError)
            {
                _testOutputHelper.WriteLine(type.Error.Message);
                _testOutputHelper.WriteLine(type.Error.Range.ToString());
            }

            Assert.NotNull(type.Value);
            Assert.Null(type.Error);

            _testOutputHelper.WriteLine(type.ToString());

            var value = ExpressionEvaluator.Evaluate(scopeNode);

            Assert.NotNull(value.Value);
            Assert.Null(value.Error);

            Assert.Equivalent(result, value.Value);
            _testOutputHelper.WriteLine(value.ToString());
        }
    }

    [Fact]
    public void CanBuildInterpreter()
    {
        var printFunction = new Function("print",
            new[] { new Variable("text", typeof(object)) },
            args => _testOutputHelper.WriteLine(args.First().ToString()));
        var printLineFunction = new Function("printLine",
            new[] { new Variable("text", typeof(object)) },
            args => _testOutputHelper.WriteLine(args.First().ToString()));

        var piVariable = new Variable("PI", typeof(double), Math.PI);
        var eVariable = new Variable("E", typeof(double), Math.E);

        var interpreter = InterpreterBuilder.CreateBuilder()
            .WithPredefinedFunction(printFunction)
            .WithPredefinedFunction(printLineFunction)
            .WithPredefinedVariable(piVariable)
            .WithPredefinedVariable(eVariable)
            .Build();

        Assert.NotNull(interpreter);
    }

    [Fact]
    public void CanUsePredefinedFunctionsAndVariables()
    {
        var printFunction = new Function("print",
            new[] { new Variable("text", typeof(object)) },
            args => _testOutputHelper.WriteLine(args.First().ToString()));
        var printLineFunction = new Function("printLine",
            new[] { new Variable("text", typeof(object)) },
            args => _testOutputHelper.WriteLine(args.First().ToString()));

        var piVariable = new Variable("PI", typeof(double), Math.PI);
        var eVariable = new Variable("E", typeof(double), Math.E);

        var interpreter = InterpreterBuilder.CreateBuilder()
            .WithPredefinedFunction(printFunction)
            .WithPredefinedFunction(printLineFunction)
            .WithPredefinedVariable(piVariable)
            .WithPredefinedVariable(eVariable)
            .Build();

        Assert.NotNull(interpreter);

        const string text =
            "print('Hello, world')\nprint(PI)\nprintLine('print line')\nprintLine('E = ' + E)\nif (PI > E)\n\tprint(PI)\nelse\n\tprint(E)";

        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("\nresult:\n");

        interpreter.Initialize(text);

        if (interpreter.HasErrors)
            _testOutputHelper.WriteLine(interpreter.Error.Message);

        Assert.False(interpreter.HasErrors);

        var result = interpreter.Interpret(CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void CanUsePredefinedFunctionsAndVariablesInLoops()
    {
        var printFunction = new Function("print",
            new[] { new Variable("text", typeof(object)) },
            args => _testOutputHelper.WriteLine(args.First().ToString()));

        var piVariable = new Variable("PI", typeof(double), Math.PI);
        var eVariable = new Variable("E", typeof(double), Math.E);

        var interpreter = InterpreterBuilder.CreateBuilder()
            .WithPredefinedFunction(printFunction)
            .WithPredefinedVariable(piVariable)
            .WithPredefinedVariable(eVariable)
            .Build();

        Assert.NotNull(interpreter);

        const string text =
            "print(\"start\")\n\nnumber someNumber = 6\n\nfor (number i = 0; i < someNumber; i = i + 1)\n    print(\"current i = \" + i)\n\nnumber i = 0\n\nwhile (i > someNumber / 2)\n{\n    print(i + \": PI still equals \" + PI)\n\n    i = i - 1\n}\n\nnumber j = 0\n\nrepeat 5 times\n{\n    print(\"Repeat print E\")\n    print(E)\n}\nprint(\"done\")";

        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("\nresult:\n");

        interpreter.Initialize(text);

        if (interpreter.HasErrors)
            _testOutputHelper.WriteLine(interpreter.Error.Message);

        Assert.False(interpreter.HasErrors);

        var result = interpreter.Interpret(CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
    }
    
    [Fact]
    public void CanEvaluateForLoops()
    {
        var printFunction = new Function("print",
            new[] { new Variable("text", typeof(object)) },
            args => _testOutputHelper.WriteLine(args.First().ToString()));

        var interpreter = InterpreterBuilder.CreateBuilder()
            .WithPredefinedFunction(printFunction)
            .Build();

        Assert.NotNull(interpreter);

        const string text =
            "for number i = 0 to 10 print(i) for number i = 10 down to 1 print(i) for string s in ['hello', 'hi', 'world', 'some string'] print(s) for number i in [2, 3, 6, 7] print(i) for bool b in [true, false, true, true, false, false] print(b)";

        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("\nresult:\n");

        interpreter.Initialize(text);

        if (interpreter.HasErrors)
            _testOutputHelper.WriteLine(interpreter.Error.Message);

        Assert.False(interpreter.HasErrors);

        var result = interpreter.Interpret(CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void CanCalculateDivision()
    {
        const string text = "1 / 2 / 3 / 4";

        const double expected = 1.0 / 2.0 / 3.0 / 4.0;

        var interpreter = InterpreterBuilder.CreateBuilder()
            .Build();

        Assert.NotNull(interpreter);

        interpreter.Initialize(text);

        Assert.False(interpreter.HasErrors);

        var result = interpreter.Interpret(CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.Null(result.Error);

        Assert.Equivalent(expected, result.Value);

        _testOutputHelper.WriteLine(result.Value.ToString());
    }

    [Fact]
    public void CanCalculateSubtraction()
    {
        const string text = "1 - 2 - 3 - 4";

        const double expected = 1.0 - 2.0 - 3.0 - 4.0;

        var interpreter = InterpreterBuilder.CreateBuilder()
            .Build();

        Assert.NotNull(interpreter);

        interpreter.Initialize(text);

        Assert.False(interpreter.HasErrors);

        var result = interpreter.Interpret(CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.Null(result.Error);

        Assert.Equivalent(expected, result.Value);

        _testOutputHelper.WriteLine(result.Value.ToString());
    }

    [Fact]
    public void CanCalculatePI()
    {
        const string text =
            "number r = 0\nnumber c = 16\nnumber n = 104\n\nfor (number i = 0; i <= n; i++)\n{\n\tc = c / 16\n\tr = r + c * (4 / (8 * i + 1) - 2 / (8 * i + 4) - 1 / (8 * i + 5) - 1 / (8 * i + 6))\n}";

        var interpreter = InterpreterBuilder.CreateBuilder().Build();

        Assert.NotNull(interpreter);

        interpreter.Initialize(text);

        Assert.False(interpreter.HasErrors);

        var result = interpreter.Interpret(CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.Null(result.Error);

        Assert.Equivalent(Math.PI, result.Value);

        _testOutputHelper.WriteLine(result.Value.ToString());
    }

    [Fact]
    public void CanCalculateReminder()
    {
        (string text, object result)[] texts =
        {
            (text: "5 % 3", result: 5.0 % 3.0),
            (text: "25 % 3 % 2", result: 25.0 % 3.0 % 2.0),
            (text: "10 % 2 % 5", result: 10.0 % 2.0 % 5.0)
        };

        foreach (var (text, result) in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);

            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);

            var scopeNode = DeclarationsCollector.Collect(expression.Value);

            Assert.NotNull(scopeNode);

            var type = TypeResolver.Resolve(scopeNode);

            if (type.IsError)
            {
                _testOutputHelper.WriteLine(type.Error.Message);
                _testOutputHelper.WriteLine(type.Error.Range.ToString());
            }

            Assert.NotNull(type.Value);
            Assert.Null(type.Error);

            _testOutputHelper.WriteLine(type.ToString());

            var value = ExpressionEvaluator.Evaluate(scopeNode);

            Assert.NotNull(value.Value);
            Assert.Null(value.Error);

            Assert.Equivalent(result, value.Value);
            _testOutputHelper.WriteLine(value.ToString());
        }
    }

    [Fact]
    public void CanEvaluateArrayDeclarations()
    {
        (string text, object result)[] texts =
        {
            (text: "number[] numbers", result: Empty.Instance),
            (text: "number[] numbers = number[10]", result: new double[10]),
            (text: "number n = number[10][0]", result: 0.0),
            (text: "number[] numbers = [1,2,3,4,5]", result: new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }),
            (text: "number n = [1,2,3,4,5][3]", result: 4.0),
            (text: "string[] colors = ['red', 'blue', 'green', 'yellow', 'purple', 'pink']",
                new[] { "red", "blue", "green", "yellow", "purple", "pink" })
        };

        foreach (var (text, result) in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);

            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);

            var scopeNode = DeclarationsCollector.Collect(expression.Value);

            Assert.NotNull(scopeNode);

            var type = TypeResolver.Resolve(scopeNode);

            if (type.IsError)
            {
                _testOutputHelper.WriteLine(type.Error.Message);
                _testOutputHelper.WriteLine(type.Error.Range.ToString());
            }

            Assert.NotNull(type.Value);
            Assert.Null(type.Error);

            _testOutputHelper.WriteLine(type.ToString());

            var value = ExpressionEvaluator.Evaluate(scopeNode);

            Assert.NotNull(value.Value);
            Assert.Null(value.Error);

            Assert.Equivalent(result, value.Value);
            _testOutputHelper.WriteLine(value.ToString());
        }
    }
    
    [Fact]
    public void CanEvaluateElementAccessExpressions()
    {
        (string text, object result)[] texts =
        {
            (text: "number n = number[10][0]", result: 0.0),
            (text: "number[] numbers = [1,2,3,4,5]", result: new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }),
            (text: "number n = [1,2,3,4,5][3]", result: 4.0),
            (text: "number[] numbers = number[3]\n\nnumbers[0] = 1\nnumbers[1] = -2.4\nnumbers[2] = 32", result: 32.0)
        };

        foreach (var (text, result) in texts)
        {
            _testOutputHelper.WriteLine(text);

            var expression = ExpressionsParser.Parse(text);

            if (expression.Error is not null)
                _testOutputHelper.WriteLine(expression.Error.Message);

            Assert.NotNull(expression.Value);
            Assert.Null(expression.Error);

            var scopeNode = DeclarationsCollector.Collect(expression.Value);

            Assert.NotNull(scopeNode);

            var type = TypeResolver.Resolve(scopeNode);

            if (type.IsError)
            {
                _testOutputHelper.WriteLine(type.Error.Message);
                _testOutputHelper.WriteLine(type.Error.Range.ToString());
            }

            Assert.NotNull(type.Value);
            Assert.Null(type.Error);

            _testOutputHelper.WriteLine(type.ToString());

            var value = ExpressionEvaluator.Evaluate(scopeNode);

            Assert.NotNull(value.Value);
            Assert.Null(value.Error);

            Assert.Equivalent(result, value.Value);
            _testOutputHelper.WriteLine(value.ToString());
        }
    }

    [Fact]
    public void CanInvokeOverloadedFunctions()
    {
        var foo = new Function("foo", Enumerable.Empty<Variable>(),
            args => _testOutputHelper.WriteLine("Invoked foo without args"));

        var fooOverloadNumber = new Function("foo", new[] { new Variable("a", typeof(double)) },
            arg => _testOutputHelper.WriteLine($"Invoked foo with argument {arg[0]}"));

        var fooOverloadString = new Function<string>("foo", new[] { new Variable("a", typeof(string)) },
            arg =>
            {
                _testOutputHelper.WriteLine($"Invoked foo with argument {arg[0]}. Returns string");

                return arg[0].ToString() ?? string.Empty;
            });

        var fooOverloadObject = new Function("foo", new[] { new Variable("a", typeof(object)) },
            arg => _testOutputHelper.WriteLine($"Invoked foo with object argument {arg[0]}"));

        var interpreter = InterpreterBuilder.CreateBuilder()
            .WithPredefinedFunction(foo)
            .WithPredefinedFunction(fooOverloadNumber)
            .WithPredefinedFunction(fooOverloadString)
            .WithPredefinedFunction(fooOverloadObject)
            .Build();

        Assert.NotNull(interpreter);

        const string text = "foo()\n foo(2) \n foo('Hello, world') \n foo(foo('foo inside foo')) \n foo(false)";

        _testOutputHelper.WriteLine(text);
        _testOutputHelper.WriteLine("\nresult:\n");

        interpreter.Initialize(text);

        if (interpreter.HasErrors)
            _testOutputHelper.WriteLine(interpreter.Error.Message);

        Assert.False(interpreter.HasErrors);

        var result = interpreter.Interpret(CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void CanInvokeFunctionWithArrayParameter()
    {
        var lengthFunction = new Function<double>("length", new[] { new Variable("array", typeof(Array)) }, args => ((Array)args[0]).Length);

        var interpreter = InterpreterBuilder.CreateBuilder()
            .WithPredefinedFunction(lengthFunction)
            .Build();
        
        Assert.NotNull(interpreter);
        
        (string text, object result)[] texts =
        {
            (text: "length(number[10])", result: 10.0),
            (text: "length(['12', 'ssf', 'ggeer'])", result: 3.0),
            (text: "bool[] array = bool[213]\nlength(array)", result: 213.0)
        };

        foreach (var (text, result) in texts)
        {
            _testOutputHelper.WriteLine(text);

            interpreter.Initialize(text);

            if (interpreter.HasErrors)
                _testOutputHelper.WriteLine(interpreter.Error.Message);

            Assert.False(interpreter.HasErrors);

            var r = interpreter.Interpret(CancellationToken.None);

            Assert.NotNull(r.Value);
            Assert.Null(r.Error);
            Assert.Equivalent(result, r.Value);
        }
    }
}