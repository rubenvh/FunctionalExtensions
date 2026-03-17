namespace FluentSolidity.FunctionalExtensions;

public static class Result
{
    /// <summary>
    /// Creates a new successful Result using a given value 
    /// </summary>
    /// <param name="value">successful value</param>
    /// <param name="messages">an optional collection of pipeline messages</param>
    /// <typeparam name="T">type of value</typeparam>
    /// <returns></returns>
    public static Result<T> New<T>(T value, params PipelineMessage[] messages) => new(value, messages);

    /// <summary>
    /// Creates a new successful Result with a pre-built message list (avoids array copy).
    /// </summary>
    internal static Result<T> New<T>(T value, IReadOnlyList<PipelineMessage> messages) => new(value, messages);

    /// <summary>
    /// Creates an erroneous Result using a given Error object
    /// </summary>
    /// <param name="error">the error details</param>
    /// <param name="messages">an optional collection of pipeline messages</param>
    /// <typeparam name="T">would be type of the successful value</typeparam>
    /// <returns></returns>
    public static Result<T> Error<T>(Error error, params PipelineMessage[] messages) => new(error, messages);

    /// <summary>
    /// Collapses a Result that can be successful or erroneous to a single value by providing a mapper lambda for
    /// both scenario's.
    ///
    /// Result{T} -> Func{T, R} -> Func{Error, R} -> R
    /// </summary>
    /// <param name="result">The result to collapse</param>
    /// <param name="valueMapper">A lambda to invoke on successful values</param>
    /// <param name="errorMapper">A lambda to invoke on erroneous values</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <typeparam name="TResult">type into which this Result will be collapsed</typeparam>
    /// <returns>A single value representing the collapsed Result</returns>
    public static TResult Match<T, TResult>(this Result<T> result,
        Func<T, TResult> valueMapper,
        Func<Error, TResult> errorMapper) =>
        result.MatchError(out var left) ? errorMapper(left) : valueMapper(result.Value!);

    /// <summary>
    /// Asynchronous version of <see cref="Match{T,TResult}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{T,TResult},System.Func{FluentSolidity.FunctionalExtensions.Error,TResult})"/>
    /// This async version supports passing asynchronous mapper functions into a synchronous Result.
    /// </summary>
    /// <param name="result">The result to collapse</param>
    /// <param name="valueMapper">A lambda to invoke on successful values returning async result</param>
    /// <param name="errorMapper">A lambda to invoke on erroneous values returning async result</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <typeparam name="TResult">type into which this Result will be collapsed</typeparam>
    /// <returns>The promise of a single value representing the collapsed Result</returns>
    public static async Task<TResult> Match<T, TResult>(this Result<T> result,
        Func<T, Task<TResult>> valueMapper,
        Func<Error, Task<TResult>> errorMapper) =>
        result.MatchError(out var left) ? await errorMapper(left) : await valueMapper(result.Value!);

    /// <summary>
    /// Asynchronous version of <see cref="Match{T,TResult}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{T,TResult},System.Func{FluentSolidity.FunctionalExtensions.Error,TResult})"/>
    /// This async version supports passing synchronous mapper functions into an asynchronous Result.
    /// </summary>
    /// <param name="result">The promise of a result that needs collapsing</param>
    /// <param name="valueMapper">A lambda to invoke on successful values</param>
    /// <param name="errorMapper">A lambda to invoke on erroneous values</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <typeparam name="TResult">type into which this Result will be collapsed</typeparam>
    /// <returns>The promise of a single value representing the collapsed Result</returns>
    public static async Task<TResult> Match<T, TResult>(this Task<Result<T>> result,
        Func<T, TResult> valueMapper,
        Func<Error, TResult> errorMapper) => (await result).Match(valueMapper, errorMapper);

