namespace FluentSolidity.FunctionalExtensions;

public static class Either
{
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft left) => Either<TLeft, TRight>.Left(left);

    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight right) => Either<TLeft, TRight>.Right(right);

    #region Match

    /// <summary>
    /// Collapses an Either to a single value by providing a mapper for both sides.
    /// </summary>
    public static T Match<TLeft, TRight, T>(this Either<TLeft, TRight> either,
        Func<TRight, T> rightMapper,
        Func<TLeft, T> leftMapper) =>
        either.MatchLeft(out var left) ? leftMapper(left) : rightMapper(either.Value!);

    /// <summary>
    /// Async Match: sync Either, async mappers.
    /// </summary>
    public static async Task<T> Match<TLeft, TRight, T>(this Either<TLeft, TRight> either,
        Func<TRight, Task<T>> rightMapper,
        Func<TLeft, Task<T>> leftMapper) =>
        either.MatchLeft(out var left) ? await leftMapper(left) : await rightMapper(either.Value!);

    /// <summary>
    /// Async Match: async Either, sync mappers.
    /// </summary>
    public static async Task<T> Match<TLeft, TRight, T>(this Task<Either<TLeft, TRight>> either,
        Func<TRight, T> rightMapper,
        Func<TLeft, T> leftMapper) => (await either).Match(rightMapper, leftMapper);

    /// <summary>
    /// Async Match: async Either, async mappers.
    /// </summary>
    public static async Task<T> Match<TLeft, TRight, T>(this Task<Either<TLeft, TRight>> either,
        Func<TRight, Task<T>> rightMapper,
        Func<TLeft, Task<T>> leftMapper) => await (await either).Match(rightMapper, leftMapper);

    #endregion

    #region Map (right)

    /// <summary>
    /// Maps a function over the right value. Short-circuits on left.
    /// </summary>
    public static Either<TLeft, TResult> Map<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
        Func<TRight, TResult> mapper) =>
        either.MatchLeft(out var left)
            ? Either<TLeft, TResult>.Left(left)
            : Either<TLeft, TResult>.Right(mapper(either.Value!));

    /// <summary>
    /// Async Map: sync Either, async mapper.
    /// </summary>
    public static async Task<Either<TLeft, TResult>> Map<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
        Func<TRight, Task<TResult>> mapper) =>
        either.MatchLeft(out var left)
            ? Either<TLeft, TResult>.Left(left)
            : Either<TLeft, TResult>.Right(await mapper(either.Value!));

    /// <summary>
    /// Async Map: async Either, sync mapper.
    /// </summary>
    public static async Task<Either<TLeft, TResult>> Map<TLeft, TRight, TResult>(this Task<Either<TLeft, TRight>> either,
        Func<TRight, TResult> mapper) => (await either).Map(mapper);

    /// <summary>
    /// Async Map: async Either, async mapper.
    /// </summary>
    public static async Task<Either<TLeft, TResult>> Map<TLeft, TRight, TResult>(this Task<Either<TLeft, TRight>> either,
        Func<TRight, Task<TResult>> mapper) => await (await either).Map(mapper);

    #endregion

    #region Bind (right)

    /// <summary>
    /// Binds a function over the right value. Short-circuits on left.
    /// Use Bind when the mapper returns an Either.
    /// </summary>
    public static Either<TLeft, TResult> Bind<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
        Func<TRight, Either<TLeft, TResult>> mapper) =>
        either.MatchLeft(out var left)
            ? Either<TLeft, TResult>.Left(left)
            : mapper(either.Value!);

    /// <summary>
    /// Async Bind: sync Either, async mapper.
    /// </summary>
    public static async Task<Either<TLeft, TResult>> Bind<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
        Func<TRight, Task<Either<TLeft, TResult>>> mapper) =>
        either.MatchLeft(out var left)
            ? Either<TLeft, TResult>.Left(left)
            : await mapper(either.Value!);

    /// <summary>
    /// Async Bind: async Either, sync mapper.
    /// </summary>
    public static async Task<Either<TLeft, TResult>> Bind<TLeft, TRight, TResult>(this Task<Either<TLeft, TRight>> either,
        Func<TRight, Either<TLeft, TResult>> mapper) => (await either).Bind(mapper);

    /// <summary>
    /// Async Bind: async Either, async mapper.
    /// </summary>
    public static async Task<Either<TLeft, TResult>> Bind<TLeft, TRight, TResult>(this Task<Either<TLeft, TRight>> either,
        Func<TRight, Task<Either<TLeft, TResult>>> mapper) => await (await either).Bind(mapper);

    #endregion

    #region MapLeft

    /// <summary>
    /// Maps a function over the left value. Short-circuits on right.
    /// </summary>
    public static Either<TResult, TRight> MapLeft<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
        Func<TLeft, TResult> mapper) =>
        either.MatchLeft(out var left)
            ? Either<TResult, TRight>.Left(mapper(left))
            : Either<TResult, TRight>.Right(either.Value!);

    /// <summary>
    /// Async MapLeft: async Either, sync mapper.
    /// </summary>
    public static async Task<Either<TResult, TRight>> MapLeft<TLeft, TRight, TResult>(this Task<Either<TLeft, TRight>> either,
        Func<TLeft, TResult> mapper) => (await either).MapLeft(mapper);

    #endregion

    #region BindLeft

    /// <summary>
    /// Binds a function over the left value. Short-circuits on right.
    /// </summary>
    public static Either<TResult, TRight> BindLeft<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
        Func<TLeft, Either<TResult, TRight>> mapper) =>
        either.MatchLeft(out var left)
            ? mapper(left)
            : Either<TResult, TRight>.Right(either.Value!);

    /// <summary>
    /// Async BindLeft: async Either, sync mapper.
    /// </summary>
    public static async Task<Either<TResult, TRight>> BindLeft<TLeft, TRight, TResult>(this Task<Either<TLeft, TRight>> either,
        Func<TLeft, Either<TResult, TRight>> mapper) => (await either).BindLeft(mapper);

    #endregion

    #region Do

    /// <summary>
    /// Executes an action on the right value without modifying the Either.
    /// </summary>
    public static Either<TLeft, TRight> Do<TLeft, TRight>(this Either<TLeft, TRight> either, Action<TRight> action)
    {
        if (either.MatchRight(out var right)) action(right);
        return either;
    }

    /// <summary>
    /// Async Do: sync Either, async action.
    /// </summary>
    public static async Task<Either<TLeft, TRight>> Do<TLeft, TRight>(this Either<TLeft, TRight> either,
        Func<TRight, Task> action)
    {
        if (either.MatchRight(out var right)) await action(right);
        return either;
    }

    /// <summary>
    /// Async Do: async Either, sync action.
    /// </summary>
    public static async Task<Either<TLeft, TRight>> Do<TLeft, TRight>(this Task<Either<TLeft, TRight>> either,
        Action<TRight> action) => (await either).Do(action);

    /// <summary>
    /// Async Do: async Either, async action.
    /// </summary>
    public static async Task<Either<TLeft, TRight>> Do<TLeft, TRight>(this Task<Either<TLeft, TRight>> either,
        Func<TRight, Task> action) => await (await either).Do(action);

    #endregion

    #region Support async query syntax

    public static async Task<Either<TLeft, TResult>> Select<TLeft, TRight, TResult>(
        this Either<TLeft, TRight> either, Func<TRight, Task<TResult>> mapper) =>
        await either.Map(mapper);

    public static async Task<Either<TLeft, TResult>> Select<TLeft, TRight, TResult>(
        this Task<Either<TLeft, TRight>> either, Func<TRight, TResult> mapper) =>
        await either.Map(mapper);

    public static async Task<Either<TLeft, TResult>> Select<TLeft, TRight, TResult>(
        this Task<Either<TLeft, TRight>> either, Func<TRight, Task<TResult>> mapper) =>
        await either.Map(mapper);

    public static async Task<Either<TLeft, TResult>> SelectMany<TLeft, TRight, TIntermediate, TResult>(
        this Task<Either<TLeft, TRight>> either,
        Func<TRight, Task<Either<TLeft, TIntermediate>>> binder, Func<TRight, TIntermediate, TResult> mapper) =>
        await either.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    public static async Task<Either<TLeft, TResult>> SelectMany<TLeft, TRight, TIntermediate, TResult>(
        this Task<Either<TLeft, TRight>> either,
        Func<TRight, Either<TLeft, TIntermediate>> binder, Func<TRight, TIntermediate, TResult> mapper) =>
        await either.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    public static async Task<Either<TLeft, TResult>> SelectMany<TLeft, TRight, TIntermediate, TResult>(
        this Task<Either<TLeft, TRight>> either,
        Func<TRight, Task<Either<TLeft, TIntermediate>>> binder, Func<TRight, TIntermediate, Task<TResult>> mapper) =>
        await either.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    public static async Task<Either<TLeft, TResult>> SelectMany<TLeft, TRight, TIntermediate, TResult>(
        this Task<Either<TLeft, TRight>> either,
        Func<TRight, Task<Either<TLeft, TIntermediate>>> binder, Func<TRight, TIntermediate, Task<Either<TLeft, TResult>>> mapper) =>
        await either.Bind(r => binder(r).Bind(sub => mapper(r, sub)));

    #endregion
}

