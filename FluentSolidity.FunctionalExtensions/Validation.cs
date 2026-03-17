namespace FluentSolidity.FunctionalExtensions;

public static class Validation
{
    /// <summary>
    /// Creates a valid Validation containing a value.
    /// </summary>
    public static Validation<TError, T> Valid<TError, T>(T value) => new(value);

    /// <summary>
    /// Creates an invalid Validation containing one or more errors.
    /// </summary>
    public static Validation<TError, T> Invalid<TError, T>(params TError[] errors) => new(errors);

    /// <summary>
    /// Creates an invalid Validation containing a read-only list of errors.
    /// </summary>
    public static Validation<TError, T> Invalid<TError, T>(IReadOnlyList<TError> errors) => new(errors);

    #region Match

    /// <summary>
    /// Collapses a Validation to a single value by providing a mapper for both states.
    /// </summary>
    public static TResult Match<TError, T, TResult>(this Validation<TError, T> validation,
        Func<T, TResult> validMapper,
        Func<IReadOnlyList<TError>, TResult> errorMapper) =>
        validation.MatchErrors(out var errors) ? errorMapper(errors) : validMapper(validation.Value!);

    /// <summary>
    /// Async Match: sync Validation, async mappers.
    /// </summary>
    public static async Task<TResult> Match<TError, T, TResult>(this Validation<TError, T> validation,
        Func<T, Task<TResult>> validMapper,
        Func<IReadOnlyList<TError>, Task<TResult>> errorMapper) =>
        validation.MatchErrors(out var errors) ? await errorMapper(errors) : await validMapper(validation.Value!);

    /// <summary>
    /// Async Match: async Validation, sync mappers.
    /// </summary>
    public static async Task<TResult> Match<TError, T, TResult>(this Task<Validation<TError, T>> validation,
        Func<T, TResult> validMapper,
        Func<IReadOnlyList<TError>, TResult> errorMapper) => (await validation).Match(validMapper, errorMapper);

    /// <summary>
    /// Async Match: async Validation, async mappers.
    /// </summary>
    public static async Task<TResult> Match<TError, T, TResult>(this Task<Validation<TError, T>> validation,
        Func<T, Task<TResult>> validMapper,
        Func<IReadOnlyList<TError>, Task<TResult>> errorMapper) => await (await validation).Match(validMapper, errorMapper);

    #endregion

    #region Combine (2-arity)

    /// <summary>
    /// Combines two Validations. If both are valid, applies the combiner function.
    /// If any are invalid, accumulates all errors.
    /// </summary>
    public static Validation<TError, TResult> Combine<TError, T1, T2, TResult>(
        this Validation<TError, T1> first,
        Validation<TError, T2> second,
        Func<T1, T2, TResult> combiner)
    {
        var firstInvalid = first.MatchErrors(out var firstErrors);
        var secondInvalid = second.MatchErrors(out var secondErrors);

        if (!firstInvalid && !secondInvalid)
            return new Validation<TError, TResult>(combiner(first.Value!, second.Value!));

        return new Validation<TError, TResult>(AccumulateErrors(firstInvalid, firstErrors, secondInvalid, secondErrors));
    }

