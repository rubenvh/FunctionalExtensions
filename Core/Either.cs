namespace Core;

public static class Either
{
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft left) => Either<TLeft, TRight>.Left(left);
    
    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight right) => Either<TLeft, TRight>.Right(right);

    public static T Match<TLeft, TRight, T>(this Either<TLeft, TRight> either,
        Func<TRight, T> rightMapper,
        Func<TLeft, T> leftMapper) =>
        either.MatchLeft(out var left) ? leftMapper(left) :
        either.MatchRight(out var right) ? rightMapper(right) :
        throw new ArgumentException("Either matches neither left nor right", nameof(either));
    public static Either<TLeft, TResult> Map<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
        Func<TRight, TResult> mapper) =>
        either.MatchLeft(out var left)
            ? Either<TLeft, TResult>.Left(left)
            : Either<TLeft, TResult>.Right(mapper(either.Value!));

    public static Either<TLeft, TResult> Bind<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
        Func<TRight, Either<TLeft, TResult>> mapper) =>
        either.MatchLeft(out var left)
            ? Either<TLeft, TResult>.Left(left)
            : mapper(either.Value!);
    
    public static Either<TResult, TRight> MapLeft<TLeft, TRight, TResult>(this Either<TLeft, TRight> either, 
        Func<TLeft, TResult> mapper) =>
        either.MatchLeft(out var left)
            ? Either<TResult, TRight>.Left(mapper(left))
            : Either<TResult, TRight>.Right(either.Value!);

    public static Either<TResult, TRight> BindLeft<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
        Func<TLeft, Either<TResult, TRight>> mapper) =>
        either.MatchLeft(out var left)
            ? mapper(left)
            : Either<TResult, TRight>.Right(either.Value!);
    
    public static async Task<Either<TLeft, TResult>> MapAsync<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
        Func<TRight, Task<TResult>> mapper) =>
        either.MatchLeft(out var left)
            ? Either<TLeft, TResult>.Left(left)
            : Either<TLeft, TResult>.Right(await mapper(either.Value!));
    
    public static async Task<Either<TLeft, TResult>> MapAsync<TLeft, TRight, TResult>(this Task<Either<TLeft, TRight>> either,
        Func<TRight, TResult> mapper) => (await either).Map(mapper);
    
    public static async Task<Either<TLeft, TResult>> MapAsync<TLeft, TRight, TResult>(this Task<Either<TLeft, TRight>> either,
        Func<TRight, Task<TResult>> mapper) => await (await either).MapAsync(mapper);
}

public class Either<TLeft, TRight>
{
    public TRight? Value { get; }
    public TLeft? LValue { get; }

    private Either(TLeft left) => LValue = left;
    private Either(TRight right) => Value = right;
    
    public static Either<TLeft, TRight> Left(TLeft left) => new(left);
    public static Either<TLeft, TRight> Right(TRight right) => new(right);

    public bool MatchLeft(out TLeft left) => !((left = LValue)?.Equals(default(TLeft)) ?? true);
    public bool MatchRight(out TRight right)
    {
        right = Value!;
        return !MatchLeft(out _);
    }
}