using System.Diagnostics.CodeAnalysis;

namespace LanguageParser.Common;

public static class Result
{
    public static Result<TError, TValue> FromError<TError, TValue>(TError error)
    {
        return new Result<TError, TValue>(error);
    }

    public static Result<TError, TValue> FromValue<TError, TValue>(TValue value)
    {
        return new Result<TError, TValue>(value);
    }
}

public readonly struct Result<TError, TValue>
{
    public TError? Error { get; }
    public TValue? Value { get; }

    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsError => Error is not null;

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsValue => Value is not null;

    public bool IsDefault => Error is null && Value is null;

    public Result(TError? error)
    {
        Error = error;
        Value = default;
    }

    public Result(TValue? value)
    {
        Value = value;
        Error = default;
    }

    public static implicit operator Result<TError, TValue>(TError error)
    {
        return new Result<TError, TValue>(error);
    }

    public static implicit operator Result<TError, TValue>(TValue value)
    {
        return new Result<TError, TValue>(value);
    }

    public static explicit operator TError(Result<TError, TValue> either)
    {
        return either.IsError
            ? either.Error
            : throw new InvalidCastException();
    }

    public static explicit operator TValue(Result<TError, TValue> either)
    {
        return either.IsValue
            ? either.Value
            : throw new InvalidCastException();
    }

    public Result<TError, TValueNew> Bind<TValueNew>(Func<TValue, Result<TError, TValueNew>> bind)
    {
        return IsValue
            ? bind.Invoke(Value)
            : new Result<TError, TValueNew>(Error);
    }

    public Result<TErrorNew, TValueNew> Map<TErrorNew, TValueNew>(Func<TError, TErrorNew> mapError,
        Func<TValue, TValueNew> mapValue)
    {
        if (IsValue)
            return new Result<TErrorNew, TValueNew>(mapValue.Invoke(Value));
        if (IsError)
            return new Result<TErrorNew, TValueNew>(mapError.Invoke(Error));
        throw new InvalidOperationException();
    }

    public Result<TError, TValueNew> MapValue<TValueNew>(Func<TValue, TValueNew> mapValue)
    {
        return IsValue
            ? new Result<TError, TValueNew>(mapValue.Invoke(Value))
            : new Result<TError, TValueNew>(Error);
    }

    public Result<TErrorNew, TValue> MapError<TErrorNew>(Func<TError, TErrorNew> mapError)
    {
        return IsError
            ? new Result<TErrorNew, TValue>(mapError.Invoke(Error))
            : new Result<TErrorNew, TValue>(Value);
    }

    public TResult Join<TResult>(Func<TError, TResult> mapError, Func<TValue, TResult> mapValue)
    {
        if (IsValue)
            return mapValue.Invoke(Value);
        if (IsError)
            return mapError.Invoke(Error);
        throw new InvalidOperationException();
    }

    public override string ToString()
    {
        if (IsValue)
            return "Value: " + Value;
        if (IsError)
            return "Error: " + Error;
        return "Default";
    }

    #region equality

    public static bool operator ==(Result<TError, TValue>? left, Result<TError, TValue>? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(Result<TError, TValue>? left, Result<TError, TValue>? right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        return obj is Result<TError, TValue> other && Equals(other);
    }

    public bool Equals(Result<TError, TValue> other)
    {
        if (IsError && other.IsError)
            return EqualityComparer<TError>.Default.Equals(Error, other.Error);
        if (IsValue && other.IsValue)
            return EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        return false;
    }

    public override int GetHashCode()
    {
        if (IsValue)
            return EqualityComparer<TValue>.Default.GetHashCode(Value);
        if (IsError)
            return EqualityComparer<TError>.Default.GetHashCode(Error);
        return 0;
    }

    #endregion
}