    /// <summary>
    /// Async Combine (2-arity): sync Validations, async combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, TResult>(
        this Validation<TError, T1> first,
        Validation<TError, T2> second,
        Func<T1, T2, Task<TResult>> combiner)
    {
        var firstInvalid = first.MatchErrors(out var firstErrors);
        var secondInvalid = second.MatchErrors(out var secondErrors);

        if (!firstInvalid && !secondInvalid)
            return new Validation<TError, TResult>(await combiner(first.Value!, second.Value!));

        return new Validation<TError, TResult>(AccumulateErrors(firstInvalid, firstErrors, secondInvalid, secondErrors));
    }

    /// <summary>
    /// Async Combine (2-arity): async Validations, sync combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, TResult>(
        this Task<Validation<TError, T1>> first,
        Task<Validation<TError, T2>> second,
        Func<T1, T2, TResult> combiner) => (await first).Combine(await second, combiner);

    /// <summary>
    /// Async Combine (2-arity): async Validations, async combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, TResult>(
        this Task<Validation<TError, T1>> first,
        Task<Validation<TError, T2>> second,
        Func<T1, T2, Task<TResult>> combiner) => await (await first).Combine(await second, combiner);

    #endregion

    #region Combine (3-arity)

    /// <summary>
    /// Combines three Validations. If all are valid, applies the combiner function.
    /// If any are invalid, accumulates all errors.
    /// </summary>
    public static Validation<TError, TResult> Combine<TError, T1, T2, T3, TResult>(
        this Validation<TError, T1> first,
        Validation<TError, T2> second,
        Validation<TError, T3> third,
        Func<T1, T2, T3, TResult> combiner)
    {
        var errors = CollectErrors(first, second, third);
        return errors.Count > 0
            ? new Validation<TError, TResult>(errors)
            : new Validation<TError, TResult>(combiner(first.Value!, second.Value!, third.Value!));
    }

    /// <summary>
    /// Async Combine (3-arity): sync Validations, async combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, TResult>(
        this Validation<TError, T1> first,
        Validation<TError, T2> second,
        Validation<TError, T3> third,
        Func<T1, T2, T3, Task<TResult>> combiner)
    {
        var errors = CollectErrors(first, second, third);
        return errors.Count > 0
            ? new Validation<TError, TResult>(errors)
            : new Validation<TError, TResult>(await combiner(first.Value!, second.Value!, third.Value!));
    }

    /// <summary>
    /// Async Combine (3-arity): async Validations, sync combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, TResult>(
        this Task<Validation<TError, T1>> first,
        Task<Validation<TError, T2>> second,
        Task<Validation<TError, T3>> third,
        Func<T1, T2, T3, TResult> combiner) => (await first).Combine(await second, await third, combiner);

    /// <summary>
    /// Async Combine (3-arity): async Validations, async combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, TResult>(
        this Task<Validation<TError, T1>> first,
        Task<Validation<TError, T2>> second,
        Task<Validation<TError, T3>> third,
        Func<T1, T2, T3, Task<TResult>> combiner) => await (await first).Combine(await second, await third, combiner);

    #endregion

    #region Combine (4-arity)

    /// <summary>
    /// Combines four Validations. If all are valid, applies the combiner function.
    /// If any are invalid, accumulates all errors.
    /// </summary>
    public static Validation<TError, TResult> Combine<TError, T1, T2, T3, T4, TResult>(
        this Validation<TError, T1> first,
        Validation<TError, T2> second,
        Validation<TError, T3> third,
        Validation<TError, T4> fourth,
        Func<T1, T2, T3, T4, TResult> combiner)
    {
        var errors = CollectErrors(first, second, third, fourth);
        return errors.Count > 0
            ? new Validation<TError, TResult>(errors)
            : new Validation<TError, TResult>(combiner(first.Value!, second.Value!, third.Value!, fourth.Value!));
    }

    /// <summary>
    /// Async Combine (4-arity): sync Validations, async combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, T4, TResult>(
        this Validation<TError, T1> first,
        Validation<TError, T2> second,
        Validation<TError, T3> third,
        Validation<TError, T4> fourth,
        Func<T1, T2, T3, T4, Task<TResult>> combiner)
    {
        var errors = CollectErrors(first, second, third, fourth);
        return errors.Count > 0
            ? new Validation<TError, TResult>(errors)
            : new Validation<TError, TResult>(await combiner(first.Value!, second.Value!, third.Value!, fourth.Value!));
    }

    /// <summary>
    /// Async Combine (4-arity): async Validations, sync combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, T4, TResult>(
        this Task<Validation<TError, T1>> first,
        Task<Validation<TError, T2>> second,
        Task<Validation<TError, T3>> third,
        Task<Validation<TError, T4>> fourth,
        Func<T1, T2, T3, T4, TResult> combiner) => (await first).Combine(await second, await third, await fourth, combiner);

    /// <summary>
    /// Async Combine (4-arity): async Validations, async combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, T4, TResult>(
        this Task<Validation<TError, T1>> first,
        Task<Validation<TError, T2>> second,
        Task<Validation<TError, T3>> third,
        Task<Validation<TError, T4>> fourth,
        Func<T1, T2, T3, T4, Task<TResult>> combiner) => await (await first).Combine(await second, await third, await fourth, combiner);

    #endregion

    #region Combine (5-arity)

    /// <summary>
    /// Combines five Validations. If all are valid, applies the combiner function.
    /// If any are invalid, accumulates all errors.
    /// </summary>
    public static Validation<TError, TResult> Combine<TError, T1, T2, T3, T4, T5, TResult>(
        this Validation<TError, T1> first,
        Validation<TError, T2> second,
        Validation<TError, T3> third,
        Validation<TError, T4> fourth,
        Validation<TError, T5> fifth,
        Func<T1, T2, T3, T4, T5, TResult> combiner)
    {
        var errors = CollectErrors(first, second, third, fourth, fifth);
        return errors.Count > 0
            ? new Validation<TError, TResult>(errors)
            : new Validation<TError, TResult>(combiner(first.Value!, second.Value!, third.Value!, fourth.Value!, fifth.Value!));
    }

    /// <summary>
    /// Async Combine (5-arity): sync Validations, async combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, T4, T5, TResult>(
        this Validation<TError, T1> first,
        Validation<TError, T2> second,
        Validation<TError, T3> third,
        Validation<TError, T4> fourth,
        Validation<TError, T5> fifth,
        Func<T1, T2, T3, T4, T5, Task<TResult>> combiner)
    {
        var errors = CollectErrors(first, second, third, fourth, fifth);
        return errors.Count > 0
            ? new Validation<TError, TResult>(errors)
            : new Validation<TError, TResult>(await combiner(first.Value!, second.Value!, third.Value!, fourth.Value!, fifth.Value!));
    }

    /// <summary>
    /// Async Combine (5-arity): async Validations, sync combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, T4, T5, TResult>(
        this Task<Validation<TError, T1>> first,
        Task<Validation<TError, T2>> second,
        Task<Validation<TError, T3>> third,
        Task<Validation<TError, T4>> fourth,
        Task<Validation<TError, T5>> fifth,
        Func<T1, T2, T3, T4, T5, TResult> combiner) => (await first).Combine(await second, await third, await fourth, await fifth, combiner);

    /// <summary>
    /// Async Combine (5-arity): async Validations, async combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, T4, T5, TResult>(
        this Task<Validation<TError, T1>> first,
        Task<Validation<TError, T2>> second,
        Task<Validation<TError, T3>> third,
        Task<Validation<TError, T4>> fourth,
        Task<Validation<TError, T5>> fifth,
        Func<T1, T2, T3, T4, T5, Task<TResult>> combiner) => await (await first).Combine(await second, await third, await fourth, await fifth, combiner);

    #endregion

    #region Combine (6-arity)

    /// <summary>
    /// Combines six Validations. If all are valid, applies the combiner function.
    /// If any are invalid, accumulates all errors.
    /// </summary>
    public static Validation<TError, TResult> Combine<TError, T1, T2, T3, T4, T5, T6, TResult>(
        this Validation<TError, T1> first,
        Validation<TError, T2> second,
        Validation<TError, T3> third,
        Validation<TError, T4> fourth,
        Validation<TError, T5> fifth,
        Validation<TError, T6> sixth,
        Func<T1, T2, T3, T4, T5, T6, TResult> combiner)
    {
        var errors = CollectErrors(first, second, third, fourth, fifth, sixth);
        return errors.Count > 0
            ? new Validation<TError, TResult>(errors)
            : new Validation<TError, TResult>(combiner(first.Value!, second.Value!, third.Value!, fourth.Value!, fifth.Value!, sixth.Value!));
    }

    /// <summary>
    /// Async Combine (6-arity): sync Validations, async combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, T4, T5, T6, TResult>(
        this Validation<TError, T1> first,
        Validation<TError, T2> second,
        Validation<TError, T3> third,
        Validation<TError, T4> fourth,
        Validation<TError, T5> fifth,
        Validation<TError, T6> sixth,
        Func<T1, T2, T3, T4, T5, T6, Task<TResult>> combiner)
    {
        var errors = CollectErrors(first, second, third, fourth, fifth, sixth);
        return errors.Count > 0
            ? new Validation<TError, TResult>(errors)
            : new Validation<TError, TResult>(await combiner(first.Value!, second.Value!, third.Value!, fourth.Value!, fifth.Value!, sixth.Value!));
    }

    /// <summary>
    /// Async Combine (6-arity): async Validations, sync combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, T4, T5, T6, TResult>(
        this Task<Validation<TError, T1>> first,
        Task<Validation<TError, T2>> second,
        Task<Validation<TError, T3>> third,
        Task<Validation<TError, T4>> fourth,
        Task<Validation<TError, T5>> fifth,
        Task<Validation<TError, T6>> sixth,
        Func<T1, T2, T3, T4, T5, T6, TResult> combiner) => (await first).Combine(await second, await third, await fourth, await fifth, await sixth, combiner);

    /// <summary>
    /// Async Combine (6-arity): async Validations, async combiner.
    /// </summary>
    public static async Task<Validation<TError, TResult>> Combine<TError, T1, T2, T3, T4, T5, T6, TResult>(
        this Task<Validation<TError, T1>> first,
        Task<Validation<TError, T2>> second,
        Task<Validation<TError, T3>> third,
        Task<Validation<TError, T4>> fourth,
        Task<Validation<TError, T5>> fifth,
        Task<Validation<TError, T6>> sixth,
        Func<T1, T2, T3, T4, T5, T6, Task<TResult>> combiner) => await (await first).Combine(await second, await third, await fourth, await fifth, await sixth, combiner);

    #endregion

    #region Internal error accumulation helpers

    private static List<TError> AccumulateErrors<TError>(
        bool firstInvalid, IReadOnlyList<TError> firstErrors,
        bool secondInvalid, IReadOnlyList<TError> secondErrors)
    {
        var errors = new List<TError>();
        if (firstInvalid) errors.AddRange(firstErrors);
        if (secondInvalid) errors.AddRange(secondErrors);
        return errors;
    }

    private static List<TError> CollectErrors<TError, T1, T2, T3>(
        Validation<TError, T1> v1, Validation<TError, T2> v2, Validation<TError, T3> v3)
    {
        var errors = new List<TError>();
        if (v1.MatchErrors(out var e1)) errors.AddRange(e1);
        if (v2.MatchErrors(out var e2)) errors.AddRange(e2);
        if (v3.MatchErrors(out var e3)) errors.AddRange(e3);
        return errors;
    }

    private static List<TError> CollectErrors<TError, T1, T2, T3, T4>(
        Validation<TError, T1> v1, Validation<TError, T2> v2, Validation<TError, T3> v3, Validation<TError, T4> v4)
    {
        var errors = new List<TError>();
        if (v1.MatchErrors(out var e1)) errors.AddRange(e1);
        if (v2.MatchErrors(out var e2)) errors.AddRange(e2);
        if (v3.MatchErrors(out var e3)) errors.AddRange(e3);
        if (v4.MatchErrors(out var e4)) errors.AddRange(e4);
        return errors;
    }

    private static List<TError> CollectErrors<TError, T1, T2, T3, T4, T5>(
        Validation<TError, T1> v1, Validation<TError, T2> v2, Validation<TError, T3> v3,
        Validation<TError, T4> v4, Validation<TError, T5> v5)
    {
        var errors = new List<TError>();
        if (v1.MatchErrors(out var e1)) errors.AddRange(e1);
        if (v2.MatchErrors(out var e2)) errors.AddRange(e2);
        if (v3.MatchErrors(out var e3)) errors.AddRange(e3);
        if (v4.MatchErrors(out var e4)) errors.AddRange(e4);
        if (v5.MatchErrors(out var e5)) errors.AddRange(e5);
        return errors;
    }

    private static List<TError> CollectErrors<TError, T1, T2, T3, T4, T5, T6>(
        Validation<TError, T1> v1, Validation<TError, T2> v2, Validation<TError, T3> v3,
        Validation<TError, T4> v4, Validation<TError, T5> v5, Validation<TError, T6> v6)
    {
        var errors = new List<TError>();
        if (v1.MatchErrors(out var e1)) errors.AddRange(e1);
        if (v2.MatchErrors(out var e2)) errors.AddRange(e2);
        if (v3.MatchErrors(out var e3)) errors.AddRange(e3);
        if (v4.MatchErrors(out var e4)) errors.AddRange(e4);
        if (v5.MatchErrors(out var e5)) errors.AddRange(e5);
        if (v6.MatchErrors(out var e6)) errors.AddRange(e6);
        return errors;
    }

    #endregion
}

