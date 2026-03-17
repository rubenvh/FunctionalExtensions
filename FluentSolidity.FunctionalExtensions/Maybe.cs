namespace FluentSolidity.FunctionalExtensions;

public static class Maybe
{
    /// <summary>
    /// Creates a Maybe containing a value.
    /// </summary>
    public static Maybe<T> Some<T>(T value) => new(value);

    /// <summary>
    /// Creates an empty Maybe.
    /// </summary>
    public static Maybe<T> None<T>() => Maybe<T>.None;

    #region Match

    /// <summary>
    /// Collapses a Maybe to a single value by providing a mapper for both states.
    /// </summary>
    public static TResult Match<T, TResult>(this Maybe<T> maybe,
        Func<T, TResult> someMapper,
        Func<TResult> noneMapper) =>
        maybe.MatchSome(out var value) ? someMapper(value) : noneMapper();

    /// <summary>
    /// Async Match: sync Maybe, async mappers.
    /// </summary>
    public static async Task<TResult> Match<T, TResult>(this Maybe<T> maybe,
        Func<T, Task<TResult>> someMapper,
        Func<Task<TResult>> noneMapper) =>
        maybe.MatchSome(out var value) ? await someMapper(value) : await noneMapper();

    /// <summary>
    /// Async Match: async Maybe, sync mappers.
    /// </summary>
    public static async Task<TResult> Match<T, TResult>(this Task<Maybe<T>> maybe,
        Func<T, TResult> someMapper,
        Func<TResult> noneMapper) => (await maybe).Match(someMapper, noneMapper);

    /// <summary>
    /// Async Match: async Maybe, async mappers.
    /// </summary>
    public static async Task<TResult> Match<T, TResult>(this Task<Maybe<T>> maybe,
        Func<T, Task<TResult>> someMapper,
        Func<Task<TResult>> noneMapper) => await (await maybe).Match(someMapper, noneMapper);

    #endregion

    #region Map

    /// <summary>
    /// Maps a function over a Maybe. Does nothing on None.
    /// </summary>
    public static Maybe<TResult> Map<T, TResult>(this Maybe<T> maybe, Func<T, TResult> mapper) =>
        maybe.MatchSome(out var value)
            ? new Maybe<TResult>(mapper(value))
            : Maybe<TResult>.None;

    /// <summary>
    /// Async Map: sync Maybe, async mapper.
    /// </summary>
    public static async Task<Maybe<TResult>> Map<T, TResult>(this Maybe<T> maybe,
        Func<T, Task<TResult>> mapper) =>
        maybe.MatchSome(out var value)
            ? new Maybe<TResult>(await mapper(value))
            : Maybe<TResult>.None;

    /// <summary>
    /// Async Map: async Maybe, sync mapper.
    /// </summary>
    public static async Task<Maybe<TResult>> Map<T, TResult>(this Task<Maybe<T>> maybe,
        Func<T, TResult> mapper) => (await maybe).Map(mapper);

    /// <summary>
    /// Async Map: async Maybe, async mapper.
    /// </summary>
    public static async Task<Maybe<TResult>> Map<T, TResult>(this Task<Maybe<T>> maybe,
        Func<T, Task<TResult>> mapper) => await (await maybe).Map(mapper);

    #endregion

    #region Bind

    /// <summary>
    /// Binds a function over a Maybe. Does nothing on None.
    /// Use Bind when the mapper returns a Maybe.
    /// </summary>
    public static Maybe<TResult> Bind<T, TResult>(this Maybe<T> maybe, Func<T, Maybe<TResult>> mapper) =>
        maybe.MatchSome(out var value)
            ? mapper(value)
            : Maybe<TResult>.None;

    /// <summary>
    /// Async Bind: sync Maybe, async mapper.
    /// </summary>
    public static async Task<Maybe<TResult>> Bind<T, TResult>(this Maybe<T> maybe,
        Func<T, Task<Maybe<TResult>>> mapper) =>
        maybe.MatchSome(out var value)
            ? await mapper(value)
            : Maybe<TResult>.None;

    /// <summary>
    /// Async Bind: async Maybe, sync mapper.
    /// </summary>
    public static async Task<Maybe<TResult>> Bind<T, TResult>(this Task<Maybe<T>> maybe,
        Func<T, Maybe<TResult>> mapper) => (await maybe).Bind(mapper);

    /// <summary>
    /// Async Bind: async Maybe, async mapper.
    /// </summary>
    public static async Task<Maybe<TResult>> Bind<T, TResult>(this Task<Maybe<T>> maybe,
        Func<T, Task<Maybe<TResult>>> mapper) => await (await maybe).Bind(mapper);

    #endregion

    #region Do