    /// <summary>
    /// Asynchronous version of <see cref="Match{T,TResult}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{T,TResult},System.Func{FluentSolidity.FunctionalExtensions.Error,TResult})"/>
    /// This async version supports passing asynchronous mapper functions into an asynchronous Result.
    /// </summary>
    /// <param name="result">The promise of a result that needs collapsing</param>
    /// <param name="valueMapper">A lambda to invoke on successful values returning async result</param>
    /// <param name="errorMapper">A lambda to invoke on erroneous values returning async result</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <typeparam name="TResult">type into which this Result will be collapsed</typeparam>
    /// <returns>The promise of a single value representing the collapsed Result</returns>
    public static async Task<TResult> Match<T, TResult>(this Task<Result<T>> result,
        Func<T, Task<TResult>> valueMapper,
        Func<Error, Task<TResult>> errorMapper) => await (await result).Match(valueMapper, errorMapper);

    /// <summary>
    /// Map a function over a Result. Note that this function won't do anything on an erroneous Result.
    /// Similar to IEnumerable's Select
    /// </summary>
    /// <param name="result">the result being mapped</param>
    /// <param name="mapper">lambda to map the result's value to another value</param>
    /// <typeparam name="T">the type of the successful value</typeparam>
    /// <typeparam name="TResult">the type of the transformed successful value</typeparam>
    /// <returns>a mapped result</returns>
    public static Result<TResult> Map<T, TResult>(this Result<T> result, Func<T, TResult> mapper) =>
        result.MatchError(out var error)
            ? new Result<TResult>(error, result.Messages)
            : New(mapper(result.Value!), result.Messages);

    /// <summary>
    /// Asynchronous version of <see cref="Map{T,TResult}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{T,TResult})"/>
    /// This async version supports passing asynchronous mapper functions into a synchronous Result.
    /// </summary>
    /// <param name="result">The result being mapped over</param>
    /// <param name="mapper">A lambda to invoke on successful values returning async result</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <typeparam name="TResult">type into which this Result will be mapped</typeparam>
    /// <returns>The promise of a mapped <see cref="Result{TResult}"/></returns>
    public static async Task<Result<TResult>> Map<T, TResult>(this Result<T> result,
        Func<T, Task<TResult>> mapper) =>
        result.MatchError(out var error)
            ? new Result<TResult>(error, result.Messages)
            : new Result<TResult>(await mapper(result.Value!), result.Messages);

    /// <summary>
    /// Asynchronous version of <see cref="Map{T,TResult}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{T,TResult})"/>
    /// This async version supports passing synchronous mapper functions into an asynchronous Result.
    /// </summary>
    /// <param name="result">The result being mapped over</param>
    /// <param name="mapper">A lambda to invoke on successful values</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <typeparam name="TResult">type into which this Result will be mapped</typeparam>
    /// <returns>The promise of a mapped <see cref="Result{TResult}"/></returns>
    public static async Task<Result<TResult>> Map<T, TResult>(this Task<Result<T>> result,
        Func<T, TResult> mapper) => (await result).Map(mapper);

    /// <summary>
    /// Map a function over a Result. Note that this function won't do anything on an erroneous Result.
    /// Similar to IEnumerable's Select
    /// </summary>
    /// <param name="result">the result being mapped</param>
    /// <param name="mapper">async lambda to map the result's value to another value</param>
    /// <typeparam name="T">the type of the successful value</typeparam>
    /// <typeparam name="TResult">the type of the transformed successful value</typeparam>
    /// <returns>a mapped result</returns>
    [Obsolete(
        "You are passing an async mapper that does not return anything: this is not allowed, please use the .Do extension methods for this.",
        true)]
    public static async Task<Result<bool>> Map<T>(this Task<Result<T>> _, Func<T, Task> __) =>
        throw new NotSupportedException(
            "You are passing an async mapper that does not return anything: this is not allowed, please use the .Do extension methods for this.");


