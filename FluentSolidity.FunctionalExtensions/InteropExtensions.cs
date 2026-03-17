namespace FluentSolidity.FunctionalExtensions;

/// <summary>
/// Provides conversion extension methods between Maybe, Result, and Validation types.
/// </summary>
public static class InteropExtensions
{
    #region Maybe -> Result

    /// <summary>
    /// Converts a Maybe to a Result. Some becomes Success, None becomes Error using the provided error.
    /// </summary>
    public static Result<T> ToResult<T>(this Maybe<T> maybe, Error error) =>
        maybe.MatchSome(out var value) ? Result.New(value) : Result.Error<T>(error);

    /// <summary>
    /// Converts a Maybe to a Result. Some becomes Success, None becomes Error using the provided error factory.
    /// </summary>
    public static Result<T> ToResult<T>(this Maybe<T> maybe, Func<Error> errorFactory) =>
        maybe.MatchSome(out var value) ? Result.New(value) : Result.Error<T>(errorFactory());

    /// <summary>
    /// Async ToResult: async Maybe, sync error.
    /// </summary>
    public static async Task<Result<T>> ToResult<T>(this Task<Maybe<T>> maybe, Error error) =>
        (await maybe).ToResult(error);

    /// <summary>
    /// Async ToResult: async Maybe, sync error factory.
    /// </summary>
    public static async Task<Result<T>> ToResult<T>(this Task<Maybe<T>> maybe, Func<Error> errorFactory) =>
        (await maybe).ToResult(errorFactory);

    #endregion

    #region Result -> Maybe

    /// <summary>
    /// Converts a Result to a Maybe. Success becomes Some, Error becomes None.
    /// The error information is discarded.
    /// </summary>
    public static Maybe<T> ToMaybe<T>(this Result<T> result) =>
        result.MatchSuccess(out var value) ? new Maybe<T>(value) : Maybe<T>.None;

    /// <summary>
    /// Async ToMaybe: async Result.
    /// </summary>
    public static async Task<Maybe<T>> ToMaybe<T>(this Task<Result<T>> result) =>
        (await result).ToMaybe();

    #endregion

    #region Validation -> Result

    /// <summary>
    /// Converts a Validation to a Result. Valid becomes Success, Invalid becomes Error
    /// using the provided error mapper to collapse the error list into a single Error.
    /// </summary>
    public static Result<T> ToResult<TError, T>(this Validation<TError, T> validation,
        Func<IReadOnlyList<TError>, Error> errorMapper) =>
        validation.MatchErrors(out var errors)
            ? Result.Error<T>(errorMapper(errors))
            : Result.New(validation.Value!);

    /// <summary>
    /// Async ToResult: async Validation, sync error mapper.
    /// </summary>
    public static async Task<Result<T>> ToResult<TError, T>(this Task<Validation<TError, T>> validation,
        Func<IReadOnlyList<TError>, Error> errorMapper) =>
        (await validation).ToResult(errorMapper);

    #endregion

    #region Result -> Validation

    /// <summary>
    /// Converts a Result to a Validation. Success becomes Valid, Error becomes Invalid
    /// using the provided error mapper to convert the Error into TError.
    /// </summary>
    public static Validation<TError, T> ToValidation<TError, T>(this Result<T> result,
        Func<Error, TError> errorMapper) =>
        result.MatchError(out var error)
            ? Validation.Invalid<TError, T>(errorMapper(error))
            : Validation.Valid<TError, T>(result.Value!);

    /// <summary>
    /// Async ToValidation: async Result, sync error mapper.
    /// </summary>
    public static async Task<Validation<TError, T>> ToValidation<TError, T>(this Task<Result<T>> result,
        Func<Error, TError> errorMapper) =>
        (await result).ToValidation(errorMapper);

    #endregion

    #region Maybe -> Validation

    /// <summary>
    /// Converts a Maybe to a Validation. Some becomes Valid, None becomes Invalid
    /// using the provided error factory to produce a single TError.
    /// </summary>
    public static Validation<TError, T> ToValidation<TError, T>(this Maybe<T> maybe,
        Func<TError> errorFactory) =>
        maybe.MatchSome(out var value)
            ? Validation.Valid<TError, T>(value)
            : Validation.Invalid<TError, T>(errorFactory());

    /// <summary>
    /// Async ToValidation: async Maybe, sync error factory.
    /// </summary>
    public static async Task<Validation<TError, T>> ToValidation<TError, T>(this Task<Maybe<T>> maybe,
        Func<TError> errorFactory) =>
        (await maybe).ToValidation(errorFactory);

    #endregion
}
