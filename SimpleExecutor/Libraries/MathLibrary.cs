using System;
using System.Collections.Generic;
using LanguageParser.Common;

namespace SimpleExecutor.Libraries;

public static class MathLibrary
{
    public static IEnumerable<FunctionBase> GetMathFunctions()
    {
        yield return FunctionBase.Create("abs", args => Math.Abs((double)args[0]), typeof(double));

        yield return FunctionBase.Create("acos", args => Math.Acos((double)args[0]).ToDegree(), typeof(double));

        yield return FunctionBase.Create("acosh", args => Math.Acosh((double)args[0]).ToDegree(), typeof(double));

        yield return FunctionBase.Create("asin", args => Math.Asin((double)args[0]).ToDegree(), typeof(double));

        yield return FunctionBase.Create("asinh", args => Math.Asinh((double)args[0]).ToDegree(), typeof(double));

        yield return FunctionBase.Create("atan", args => Math.Atan((double)args[0]).ToDegree(), typeof(double));

        yield return FunctionBase.Create("atan2", args => Math.Atan2((double)args[0], (double)args[1]).ToDegree(),
            typeof(double),
            typeof(double));

        yield return FunctionBase.Create("cbrt", args => Math.Cbrt((double)args[0]), typeof(double));

        yield return FunctionBase.Create("ceiling", args => Math.Ceiling((double)args[0]), typeof(double));

        yield return FunctionBase.Create("clamp", args => Math.Clamp((double)args[0], (double)args[1], (double)args[2]),
            typeof(double), typeof(double), typeof(double));

        yield return FunctionBase.Create("copySign", args => Math.CopySign((double)args[0], (double)args[1]),
            typeof(double), typeof(double));

        yield return FunctionBase.Create("cos", args => Math.Cos(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("cosh", args => Math.Cosh(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("exp", args => Math.Exp((double)args[0]), typeof(double));

        yield return FunctionBase.Create("floor", args => Math.Floor((double)args[0]), typeof(double));

        yield return FunctionBase.Create("iLogB", args => (double)Math.ILogB((double)args[0]), typeof(double));

        yield return FunctionBase.Create("log", args => Math.Log((double)args[0]), typeof(double));

        yield return FunctionBase.Create("log", args => Math.Log((double)args[0], (double)args[1]), typeof(double),
            typeof(double));

        yield return FunctionBase.Create("log10", args => Math.Log10((double)args[0]), typeof(double));

        yield return FunctionBase.Create("log2", args => Math.Log2((double)args[0]), typeof(double));

        yield return FunctionBase.Create("max", args => Math.Max((double)args[0], (double)args[1]), typeof(double),
            typeof(double));

        yield return FunctionBase.Create("maxMagnitude", args => Math.MaxMagnitude((double)args[0], (double)args[1]),
            typeof(double), typeof(double));

        yield return FunctionBase.Create("min", args => Math.Min((double)args[0], (double)args[1]), typeof(double),
            typeof(double));

        yield return FunctionBase.Create("minMagnitude", args => Math.MinMagnitude((double)args[0], (double)args[1]),
            typeof(double), typeof(double));

        yield return FunctionBase.Create("pow", args => Math.Pow((double)args[0], (double)args[1]), typeof(double),
            typeof(double));

        yield return FunctionBase.Create("reciprocalEstimate", args => Math.ReciprocalEstimate((double)args[0]),
            typeof(double));

        yield return FunctionBase.Create("round", args => Math.Round((double)args[0]), typeof(double));

        yield return FunctionBase.Create("sign", args => Math.Sign((double)args[0]), typeof(double));

        yield return FunctionBase.Create("sin", args => Math.Sin(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("sinCos", args => Math.SinCos(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("sinh", args => Math.Sinh(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("sqrt", args => Math.Sqrt((double)args[0]), typeof(double));

        yield return FunctionBase.Create("tan", args => Math.Tan(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("tanh", args => Math.Tanh(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("truncate", args => Math.Truncate((double)args[0]), typeof(double));
    }

    private static double ToDegree(this double d)
    {
        return d * 180 / Math.PI;
    }

    private static double ToRadians(this double d)
    {
        return d * Math.PI / 180;
    }
}