using System;
using System.Collections.Generic;
using System.Linq;
using LanguageParser.Common;

namespace SimpleExecutor.Libraries;

public static class ArrayLibrary
{
    public static IEnumerable<FunctionBase> GetFunctions()
    {
        yield return FunctionBase.Create("length", args => (double)((Array)args[0]).Length, typeof(Array));

        yield return FunctionBase.Create<Array>("repeatElement", args =>
        {
            var n = (int)(double)args[1];

            return args[0] switch
            {
                double a => Enumerable.Repeat(a, n).ToArray(),
                string a => Enumerable.Repeat(a, n).ToArray(),
                bool a => Enumerable.Repeat(a, n).ToArray(),
                _ => throw new Exception("Unknown type")
            };
        }, typeof(object), typeof(double));

        yield return FunctionBase.Create("range", args =>
        {
            var n = (int)(double)args[1];

            var result = new double[n];

            for (var i = 0; i < n; i++)
                result[i] = (double)args[0] + i;

            return result;
        }, typeof(double), typeof(double));

        yield return FunctionBase.Create("range", args =>
        {
            var step = (double)args[1];
            var n = (int)(double)args[2];

            var result = new double[n];

            for (var i = 0; i < n; i++)
                result[i] = (double)args[0] + i * step;

            return result;
        }, typeof(double), typeof(double), typeof(double));

        yield return FunctionBase.Create("sum", args => ((double[])args[0]).Sum(), typeof(double[]));

        yield return FunctionBase.Create("min", args => ((double[])args[0]).Min(), typeof(double[]));

        yield return FunctionBase.Create("max", args => ((double[])args[0]).Max(), typeof(double[]));

        yield return FunctionBase.Create("concat", args => ((double[])args[0]).Concat((double[])args[1]).ToArray(),
            typeof(double[]), typeof(double[]));

        yield return FunctionBase.Create("concat", args => ((string[])args[0]).Concat((string[])args[1]).ToArray(),
            typeof(string[]), typeof(string[]));

        yield return FunctionBase.Create("concat", args => ((bool[])args[0]).Concat((bool[])args[1]).ToArray(),
            typeof(bool[]), typeof(bool[]));

        yield return FunctionBase.Create("sort", args => ((double[])args[0]).OrderBy(d => d).ToArray(),
            typeof(double[]));

        yield return FunctionBase.Create("sortDescending",
            args => ((double[])args[0]).OrderByDescending(d => d).ToArray(), typeof(double[]));

        yield return FunctionBase.Create("sort", args => ((string[])args[0]).OrderBy(d => d).ToArray(),
            typeof(string[]));

        yield return FunctionBase.Create("sortDescending",
            args => ((string[])args[0]).OrderByDescending(d => d).ToArray(), typeof(string[]));

        yield return FunctionBase.Create("reverse", args => ((double[])args[0]).Reverse().ToArray(), typeof(double[]));
        
        yield return FunctionBase.Create("reverse", args => ((string[])args[0]).Reverse().ToArray(), typeof(string[]));

        yield return FunctionBase.Create("reverse", args => ((bool[])args[0]).Reverse().ToArray(), typeof(bool[]));
    }
}