    /// <summary>
    /// Asynchronous version of <see cref="Map{T,TResult}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{T,TResult})"/>
    /// This async version supports passing asynchronous mapper functions into an asynchronous Result.
    /// </summary>
    /// <param name="result">The promise of a result being mapped over</param>
    /// <param name="mapper">A lambda to invoke on successful values returning async result</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <typeparam name="TResult">type into which this Result will be mapped</typeparam>
    /// <returns>The promise of a mapped <see cref="Result{TResult}"/></returns>
    public static async Task<Result<TResult>> Map<T, TResult>(this Task<Result<T>> result,
        Func<T, Task<TResult>> mapper) => await (await result).Map(mapper);


    /// <summary>
    /// Executes a function over a Result. Note that the function won't be executed on an erroneous Result.
    /// Returns the unmodified result back to the caller.
    /// </summary>
    /// <param name="result">the result on which to execute</param>
    /// <param name="action">action that takes the result's successful value</param>
    /// <typeparam name="T">the type of the successful value</typeparam>
    /// <returns>the unmodified result</returns>
    public static Result<T> Do<T>(this Result<T> result, Action<T> action)
    {
        if (result.MatchSuccess(out var success)) action(success);
        return result;
    }

    /// <summary>
    /// Asynchronous version of <see cref="Do{T}(FluentSolidity.FunctionalExtensions.Result{T},System.Action{T})"/>
    /// This async version supports passing asynchronous callback functions into a synchronous Result.
    /// </summary>
    /// <param name="result">the result on which to execute</param>
    /// <param name="action">async action that takes the result's successful value</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <returns>a promise of <see cref="Result{T}"/></returns>
    public static async Task<Result<T>> Do<T>(this Result<T> result,
        Func<T, Task> action)
    {
        if (result.MatchSuccess(out var success)) await action(success);
        return result;
    }

    /// <summary>
    /// Asynchronous version of <see cref="Do{T}(FluentSolidity.FunctionalExtensions.Result{T},System.Action{T})"/>
    /// This async version supports passing synchronous callback functions into an asynchronous Result.
    /// </summary>
    /// <param name="result">the result on which to execute</param>
    /// <param name="action">async action that takes the result's successful value</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <returns>a promise of <see cref="Result{T}"/></returns>
    public static async Task<Result<T>> Do<T>(this Task<Result<T>> result,
        Action<T> action) => (await result).Do(action);

    /// <summary>
    /// Asynchronous version of <see cref="Do{T}(FluentSolidity.FunctionalExtensions.Result{T},System.Action{T})"/>
    /// This async version supports passing asynchronous callback functions into an asynchronous Result.
    /// </summary>
    /// <param name="result">the result on which to execute</param>
    /// <param name="action">async action that takes the result's successful value</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <returns>a promise of <see cref="Result{T}"/></returns>
    public static async Task<Result<T>> Do<T>(this Task<Result<T>> result,
        Func<T, Task> action) => await (await result).Do(action);

    /// <summary>
    /// Executes an action on the error value without modifying the Result.
    /// Use this for side-effects on the error track (e.g., logging).
    /// </summary>
    /// <param name="result">the result on which to execute</param>
    /// <param name="action">action that takes the result's error value</param>
    /// <typeparam name="T">the type of the successful value</typeparam>
    /// <returns>the unmodified result</returns>
    public static Result<T> DoError<T>(this Result<T> result, Action<Error> action)
    {
        if (result.MatchError(out var error)) action(error);
        return result;
    }

    /// <summary>
    /// Async DoError: sync Result, async action.
    /// </summary>
    public static async Task<Result<T>> DoError<T>(this Result<T> result,
        Func<Error, Task> action)
    {
        if (result.MatchError(out var error)) await action(error);
        return result;
    }

    /// <summary>
    /// Async DoError: async Result, sync action.
    /// </summary>
    public static async Task<Result<T>> DoError<T>(this Task<Result<T>> result,
        Action<Error> action) => (await result).DoError(action);

    /// <summary>
    /// Async DoError: async Result, async action.
    /// </summary>
    public static async Task<Result<T>> DoError<T>(this Task<Result<T>> result,
        Func<Error, Task> action) => await (await result).DoError(action);