/// <summary>
/// A sum type representing a value of one of two possible types.
/// Left is conventionally the "error" or "alternative" track; Right is the "success" or "happy" track.
/// </summary>
/// <typeparam name="TLeft">The left (alternative) type</typeparam>
/// <typeparam name="TRight">The right (happy path) type</typeparam>
public readonly struct Either<TLeft, TRight>
{
    /// <summary>
    /// Explicit tag indicating whether this Either is in the left state.
    /// Using a tag field instead of null-checking avoids fragile assumptions
    /// about reference-type defaults and value-type defaults.
    /// For a default-initialized struct, this will be false (right track).
    /// </summary>
    private readonly bool _isLeft;

    /// <summary>
    /// The right (success) value.
    /// </summary>
    public TRight? Value { get; }

    /// <summary>
    /// The left (alternative) value.
    /// </summary>
    public TLeft? LValue { get; }

    private Either(TLeft left, bool _)
    {
        _isLeft = true;
        LValue = left;
        Value = default;
    }

    private Either(TRight right)
    {
        _isLeft = false;
        Value = right;
        LValue = default;
    }

    public static Either<TLeft, TRight> Left(TLeft left) => new(left, true);
    public static Either<TLeft, TRight> Right(TRight right) => new(right);

    /// <summary>
    /// Determines if this Either is in the left state.
    /// </summary>
    /// <param name="left">out variable that will be assigned the left value</param>
    /// <returns>true if this Either is left, false otherwise</returns>
    public bool MatchLeft(out TLeft left)
    {
        left = LValue!;
        return _isLeft;
    }

    /// <summary>
    /// Determines if this Either is in the right state.
    /// </summary>
    /// <param name="right">out variable that will be assigned the right value</param>
    /// <returns>true if this Either is right, false otherwise</returns>
    public bool MatchRight(out TRight right)
    {
        right = Value!;
        return !_isLeft;
    }

    public static implicit operator Either<TLeft, TRight>(TLeft left) => Left(left);
    public static implicit operator Either<TLeft, TRight>(TRight right) => Right(right);

    #region Support query syntax

    public Either<TLeft, TResult> Select<TResult>(Func<TRight, TResult> mapper) => this.Map(mapper);
    public async Task<Either<TLeft, TResult>> Select<TResult>(Func<TRight, Task<TResult>> mapper) => await this.Map(mapper);

    // supports Either from clauses and normal select clause
    public Either<TLeft, TResult> SelectMany<TIntermediate, TResult>(
        Func<TRight, Either<TLeft, TIntermediate>> binder, Func<TRight, TIntermediate, TResult> mapper) =>
        this.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    // supports Either from clauses and Either select clause
    public Either<TLeft, TResult> SelectMany<TIntermediate, TResult>(
        Func<TRight, Either<TLeft, TIntermediate>> binder, Func<TRight, TIntermediate, Either<TLeft, TResult>> mapper) =>
        this.Bind(r => binder(r).Bind(sub => mapper(r, sub)));

    // supports async Either from clause and sync select clause
    public async Task<Either<TLeft, TResult>> SelectMany<TIntermediate, TResult>(
        Func<TRight, Task<Either<TLeft, TIntermediate>>> binder, Func<TRight, TIntermediate, TResult> mapper) =>
        await this.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    // support sync Either from and async select
    public async Task<Either<TLeft, TResult>> SelectMany<TIntermediate, TResult>(
        Func<TRight, Either<TLeft, TIntermediate>> binder, Func<TRight, TIntermediate, Task<TResult>> mapper) =>
        await this.Bind(r => binder(r).Map(sub => mapper(r, sub)));

    // support sync Either from and async select Either
    public async Task<Either<TLeft, TResult>> SelectMany<TIntermediate, TResult>(
        Func<TRight, Either<TLeft, TIntermediate>> binder, Func<TRight, TIntermediate, Task<Either<TLeft, TResult>>> mapper) =>
        await this.Bind(r => binder(r).Bind(sub => mapper(r, sub)));

    #endregion
}
