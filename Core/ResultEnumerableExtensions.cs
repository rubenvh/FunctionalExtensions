namespace FluentSolidity.FunctionalExtensions;

public static class ResultEnumerableExtensions
{
    /// <summary>
    /// Takes an IEnumerable of Results and returns a Result of IEnumerable
    /// </summary>
    /// <param name="results">The list of Result instances</param>
    /// <param name="errorIdentifier">The error id to use when more than 1 of the list of Result instances is in error state</param>
    /// <param name="errorMessage">The error message to use when more than 1 of the list of Result instances is in error state</param>
    /// <typeparam name="T">type of the entity inside the Result container</typeparam>
    /// <returns>an error result when any of the result instances is in error state; a successful result when all of the result instances are in a success state.</returns>
    public static Result<IEnumerable<T>> FlattenResults<T>( this IEnumerable<Result<T>> results, string errorIdentifier = "AggregatedError", string? errorMessage = null)
    {
        var (prepared, pipelineMessages, errors) = ExtractResults(results);
        if (!errors.Any()) return (prepared, pipelineMessages);
        if (errors.Length == 1) return new Result<IEnumerable<T>>(errors.Single(), pipelineMessages.ToArray());

        var errorResult = Error.Create(errorIdentifier, errorMessage ?? "Multiple errors found, more information in the pipeline messages.");

        return new Result<IEnumerable<T>>(errorResult, pipelineMessages.ToArray());
    }

    /// <summary>
    /// Takes an IEnumerable of Results of an IEnumerable and returns a Result of IEnumerable
    /// </summary>
    /// <param name="results">The list of Result instances containing IEnumerables</param>
    /// <param name="errorIdentifier">The error id to use when more than 1 of the list of Result instances is in error state</param>
    /// <param name="messagePrefix">A message prefix to use when more than 1 of the list of Result instances is in error state</param>
    /// <typeparam name="T">type of the entity inside the Result container</typeparam>
    /// <returns>an error result when any of the result instances is in error state; a successful result when all of the result instances are in a success state.</returns>
    public static Result<IEnumerable<T>> FlattenManyResults<T>( this IEnumerable<Result<IEnumerable<T>>> results, string errorIdentifier = "AggregatedError", string messagePrefix = "") => 
        results.FlattenResults( errorIdentifier, messagePrefix ).Map(x => x.SelectMany(y => y  ));
    
    /// <summary>
    /// Takes an IEnumerable of Results and returns a Result of IEnumerable where any error will be added as a pipeline message
    /// Note that if all results are in error state, the result will be an empty collection with the errors as pipeline messages
    /// </summary>
    /// <param name="results">The list of Result instances</param>
    /// <typeparam name="T">type of the entity inside the Result container</typeparam>
    /// <returns>always a successful result</returns>
    public static Result<IEnumerable<T>> FlattenValues<T>( this IEnumerable<Result<T>> results)
    {
        var (successes, messages, _) = ExtractResults(results);

        return new Result<IEnumerable<T>>(successes, messages);
    }
    
    private static (List<T> successes, PipelineMessage[] messages, Error[] errors) ExtractResults<T>(IEnumerable<Result<T>> results)
    {
        var pipelineMessages = new Dictionary<string, PipelineMessage>();
        var successes = new List<T>();
        var errors = new List<Error>();
        foreach( var result in results )
        {
            foreach (var m in result.Messages) AddMessage(m);
            result.Match( r =>
                {
                    successes.Add(r );
                    return true;
                },
                error =>
                {
                    errors.Add(error);
                    AddMessage(error.ToPipelineMessage());
                    return false;
                });
        }
        return (successes, pipelineMessages.Values.ToArray(), errors.ToArray());

        void AddMessage(PipelineMessage m)
        {
            var key = $"{m.Level}{m.Id}{m.Context}{m.Message}";
            if (!pipelineMessages.ContainsKey(key))
            {
                pipelineMessages[key] = m;
            }
        }
    }
}