    /// <summary>
    /// Conditionally executes an action on a successful Result's value.
    /// Short-circuits on error. If the Result is successful and the predicate returns true,
    /// the action is executed. Returns the unmodified result in all cases.
    /// </summary>
    /// <param name="result">the result on which to execute</param>
    /// <param name="predicate">predicate to evaluate on the successful value</param>
    /// <param name="action">action to execute when the predicate is true</param>
    /// <typeparam name="T">the type of the successful value</typeparam>
    /// <returns>the unmodified result</returns>
    public static Result<T> DoWhen<T>(this Result<T> result, Func<T, bool> predicate, Action<T> action)
    {
        if (result.MatchSuccess(out var success) && predicate(success)) action(success);
        return result;
    }

    /// <summary>
    /// Async DoWhen: sync Result, async action.
    /// </summary>
    public static async Task<Result<T>> DoWhen<T>(this Result<T> result,
        Func<T, bool> predicate, Func<T, Task> action)
    {
        if (result.MatchSuccess(out var success) && predicate(success)) await action(success);
        return result;
    }

    /// <summary>
    /// Async DoWhen: async Result, sync action.
    /// </summary>
    public static async Task<Result<T>> DoWhen<T>(this Task<Result<T>> result,
        Func<T, bool> predicate, Action<T> action) => (await result).DoWhen(predicate, action);

    /// <summary>
    /// Async DoWhen: async Result, async action.
    /// </summary>
    public static async Task<Result<T>> DoWhen<T>(this Task<Result<T>> result,
        Func<T, bool> predicate, Func<T, Task> action) => await (await result).DoWhen(predicate, action);

    /// <summary>
    /// Binds a function over a Result. Note that this function won't do anything on an erroneous Result.
    /// The difference with <see cref="Map{T,TResult}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{T,TResult})"/>
    /// defined above, is that Bind should be used with mapping functions returning <see cref="Result{T}"/>
    /// instances instead of values directly.
    ///
    /// Similar to IEnumerable's SelectMany
    /// </summary>
    /// <param name="result">the result being bound</param>
    /// <param name="mapper">lambda to map the result's value to another Result</param>
    /// <typeparam name="T">the type of the successful value</typeparam>
    /// <typeparam name="TResult">the type of the transformed successful value</typeparam>
    /// <returns>a mapped result</returns>
    public static Result<TResult> Bind<T, TResult>(this Result<T> result, Func<T, Result<TResult>> mapper) =>
        result.MatchError(out var error)
            ? new Result<TResult>(error, result.Messages)
            : mapper(result.Value!).WithMessages(result.Messages);

    /// <summary>
    /// Asynchronous version of <see cref="Bind{T,TResult}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{T,FluentSolidity.FunctionalExtensions.Result{TResult}})"/>
    /// This async version supports passing asynchronous mapper functions into a synchronous Result.
    /// </summary>
    /// <param name="result">The result being mapped over</param>
    /// <param name="mapper">A lambda to invoke on successful values returning async Result objects</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <typeparam name="TResult">type into which this Result will be mapped</typeparam>
    /// <returns>The promise of a mapped <see cref="Result{TResult}"/></returns>
    public static async Task<Result<TResult>> Bind<T, TResult>(this Result<T> result,
        Func<T, Task<Result<TResult>>> mapper) =>
        result.MatchError(out var error)
            ? new Result<TResult>(error, result.Messages)
            : (await mapper(result.Value!)).WithMessages(result.Messages);