    /// <summary>
    /// Executes an action on the Some value without modifying the Maybe.
    /// </summary>
    public static Maybe<T> Do<T>(this Maybe<T> maybe, Action<T> action)
    {
        if (maybe.MatchSome(out var value)) action(value);
        return maybe;
    }

    /// <summary>
    /// Async Do: sync Maybe, async action.
    /// </summary>
    public static async Task<Maybe<T>> Do<T>(this Maybe<T> maybe, Func<T, Task> action)
    {
        if (maybe.MatchSome(out var value)) await action(value);
        return maybe;
    }

    /// <summary>
    /// Async Do: async Maybe, sync action.
    /// </summary>
    public static async Task<Maybe<T>> Do<T>(this Task<Maybe<T>> maybe,
        Action<T> action) => (await maybe).Do(action);

    /// <summary>
    /// Async Do: async Maybe, async action.
    /// </summary>
    public static async Task<Maybe<T>> Do<T>(this Task<Maybe<T>> maybe,
        Func<T, Task> action) => await (await maybe).Do(action);

    #endregion

    #region DoNone

    /// <summary>
    /// Executes an action when the Maybe is None, without modifying the Maybe.
    /// Use this for side-effects on the empty track (e.g., logging).
    /// </summary>
    public static Maybe<T> DoNone<T>(this Maybe<T> maybe, Action action)
    {
        if (maybe.MatchNone()) action();
        return maybe;
    }

    /// <summary>
    /// Async DoNone: sync Maybe, async action.
    /// </summary>
    public static async Task<Maybe<T>> DoNone<T>(this Maybe<T> maybe, Func<Task> action)
    {
        if (maybe.MatchNone()) await action();
        return maybe;
    }

    /// <summary>
    /// Async DoNone: async Maybe, sync action.
    /// </summary>
    public static async Task<Maybe<T>> DoNone<T>(this Task<Maybe<T>> maybe,
        Action action) => (await maybe).DoNone(action);

    /// <summary>
    /// Async DoNone: async Maybe, async action.
    /// </summary>
    public static async Task<Maybe<T>> DoNone<T>(this Task<Maybe<T>> maybe,
        Func<Task> action) => await (await maybe).DoNone(action);

    #endregion

    #region Ensure

    /// <summary>
    /// Guards a Some Maybe by testing its value against a predicate.
    /// If the predicate returns true, the Maybe passes through unchanged.
    /// If the predicate returns false, the Maybe becomes None.
    /// Short-circuits on an already None Maybe.
    /// </summary>
    public static Maybe<T> Ensure<T>(this Maybe<T> maybe, Func<T, bool> predicate) =>
        maybe.MatchSome(out var value)
            ? predicate(value) ? maybe : Maybe<T>.None
            : maybe;

    /// <summary>
    /// Async Ensure: sync Maybe, async predicate.
    /// </summary>
    public static async Task<Maybe<T>> Ensure<T>(this Maybe<T> maybe,
        Func<T, Task<bool>> predicate) =>
        maybe.MatchSome(out var value)
            ? await predicate(value) ? maybe : Maybe<T>.None
            : maybe;

    /// <summary>
    /// Async Ensure: async Maybe, sync predicate.
    /// </summary>
    public static async Task<Maybe<T>> Ensure<T>(this Task<Maybe<T>> maybe,
        Func<T, bool> predicate) => (await maybe).Ensure(predicate);

    /// <summary>
    /// Async Ensure: async Maybe, async predicate.
    /// </summary>
    public static async Task<Maybe<T>> Ensure<T>(this Task<Maybe<T>> maybe,
        Func<T, Task<bool>> predicate) => await (await maybe).Ensure(predicate);

    #endregion

    #region OrDefault / OrElse

    /// <summary>
    /// Returns the value if Some, or default(T) if None.
    /// </summary>
    public static T? OrDefault<T>(this Maybe<T> maybe) =>
        maybe.MatchSome(out var value) ? value : default;

    /// <summary>
    /// Returns the value if Some, or the result of the fallback factory if None.
    /// </summary>
    public static T OrElse<T>(this Maybe<T> maybe, Func<T> fallback) =>
        maybe.MatchSome(out var value) ? value : fallback();

    /// <summary>
    /// Async OrElse: async Maybe, sync fallback.
    /// </summary>
    public static async Task<T> OrElse<T>(this Task<Maybe<T>> maybe, Func<T> fallback) =>
        (await maybe).OrElse(fallback);

    /// <summary>
    /// Async OrElse: sync Maybe, async fallback.
    /// </summary>
    public static async Task<T> OrElse<T>(this Maybe<T> maybe, Func<Task<T>> fallback) =>
        maybe.MatchSome(out var value) ? value : await fallback();

