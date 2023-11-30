using System;
using System.Collections.Generic;
using LanguageParser.Common;

namespace SimpleExecutor.Libraries;

public static class MathLibrary
{
    public static IEnumerable<FunctionBase> GetMathFunctions()
    {
        yield return FunctionBase.Create("abs", args => Math.Abs((double)args[0]), typeof(double));

        yield return FunctionBase.Create("abs", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Abs(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("acos", args => Math.Acos((double)args[0]).ToDegree(), typeof(double));

        yield return FunctionBase.Create("acos", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Acos(array[i]).ToDegree();

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("acosh", args => Math.Acosh((double)args[0]).ToDegree(), typeof(double));

        yield return FunctionBase.Create("acosh", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Acosh(array[i]).ToDegree();

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("asin", args => Math.Asin((double)args[0]).ToDegree(), typeof(double));

        yield return FunctionBase.Create("asin", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Asin(array[i]).ToDegree();

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("asinh", args => Math.Asinh((double)args[0]).ToDegree(), typeof(double));

        yield return FunctionBase.Create("asinh", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Asinh(array[i]).ToDegree();

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("atan", args => Math.Atan((double)args[0]).ToDegree(), typeof(double));

        yield return FunctionBase.Create("atan", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Atan(array[i]).ToDegree();

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("atan2", args => Math.Atan2((double)args[0], (double)args[1]).ToDegree(),
            typeof(double),
            typeof(double));

        yield return FunctionBase.Create("atan2", args =>
        {
            var array1 = (double[])args[0];
            var array2 = (double[])args[1];
            var result = new double[array1.Length];

            for (var i = 0; i < array1.Length; i++)
                result[i] = Math.Atan2(array1[i], array2[i]).ToDegree();

            return result;
        }, typeof(double[]), typeof(double[]));

        yield return FunctionBase.Create("cbrt", args => Math.Cbrt((double)args[0]), typeof(double));

        yield return FunctionBase.Create("cbrt", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Cbrt(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("ceiling", args => Math.Ceiling((double)args[0]), typeof(double));

        yield return FunctionBase.Create("ceiling", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Ceiling(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("clamp", args => Math.Clamp((double)args[0], (double)args[1], (double)args[2]),
            typeof(double), typeof(double), typeof(double));

        yield return FunctionBase.Create("clamp", args =>
        {
            var array1 = (double[])args[0];
            var array2 = (double[])args[1];
            var array3 = (double[])args[2];
            var result = new double[array1.Length];

            for (var i = 0; i < result.Length; i++)
                result[i] = Math.Clamp(array1[i], array2[i], array3[i]);

            return result;
        }, typeof(double[]), typeof(double[]), typeof(double[]));

        yield return FunctionBase.Create("copySign", args => Math.CopySign((double)args[0], (double)args[1]),
            typeof(double), typeof(double));

        yield return FunctionBase.Create("copySign", args =>
        {
            var array1 = (double[])args[0];
            var array2 = (double[])args[1];
            var result = new double[array1.Length];

            for (var i = 0; i < result.Length; i++)
                result[i] = Math.CopySign(array1[i], array2[i]);

            return result;
        }, typeof(double[]), typeof(double[]));

        yield return FunctionBase.Create("cos", args => Math.Cos(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("cos", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Cos(array[i].ToRadians());

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("cosh", args => Math.Cosh(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("cosh", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Cosh(array[i].ToRadians());

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("exp", args => Math.Exp((double)args[0]), typeof(double));

        yield return FunctionBase.Create("exp", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Exp(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("floor", args => Math.Floor((double)args[0]), typeof(double));

        yield return FunctionBase.Create("floor", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Floor(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("iLogB", args => (double)Math.ILogB((double)args[0]), typeof(double));

        yield return FunctionBase.Create("iLogB", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.ILogB(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("log", args => Math.Log((double)args[0]), typeof(double));

        yield return FunctionBase.Create("log", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Log(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("log", args => Math.Log((double)args[0], (double)args[1]), typeof(double),
            typeof(double));

        yield return FunctionBase.Create("log", args =>
        {
            var array1 = (double[])args[0];
            var array2 = (double[])args[1];
            var result = new double[array1.Length];

            for (var i = 0; i < result.Length; i++)
                result[i] = Math.Log(array1[i], array2[i]);

            return result;
        }, typeof(double[]), typeof(double[]));

        yield return FunctionBase.Create("log10", args => Math.Log10((double)args[0]), typeof(double));

        yield return FunctionBase.Create("log10", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Log10(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("log2", args => Math.Log2((double)args[0]), typeof(double));

        yield return FunctionBase.Create("log2", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Log2(array[i]);

            return result;
        }, typeof(double[]));

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

        yield return FunctionBase.Create("pow", args =>
        {
            var array1 = (double[])args[0];
            var array2 = (double[])args[1];
            var result = new double[array1.Length];

            for (var i = 0; i < result.Length; i++)
                result[i] = Math.Pow(array1[i], array2[i]);

            return result;
        }, typeof(double[]), typeof(double[]));

        yield return FunctionBase.Create("reciprocalEstimate", args => Math.ReciprocalEstimate((double)args[0]),
            typeof(double));

        yield return FunctionBase.Create("reciprocalEstimate", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.ReciprocalEstimate(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("round", args => Math.Round((double)args[0]), typeof(double));

        yield return FunctionBase.Create("round", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Round(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("sign", args => Math.Sign((double)args[0]), typeof(double));

        yield return FunctionBase.Create("sign", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Sign(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("sin", args => Math.Sin(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("sin", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Sin(array[i].ToRadians());

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("sinh", args => Math.Sinh(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("sinh", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Sinh(array[i].ToRadians());

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("sqrt", args => Math.Sqrt((double)args[0]), typeof(double));

        yield return FunctionBase.Create("sqrt", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Sqrt(array[i]);

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("tan", args => Math.Tan(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("tan", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Tan(array[i].ToRadians());

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("tanh", args => Math.Tanh(((double)args[0]).ToRadians()), typeof(double));

        yield return FunctionBase.Create("tanh", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Tanh(array[i].ToRadians());

            return result;
        }, typeof(double[]));

        yield return FunctionBase.Create("truncate", args => Math.Truncate((double)args[0]), typeof(double));

        yield return FunctionBase.Create("truncate", args =>
        {
            var array = (double[])args[0];
            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = Math.Truncate(array[i]);

            return result;
        }, typeof(double[]));
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