    /// <summary>
    /// Asynchronous version of <see cref="Bind{T,TResult}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{T,FluentSolidity.FunctionalExtensions.Result{TResult}})"/>
    /// This async version supports passing synchronous mapper functions into an asynchronous Result.
    /// </summary>
    /// <param name="result">The result being mapped over</param>
    /// <param name="mapper">A lambda to invoke on successful values</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <typeparam name="TResult">type into which this Result will be mapped</typeparam>
    /// <returns>The promise of a mapped <see cref="Result{TResult}"/></returns>
    public static async Task<Result<TResult>> Bind<T, TResult>(this Task<Result<T>> result,
        Func<T, Result<TResult>> mapper) => (await result).Bind(mapper);
 
 
    /// <summary>
    /// Asynchronous version of <see cref="Bind{T,TResult}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{T,FluentSolidity.FunctionalExtensions.Result{TResult}})"/>
    /// This async version supports passing asynchronous mapper functions into an asynchronous Result.
    /// </summary>
    /// <param name="result">The promise of a result being mapped over</param>
    /// <param name="mapper">A lambda to invoke on successful values returning async result</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <typeparam name="TResult">type into which this Result will be mapped</typeparam>
    /// <returns>The promise of a mapped <see cref="Result{TResult}"/></returns>
    public static async Task<Result<TResult>> Bind<T, TResult>(this Task<Result<T>> result,
        Func<T, Task<Result<TResult>>> mapper) => await (await result).Bind(mapper);

    /// <summary>
    /// Map a function over a Result's error. Note that this function won't do anything on a successful Result.
    /// </summary>
    /// <param name="result">the result being mapped</param>
    /// <param name="errorMapper">lambda to map the result's error to another error</param>
    /// <typeparam name="T">the type of the successful value</typeparam>
    /// <returns>either an unmodified result or a result containing a mapped error</returns>
    public static Result<T> MapError<T>(this Result<T> result, Func<Error, Error> errorMapper) =>
        result.MatchError(out var error)
            ? new Result<T>(errorMapper(error), result.Messages)
            : result;

    /// <summary>
    /// Asynchronous version of <see cref="MapError{T}(FluentSolidity.FunctionalExtensions.Result{T},System.Func{Error,Error})"/>
    /// This async version supports passing synchronous mapper functions into an asynchronous Result.
    /// </summary>
    /// <param name="result">the result being mapped</param>
    /// <param name="errorMapper">lambda to map the result's error to another error</param>
    /// <typeparam name="T">type of successful value</typeparam>
    /// <returns>The promise of a mapped <see cref="Result{T}"/></returns>
    public static async Task<Result<T>> MapError<T>(this Task<Result<T>> result,
        Func<Error, Error> errorMapper) => (await result).MapError(errorMapper);
    
    
    /// <summary>
    /// Guards a successful Result by testing its value against a predicate.
    /// If the predicate returns true, the Result passes through unchanged.
    /// If the predicate returns false, the Result is flipped to an error using the provided error factory.
    /// Short-circuits on an already erroneous Result.
    /// </summary>
    /// <param name="result">the result to guard</param>
    /// <param name="predicate">a predicate that must hold for the value</param>
    /// <param name="errorFactory">a factory producing an Error when the predicate fails</param>
    /// <typeparam name="T">type of the successful value</typeparam>
    /// <returns>the original result if the predicate holds, otherwise an error result</returns>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Func<T, Error> errorFactory) =>
        result.MatchSuccess(out var value)
            ? predicate(value) ? result : new Result<T>(errorFactory(value), result.Messages)
            : result;

    /// <summary>
    /// Async Ensure: sync Result, async predicate.
    /// </summary>
    public static async Task<Result<T>> Ensure<T>(this Result<T> result,
        Func<T, Task<bool>> predicate, Func<T, Error> errorFactory) =>
        result.MatchSuccess(out var value)
            ? await predicate(value) ? result : new Result<T>(errorFactory(value), result.Messages)
            : result;