    /// <summary>
    /// Async OrElse: async Maybe, async fallback.
    /// </summary>
    public static async Task<T> OrElse<T>(this Task<Maybe<T>> maybe, Func<Task<T>> fallback) =>
        await (await maybe).OrElse(fallback);

    #endregion

    #region Support async query syntax

    public static async Task<Maybe<TResult>> Select<T, TResult>(this Maybe<T> maybe, Func<T, Task<TResult>> mapper) => await maybe.Map(mapper);
    public static async Task<Maybe<TResult>> Select<T, TResult>(this Task<Maybe<T>> maybe, Func<T, TResult> mapper) => await maybe.Map(mapper);
    public static async Task<Maybe<TResult>> Select<T, TResult>(this Task<Maybe<T>> maybe, Func<T, Task<TResult>> mapper) => await maybe.Map(mapper);

    public static async Task<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> maybe,
        Func<T, Task<Maybe<TIntermediate>>> binder, Func<T, TIntermediate, TResult> mapper) =>
        await maybe.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    public static async Task<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> maybe,
        Func<T, Maybe<TIntermediate>> binder, Func<T, TIntermediate, TResult> mapper) =>
        await maybe.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    public static async Task<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> maybe,
        Func<T, Task<Maybe<TIntermediate>>> binder, Func<T, TIntermediate, Task<TResult>> mapper) =>
        await maybe.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    public static async Task<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> maybe,
        Func<T, Task<Maybe<TIntermediate>>> binder, Func<T, TIntermediate, Task<Maybe<TResult>>> mapper) =>
        await maybe.Bind(r => binder(r).Bind(sub => mapper(r, sub)));

    #endregion
}

/// <summary>
/// Represents an optional value. Either contains a value (Some) or is empty (None).
/// </summary>
/// <typeparam name="T">The type of the contained value</typeparam>
public readonly struct Maybe<T>
{
    /// <summary>
    /// Explicit tag indicating whether this Maybe contains a value.
    /// Using a tag field instead of null-checking avoids fragile assumptions
    /// about reference-type defaults and makes the discrimination unambiguous.
    /// For a default-initialized struct, this will be false (None).
    /// </summary>
    private readonly bool _hasValue;

    /// <summary>
    /// The contained value, if any.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// A static None instance for this type.
    /// </summary>
    public static Maybe<T> None => default;

    /// <summary>
    /// Constructs a Maybe containing a value.
    /// </summary>
    public Maybe(T value)
    {
        _hasValue = true;
        Value = value;
    }

    /// <summary>
    /// Determines if this Maybe contains a value.
    /// </summary>
    /// <param name="value">out variable that will be assigned the value</param>
    /// <returns>true if this Maybe contains a value, false otherwise</returns>
    public bool MatchSome(out T value)
    {
        value = Value!;
        return _hasValue;
    }

    /// <summary>
    /// Determines if this Maybe is empty.
    /// </summary>
    /// <returns>true if this Maybe is None, false otherwise</returns>
    public bool MatchNone() => !_hasValue;

    public override string ToString() =>
        _hasValue ? $"Some({Value})" : "None";

    public static implicit operator Maybe<T>(T value) => new(value);

    #region Support query syntax

    public Maybe<TResult> Select<TResult>(Func<T, TResult> mapper) => this.Map(mapper);
    public async Task<Maybe<TResult>> Select<TResult>(Func<T, Task<TResult>> mapper) => await this.Map(mapper);

    // supports Maybe<T> from clauses and normal select clause
    public Maybe<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Maybe<TIntermediate>> binder, Func<T, TIntermediate, TResult> mapper) =>
        this.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    // supports Maybe<T> from clauses and Maybe<T> select clause
    public Maybe<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Maybe<TIntermediate>> binder, Func<T, TIntermediate, Maybe<TResult>> mapper) =>
        this.Bind(r => binder(r).Bind(sub => mapper(r, sub)));

    // supports async Maybe<T> from clause and sync select clause
    public async Task<Maybe<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Task<Maybe<TIntermediate>>> binder, Func<T, TIntermediate, TResult> mapper) =>
        await this.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    // support sync Maybe<T> from and async select
    public async Task<Maybe<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Maybe<TIntermediate>> binder, Func<T, TIntermediate, Task<TResult>> mapper) =>
        await this.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    // support sync Maybe<T> from and async select Maybe<T>
    public async Task<Maybe<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Maybe<TIntermediate>> binder, Func<T, TIntermediate, Task<Maybe<TResult>>> mapper) =>
        await this.Bind(r => binder(r).Bind(sub => mapper(r, sub)));

    #endregion
}