/// <summary>
/// Represents the result of a validation that either holds a valid value or a collection of errors.
/// Unlike <see cref="Result{T}"/>, Validation is designed for applicative error accumulation:
/// multiple independent validations can be combined, collecting all errors rather than short-circuiting.
/// </summary>
/// <typeparam name="TError">The type of errors</typeparam>
/// <typeparam name="T">The type of the valid value</typeparam>
public readonly struct Validation<TError, T>
{
    /// <summary>
    /// Explicit tag indicating whether this Validation is in an error state.
    /// For a default-initialized struct, this will be false (valid track).
    /// </summary>
    private readonly bool _isError;

    /// <summary>
    /// The valid value, if any.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// The collection of errors, if any.
    /// </summary>
    public IReadOnlyList<TError> Errors { get; }

    /// <summary>
    /// Constructs a valid Validation containing a value.
    /// </summary>
    public Validation(T value)
    {
        _isError = false;
        Value = value;
        Errors = Array.Empty<TError>();
    }

    /// <summary>
    /// Constructs an invalid Validation containing errors.
    /// </summary>
    public Validation(IReadOnlyList<TError> errors)
    {
        _isError = true;
        Value = default;
        Errors = errors ?? Array.Empty<TError>();
    }

    /// <summary>
    /// Determines if this Validation is in the valid state.
    /// </summary>
    /// <param name="value">out variable that will be assigned the valid value</param>
    /// <returns>true if valid, false otherwise</returns>
    public bool MatchValid(out T value)
    {
        value = Value!;
        return !_isError;
    }

    /// <summary>
    /// Determines if this Validation is in the error state.
    /// </summary>
    /// <param name="errors">out variable that will be assigned the error collection</param>
    /// <returns>true if invalid, false otherwise</returns>
    public bool MatchErrors(out IReadOnlyList<TError> errors)
    {
        errors = Errors;
        return _isError;
    }

    public override string ToString() =>
        _isError
            ? $"Invalid([{string.Join(", ", Errors)}])"
            : $"Valid({Value})";

    public static implicit operator Validation<TError, T>(T value) => new(value);
}