    /// <summary>
    /// Async Ensure: async Result, sync predicate.
    /// </summary>
    public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> result,
        Func<T, bool> predicate, Func<T, Error> errorFactory) => (await result).Ensure(predicate, errorFactory);

    /// <summary>
    /// Async Ensure: async Result, async predicate.
    /// </summary>
    public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> result,
        Func<T, Task<bool>> predicate, Func<T, Error> errorFactory) => await (await result).Ensure(predicate, errorFactory);


    #region Support async query syntax

    public static async Task<Result<TResult>> Select<T, TResult>(this Result<T> result, Func<T, Task<TResult>> mapper) => await result.Map(mapper);
    public static async Task<Result<TResult>> Select<T, TResult>(this Task<Result<T>> result, Func<T, TResult> mapper) => await result.Map(mapper);
    public static async Task<Result<TResult>> Select<T, TResult>(this Task<Result<T>> result, Func<T, Task<TResult>> mapper) => await result.Map(mapper);
    
    public static async Task<Result<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Result<T>> result,
        Func<T, Task<Result<TIntermediate>>> binder, Func<T, TIntermediate, TResult> mapper) =>
        await result.Bind(r => binder(r).Map(sub => mapper(r, sub)));
    
    public static async Task<Result<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Result<T>> result,
        Func<T, Result<TIntermediate>> binder, Func<T, TIntermediate, TResult> mapper) =>
        await result.Bind(r => binder(r).Map(sub => mapper(r, sub)));
    
    public static async Task<Result<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Result<T>> result,
        Func<T, Task<Result<TIntermediate>>> binder, Func<T, TIntermediate, Task<TResult>> mapper) =>
        await result.Bind(r => binder(r).Map(sub => mapper(r, sub)));
    
    public static async Task<Result<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Result<T>> result,
        Func<T, Task<Result<TIntermediate>>> binder, Func<T, TIntermediate, Task<Result<TResult>>> mapper) =>
        await result.Bind(r => binder(r).Bind(sub => mapper(r, sub)));
    #endregion
}

