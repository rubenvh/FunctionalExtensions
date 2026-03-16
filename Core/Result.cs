namespace FluentSolidity.FunctionalExtensions;

public static class Result
{
    /// <summary>
    /// Creates a new successful Result using a given value 
    /// </summary>
    /// <param name="value">successful value</param>
    /// <param name="messages">an optional collection of validation messages</param>
    /// <typeparam name="T">type of value</typeparam>
    /// <returns></returns>
    public static Result<T> New<T>(T value, params ValidationMessage[] messages) => new(value, messages);

    /// <summary>
    /// Creates an erroneous Result using a given Error object
    /// </summary>
    /// <param name="error">the error details</param>
    /// <param name="messages">an optional collection of validation messages</param>
    /// <typeparam name="T">would be type of the successful value</typeparam>
    /// <returns></returns>
    public static Result<T> Error<T>(Error error, params ValidationMessage[] messages) => new(error, messages);

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
            ? new Result<TResult>(error, result.ValidationMessages)
            : New(mapper(result.Value!), result.ValidationMessages);

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
            ? new Result<TResult>(error, result.ValidationMessages)
            : new Result<TResult>(await mapper(result.Value!), result.ValidationMessages);

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
            ? new Result<TResult>(error, result.ValidationMessages)
            : mapper(result.Value!).WithValidationMessages(result.ValidationMessages);

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
            ? new Result<TResult>(error, result.ValidationMessages)
            : (await mapper(result.Value!)).WithValidationMessages(result.ValidationMessages);

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
            ? new Result<T>(errorMapper(error), result.ValidationMessages)
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
    /// A collection of validation messages
    /// </summary>
    public ValidationMessage[] ValidationMessages { get; } = Array.Empty<ValidationMessage>();

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
    /// <param name="messages">an optional collection of validation messages</param>
    public Result(T value, params ValidationMessage[] messages)
    {
        _isError = false;
        Value = value;
        ValidationMessages = messages ?? Array.Empty<ValidationMessage>();
    }

    /// <summary>
    /// Constructs a Result using an erroneous value
    /// </summary>
    /// <param name="error"></param>
    /// <param name="messages">an optional collection of validation messages</param>
    public Result(Error error, params ValidationMessage[] messages)
    {
        _isError = true;
        Error = error;
        ValidationMessages = messages ?? Array.Empty<ValidationMessage>();
    }

    /// <summary>
    /// Copy constructor for Result overriding the collection of validation messages 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="messages"></param>
    private Result(Result<T> value, params ValidationMessage[] messages)
    {
        _isError = value._isError;
        ValidationMessages = messages ?? Array.Empty<ValidationMessage>();
        Value = value.Value;
        Error = value.Error;
    }
    
    /// <summary>
    /// Prepends a collection of validation messages to the current Result
    /// </summary>
    /// <param name="messages"></param>
    /// <returns></returns>
    public Result<T> WithValidationMessages(params ValidationMessage[] messages) => new(this, (messages ?? Array.Empty<ValidationMessage>()).Concat(ValidationMessages ?? Array.Empty<ValidationMessage>()).ToArray());
  

    public override string ToString() =>
        _isError ? $"Error({Error!.ErrorIdentifier}: {Error.ErrorMessage})" : $"Success({Value})";

    public static implicit operator Result<T>(Error e) => (new Result<T>(e)).WithValidationMessages(e.ToValidationMessage());
    public static implicit operator Result<T>(T v) => new(v);
    public static implicit operator Result<T>((T v, ValidationMessage[] m) _) => new(_.v, _.m);
    public static implicit operator Result<T>((T v, List<ValidationMessage> m) _) => (_.v, _.m.ToArray());

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