/// <summary>
/// This struct is a specific version of Either where the left clause is always of type <see cref="Error"/>  
/// </summary>
/// <typeparam name="T">type of successful Result</typeparam>
public readonly struct Result<T>
{
    /// <summary>
    /// Explicit tag indicating whether this Result is in an error state.
    /// Using a tag field instead of null-checking Error avoids fragile assumptions
    /// about reference-type defaults and makes the discrimination unambiguous.
    /// For a default-initialized struct, this will be false (success track).
    /// </summary>
    private readonly bool _isError;

    /// <summary>
    /// Successful value
    /// </summary>
    public T? Value { get; } = default;

    /// <summary>
    /// Erroneous value
    /// </summary>
    public Error? Error { get; } = null;

    /// <summary>
    /// A collection of pipeline messages
    /// </summary>
    public IReadOnlyList<PipelineMessage> Messages { get; } = Array.Empty<PipelineMessage>();

    /// <summary>
    /// Determines if this result represents an erroneous value
    /// </summary>
    /// <param name="error">out variable that will be assigned the error object</param>
    /// <returns>true if this Result is erroneous, false otherwise</returns>
    public bool MatchError(out Error error)
    {
        error = Error!;
        return _isError;
    }

    /// <summary>
    /// Determines if this result represents a successful value
    /// </summary>
    /// <param name="value">out variable that will be assigned the successful value</param>
    /// <returns>true if this Result is successful, false otherwise</returns>
    public bool MatchSuccess(out T value)
    {
        value = Value!;
        return !_isError;
    }

    /// <summary>
    /// constructs a Result using a successful value 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="messages">an optional collection of pipeline messages</param>
    public Result(T value, params PipelineMessage[] messages)
    {
        _isError = false;
        Value = value;
        Messages = messages ?? Array.Empty<PipelineMessage>();
    }

    /// <summary>
    /// Constructs a Result using an erroneous value
    /// </summary>
    /// <param name="error"></param>
    /// <param name="messages">an optional collection of pipeline messages</param>
    public Result(Error error, params PipelineMessage[] messages)
    {
        _isError = true;
        Error = error;
        Messages = messages ?? Array.Empty<PipelineMessage>();
    }

    /// <summary>
    /// Constructs a successful Result using a pre-built message list (avoids array copy).
    /// </summary>
    internal Result(T value, IReadOnlyList<PipelineMessage> messages)
    {
        _isError = false;
        Value = value;
        Messages = messages;
    }

    /// <summary>
    /// Constructs an error Result using a pre-built message list (avoids array copy).
    /// </summary>
    internal Result(Error error, IReadOnlyList<PipelineMessage> messages)
    {
        _isError = true;
        Error = error;
        Messages = messages;
    }

    /// <summary>
    /// Copy constructor for Result overriding the collection of pipeline messages 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="messages"></param>
    private Result(Result<T> value, IReadOnlyList<PipelineMessage> messages)
    {
        _isError = value._isError;
        Messages = messages;
        Value = value.Value;
        Error = value.Error;
    }
    
    /// <summary>
    /// Prepends a collection of pipeline messages to the current Result.
    /// Uses a single list allocation with pre-calculated capacity to avoid
    /// O(N²) array copying when messages accumulate through a pipeline chain.
    /// </summary>
    /// <param name="messages">messages to prepend</param>
    /// <returns>a new Result with the combined messages</returns>
    public Result<T> WithMessages(params PipelineMessage[] messages)
    {
        var current = Messages ?? Array.Empty<PipelineMessage>();
        if (messages is not { Length: > 0 }) return this;
        if (current.Count == 0) return new(this, messages);

        var combined = new List<PipelineMessage>(messages.Length + current.Count);
        combined.AddRange(messages);
        for (var i = 0; i < current.Count; i++)
            combined.Add(current[i]);
        return new(this, combined);
    }

    /// <summary>
    /// Prepends a read-only list of pipeline messages to the current Result.
    /// </summary>
    internal Result<T> WithMessages(IReadOnlyList<PipelineMessage> messages)
    {
        var current = Messages ?? Array.Empty<PipelineMessage>();
        if (messages is not { Count: > 0 }) return this;
        if (current.Count == 0) return new(this, messages);

        var combined = new List<PipelineMessage>(messages.Count + current.Count);
        for (var i = 0; i < messages.Count; i++)
            combined.Add(messages[i]);
        for (var i = 0; i < current.Count; i++)
            combined.Add(current[i]);
        return new(this, combined);
    }
  

    public override string ToString() =>
        _isError ? $"Error({Error!.ErrorIdentifier}: {Error.ErrorMessage})" : $"Success({Value})";

    public static implicit operator Result<T>(Error e) => (new Result<T>(e)).WithMessages(e.ToPipelineMessage());
    public static implicit operator Result<T>(T v) => new(v);
    public static implicit operator Result<T>((T v, PipelineMessage[] m) _) => new(_.v, _.m);
    public static implicit operator Result<T>((T v, List<PipelineMessage> m) _) => (_.v, _.m.ToArray());

    #region Support query syntax

    public Result<TResult> Select<TResult>(Func<T, TResult> mapper) => this.Map(mapper);
    public async Task<Result<TResult>> Select<TResult>(Func<T, Task<TResult>> mapper) => await this.Map(mapper);

    // supports Result<T> from clauses and normal select clause
    public Result<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Result<TIntermediate>> binder, Func<T, TIntermediate, TResult> mapper) =>
        this.Bind(r => binder(r).Map(sub => mapper(r, sub)));
    
    // supports Result<T> from clauses and Result<T> select clause
    public Result<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Result<TIntermediate>> binder, Func<T, TIntermediate, Result<TResult>> mapper) =>
        this.Bind(r => binder(r).Bind(sub => mapper(r, sub)));
    
    // supports async Result<T> from clause and sync select clause
    public async Task<Result<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Task<Result<TIntermediate>>> binder, Func<T, TIntermediate, TResult> mapper) =>
        await this.Bind(r => binder(r).Map(sub => mapper(r, sub)));
    
    //  support sync Result<T> from and async select
    public async Task<Result<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Result<TIntermediate>> binder, Func<T, TIntermediate, Task<TResult>> mapper) =>
        await this.Bind(r => binder(r).Map(sub => mapper(r, sub)));
    
    //  support sync Result<T> from and async select Result<T>
    public async Task<Result<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Result<TIntermediate>> binder, Func<T, TIntermediate, Task<Result<TResult>>> mapper) =>
        await this.Bind(r => binder(r).Bind(sub => mapper(r, sub)));
    #endregion
}