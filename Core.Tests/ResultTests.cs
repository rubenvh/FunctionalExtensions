namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultTests
{
    private Error someError = null!;
    private bool wasAwaited = false;

    [SetUp]
    public void Setup()
    {
        someError = Error.Create("error", "message");
    }

    [Test]
    public void successful_result_matches_success()
    {
        Assert.That(Result.New("success").MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("success"));
    }

    [Test]
    public void successful_result_does_not_match_error()
    {
        Assert.That(Result.New("success").MatchError(out _), Is.False);
    }

    [Test]
    public void matching_successful_result_executes_success_lambda()
    {
        Assert.That(Result.New("success").Match(_ => true, _ => false), Is.True);
    }

    [Test]
    public void matching_error_result_executes_error_lambda()
    {
        Assert.That(Result.Error<string>(someError).Match(_ => true, _ => false), Is.False);
    }

    [Test]
    public void error_result_matches_error()
    {
        Assert.That(Result.Error<string>(someError).MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo(someError.ErrorIdentifier));
        Assert.That(error.ErrorMessage, Is.EqualTo(someError.ErrorMessage));
    }

    [Test]
    public void error_result_does_not_match_success()
    {
        Assert.That(Result.Error<string>(someError).MatchSuccess(out _), Is.False);
    }

    [Test]
    public void mapping_successful_result_to_same_type()
    {
        Assert.That(Result.New("value").Map(v => v.ToUpper()).MatchSuccess(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo("VALUE"));
    }

    [Test]
    public void mapping_successful_result_to_new_type()
    {
        Assert.That(Result.New("value").Map(v => v.Length).MatchSuccess(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo("value".Length));
    }

    [Test]
    public void mapping_error_result_shortcircuits_chain_at_start()
    {
        Assert.That(Result.Error<string>(someError)
            .Map(v =>
            {
                Assert.Fail("Should not execute this lambda");
                return v;
            })
            .MatchError(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo(someError));
    }

    [Test]
    public void mapping_error_result_shortcircuits_chain_at_middle()
    {
        Assert.That(Result.New("value")
            .Bind(v => Result.Error<string>(someError))
            .Map(v => v.ToUpper())
            .MatchError(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo(someError));
    }

    [Test]
    public async Task async_mapping_error_result()
    {
        var actual = await Result.Error<string>(someError).Map(v => Task.FromResult(v.ToUpper()))
            .Match(s => s.ToUpper(), e => e.ErrorMessage);
        Assert.That(actual, Is.EqualTo(someError.ErrorMessage));
    }

    [Test]
    public async Task async_mapping_successful_result()
    {
        var actual = await Result.New("value")
            .Map(v => Task.FromResult(v.ToUpper())) // starting async
            .Map(v => $"{v}_") // chaining async
            .Map(async v => await DoubleStringAsync(v)) // returning async in async chain
            .Match(s => s, e => e.ErrorMessage);
        Assert.That(actual, Is.EqualTo("VALUE_VALUE_"));
    }
    
    [Test]
    public async Task async_mapping_void_doer_successful_result()
    {
        var actual = await Result.New("value")
            .Map(v => Task.FromResult(v.ToUpper())) // starting async
            .Map(v => $"{v}_") // chaining async
            .Do(_ => ReturnsNothingAsync())
            // returning async in async chain
            .Match(s => s, e => e.ErrorMessage);
        Assert.That(wasAwaited);
        Assert.That(actual, Is.EqualTo("VALUE_"));
    }

    [Test]
    public async Task async_binding_successful_result()
    {
        var actual = await Result.New("value")
            .Bind(v => Task.FromResult(Result.New(v.ToUpper()))) // starting async
            .Bind(v => Result.New($"{v}_")) // chaining async
            .Bind(async v => await DoubleStringResultAsync(v)) // returning async in async chain
            .Match(s => s, e => e.ErrorMessage);
        Assert.That(actual, Is.EqualTo("VALUE_VALUE_"));
    }
    
    [Test]
    public async Task async_binding_starting_from_task_result()
    {
        wasAwaited = false;
        var actual = await Task.FromResult(Result.New("value"))
            .Bind(v => Task.FromResult(Result.New(v.ToUpper()))) // starting from Task<Result<T>>
            .Bind(v => Task.FromResult(Result.New(v.ToUpper()))) // chaining async binds
            .Bind(v => Task.FromResult(Result.New($"{v}_")))
            .Bind(DoubleStringResultAsync) // method group syntax
            .Do(_ => ReturnsNothingAsync())
            .Match(s => s, e => e.ErrorMessage);
        Assert.That(wasAwaited, Is.True);
        Assert.That(actual, Is.EqualTo("VALUE_VALUE_"));
    }


    [Test]
    public async Task async_matching_successful_result_executes_success_lambda()
    {
        Assert.That(await Result.New("success")
                .Map(Task.FromResult)
                .Match(_ => Task.FromResult(true), _ => Task.FromResult(false)),
            Is.True);
    }

    [Test]
    public async Task async_matching_error_result_executes_error_lambda()
    {
        Assert.That(await Result.Error<string>(someError)
                .Map(Task.FromResult)
                .Match(_ => Task.FromResult(true), _ => Task.FromResult(false)),
            Is.False);
    }
    
    [Test]
    public void implicit_conversion_from_error_works()
    {
        Result<string> result = someError;
        Assert.That(result.MatchError(out var e), Is.True);
        Assert.That(e, Is.EqualTo(someError));
    }
    
    [Test]
    public void implicit_conversion_from_value_works()
    {
        Result<string> result = "success";
        Assert.That(result.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo("success"));
    }


    [Test]
    public void executing_action_on_successful_result()
    {
        bool wasRun = false;
        Assert.That(Result.New("value").Do(v => wasRun = true).MatchSuccess(out var actual), Is.True);
        Assert.That(wasRun, Is.True);
        Assert.That(actual, Is.EqualTo("value"));
    }
    
    [Test]
    public void action_on_error_result_not_triggered()
    {
        bool wasRun = false;
        Assert.That(Result.Error<string>(someError).Do(v => wasRun = true).MatchError(out var actual), Is.True);
        Assert.That(wasRun, Is.False);
    }
    
    [Test]
    public async Task async_actions_executed_on_successful_result()
    {
        var runCount = 0;
        var actual = await Result.New("value")
            .Do(v => Task.Run(() => runCount++))               // starting async
            .Do(v => { runCount++; })                          // chaining async
            .Do(async v => await Task.Run(()=> runCount++))    // returning async in async chain
            .Match(s => s, e => e.ErrorMessage);
        Assert.That(actual, Is.EqualTo("value"));
        Assert.That(runCount, Is.EqualTo(3));
    }
    
    [Test]
    public async Task async_actions_not_executed_on_error_result_result()
    {
        var runCount = 0;
        await Result.Error<string>(someError)
            .Do(v => Task.Run(() => runCount++))               // starting async
            .Do(v => { runCount++; })                          // chaining async
            .Do(async v => await Task.Run(()=> runCount++))    // returning async in async chain
            .Match(s => s, e => e.ErrorMessage);
        Assert.That(runCount, Is.EqualTo(0));
    }

    [Test]
    public async Task exception_is_translated_to_error_result()
    {
        var exMessage = "I throw some exception";
        var result = await WrapExceptionHelper(exMessage);
        Assert.That( result.MatchError( out var e ), Is.True );
        Assert.That( e.ErrorIdentifier, Is.EqualTo( "UnhandledException" ) );
        Assert.That( e.ErrorMessage, Is.EqualTo( exMessage ) );
    }
    
    [Test]
    public void mapping_error_result_matches_error()
    {
        var mappedError = Result.Error<string>(someError).MapError(e => Error.Create("identifier", "data"));
        
        Assert.That(mappedError.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo("identifier"));
        Assert.That(error.ErrorMessage, Is.EqualTo("data"));
    }
    
    [Test]
    public void mapping_error_on_successful_result_matches_success()
    {
        var unmodifiedResult = Result.New("value")
            .MapError(e => Error.Create("identifier", "data"));
        
        Assert.That(unmodifiedResult.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo("value"));
    }
    
    [Test]
    public async Task async_mapping_error_on_error_result_matches_mapped_Error()
    {
        var actual = await Result.Error<string>(someError)
            .Map(v => Task.FromResult(v.ToUpper()))
            .MapError(e => Error.Create(e.ErrorIdentifier, e.ErrorMessage));

        Assert.That(actual.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo(someError.ErrorIdentifier));
        Assert.That(error.ErrorMessage, Is.EqualTo(someError.ErrorMessage));
    }

    [Test]
    public async Task nesting_binds_using_query_syntax()
    {
        var actual = DoubleStringResult("1")
            .Bind(v1 => DoubleStringResult(v1)
                .Bind(v2 => DoubleStringResult(v2)
                    .Map(v3 => DoubleString(v3))));
        
        Assert.That(actual.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo("1111111111111111"));


        Result<string> querySyntaxResult =
            from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResult(v1)
            from v3 in DoubleStringResult(v2)
            select DoubleString(v3);
        Assert.That(querySyntaxResult.MatchSuccess(out var querySyntax), Is.True, () => querySyntaxResult.Error!.ErrorMessage);
        Assert.That(querySyntax, Is.EqualTo("1111111111111111"));
    }
    
    [Test]
    public async Task nesting_binds_using_query_syntax_asynchronously()
    {
        var query =
            await from v1 in DoubleStringResultAsync("1")
                from v2 in DoubleStringResult(v1)
                from v3 in DoubleStringResult(v2)
                select DoubleString(v3);
        Assert.That(query.MatchSuccess(out var actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));
        
        query = await from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResultAsync(v1)
            from v3 in DoubleStringResult(v2)
            select DoubleString(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));
        
        query = await from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResult(v1)
            from v3 in DoubleStringResultAsync(v2)
            select DoubleString(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));
        
        query = await from v1 in DoubleStringResultAsync("1")
            from v2 in DoubleStringResultAsync(v1)
            from v3 in DoubleStringResultAsync(v2)
            select DoubleStringAsync(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));
        
        query = await from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResult(v1)
            from v3 in DoubleStringResult(v2)
            select DoubleStringAsync(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));
        
        query = from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResult(v1)
            from v3 in DoubleStringResult(v2)
            select DoubleStringResult(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));
        
        query = await from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResult(v1)
            from v3 in DoubleStringResult(v2)
            select DoubleStringResultAsync(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));
        
        query = await from v1 in DoubleStringResultAsync("1")
            from v2 in DoubleStringResultAsync(v1)
            from v3 in DoubleStringResultAsync(v2)
            select DoubleStringResultAsync(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));
    }

    [Test]
    public async Task mapping_with_query_syntax()
    {
        var result1 = Result.New("1");

        var querySyntaxResult =
            from v1 in result1
            select DoubleString(v1);
        
        Assert.That(querySyntaxResult.MatchSuccess(out var querySyntax), Is.True, () => querySyntaxResult.Error!.ErrorMessage);
        Assert.That(querySyntax, Is.EqualTo("11"));
    }
    
    [Test]
    public async Task mapping_with_query_syntax_Async()
    {
        var result1 = Task.FromResult(Result.New("1"));

        var querySyntaxResult =
            await (from v1 in result1
                select DoubleStringAsync(v1));
        
        Assert.That(querySyntaxResult.MatchSuccess(out var querySyntax), Is.True, () => querySyntaxResult.Error!.ErrorMessage);
        Assert.That(querySyntax, Is.EqualTo("11"));
    }
    
    [Test]
    public async Task flattening_ienumerable_of_results_with_error_works()
    {
        var input = new[] { "1", "2", "3", "triggersError", "5"};

        var result = input.Select(i => DoubleStringResult(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : someError))
            .FlattenResults(someError.ErrorIdentifier);

        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo(someError.ErrorIdentifier));
        Assert.That(error.ErrorMessage, Is.EqualTo(someError.ErrorMessage));
    }
    
    [Test]
    public async Task flattening_ienumerable_of_results_with_multiple_errors_works()
    {
        var input = new[] { "1", "x", "3", "y", "5", "x", "z"};

        var result = input.Select(i => new Result<string>(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : Error.Create(someError.ErrorIdentifier, $"cannot parse {i}")))
            .FlattenResults(someError.ErrorIdentifier, "someMessage");

        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo(someError.ErrorIdentifier));
        Assert.That(error.ErrorMessage, Is.EqualTo("someMessage"));
        Assert.That(result.ValidationMessages, Has.Length.EqualTo(3));
        var messages = result.ValidationMessages.Select(m => m.Message).ToArray();
        Assert.That(messages, Does.Contain("cannot parse x"));
        Assert.That(messages, Does.Contain("cannot parse y"));
        Assert.That(messages, Does.Contain("cannot parse z"));
    }
    
    [Test]
    public async Task flattening_ienumerable_of_results_without_error_works()
    {
        var input = new[] { "1", "2", "3", "4", "5"};

        var result = input.Select(i => DoubleStringResult(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : someError))
            .FlattenResults(someError.ErrorIdentifier)
            .Map(x => x.Sum());

        Assert.That(result.MatchSuccess(out var sum), Is.True);
        Assert.That(sum, Is.EqualTo(11+22+33+44+55));
    }
    
    [Test]
    public void flattening_values_ienumerable_of_results_with_error_works()
    {
        var input = new[] { "1", "2", "3", "triggersError", "5"};

        var result = input.Select(i => DoubleStringResult(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : someError))
            .FlattenValues()
            .Map(x => x.Sum());

        Assert.That(result.MatchSuccess(out var sum), Is.True);
        Assert.That(sum, Is.EqualTo(11+22+33+55));
        Assert.That(result.ValidationMessages, Has.Length.EqualTo(1));
    }
    
    [Test]
    public async Task flattening_values_ienumerable_of_results_with_multiple_errors_works()
    {
        var input = new[] { "1", "x", "3", "y", "5", "x", "z"};

        var result = input.Select(i => new Result<string>(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : Error.Create(someError.ErrorIdentifier, $"cannot parse {i}")))
            .FlattenValues()
            .Map(x => x.Sum());

        Assert.That(result.MatchSuccess(out var sum), Is.True);
        Assert.That(sum, Is.EqualTo(1+3+5));
        Assert.That(result.ValidationMessages, Has.Length.EqualTo(3));
    }
    
    [Test]
    public async Task flattening_values_ienumerable_of_results_with_all_errors_works()
    {
        var input = new[] { "x", "y", "x", "z"};

        var result = input.Select(i => new Result<string>(i)
                .Bind<string, int>(i =>
                    int.TryParse(i, out var result)
                        ? result
                        : Error.Create(someError.ErrorIdentifier, $"cannot parse {i}")))
            .FlattenValues();
            

        Assert.That(result.MatchSuccess(out var parsedValues), Is.True);
        Assert.That(parsedValues, Is.Empty);

        Assert.That(result.ValidationMessages, Has.Length.EqualTo(3));
    }
    
    [Test]
    public void flattening_values_ienumerable_of_results_without_error_works()
    {
        var input = new[] { "1", "2", "3", "4", "5"};

        var result = input.Select(i => DoubleStringResult(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : someError))
            .FlattenValues()
            .Map(x => x.Sum());

        Assert.That(result.MatchSuccess(out var sum), Is.True);
        Assert.That(sum, Is.EqualTo(11+22+33+44+55));
        Assert.That(result.ValidationMessages, Has.Length.EqualTo(0));
    }
    
    [Test]
    public async Task tapping_into_the_chain_works()
    {
        void TapAction(Result<string> s) => Assert.That(s.Value, Is.Not.Null);
        var actual = await Result.New("value")
            .Tap(TapAction)
            .Bind(v => Task.FromResult(Result.New(v.ToUpper()))) // starting async
            .Tap(TapAction)
            .Bind(v => Result.New($"{v}_")) // chaining async
            .Tap(TapAction)
            .Bind(async v => await DoubleStringResultAsync(v)) // returning async in async chain
            .Tap(TapAction)
            .Match(s => s, e => e.ErrorMessage);
        Assert.That(actual, Is.EqualTo("VALUE_VALUE_"));
    }

    [Test]
    public void json_deserialize_error_will_work_net_version()
    {
        var error = System.Text.Json.JsonSerializer.Deserialize<Error>("{\"ErrorIdentifier\":\"GenericError\",\"ErrorMessage\":\"Cannot interpret id string as a valid numerical identifier.\", \"Context\":\"some context\" }");
        
        Assert.That(error.ErrorIdentifier, Is.EqualTo("GenericError"));
        Assert.That(error.ErrorMessage, Is.EqualTo("Cannot interpret id string as a valid numerical identifier."));
        Assert.That(error.Context, Is.EqualTo("some context"));
    }

    private async Task<Result<string>> WrapExceptionHelper( string exMessage )
    {
        try
        {
            await Task.Delay( 1 );
            throw new Exception( exMessage );
        }
        catch( Exception ex )
        {
            return Error.Create( "UnhandledException", ex.Message );
        }
    }
    
    private Task<string> DoubleStringAsync(string x) => Task.FromResult($"{x}{x}");
    private Task<Result<string>> DoubleStringResultAsync(string x) => Task.FromResult(Result.New($"{x}{x}"));
    
    private Result<string> DoubleStringResult(string x) => Result.New($"{x}{x}");
    private string DoubleString(string x) => $"{x}{x}";
    
    private Result<string> DoubleStringError(string x) => Error.Create("", $"shit");
    
    private async Task ReturnsNothingAsync()
    {
        await Task.Delay(100);
        wasAwaited = true;
    }
    
    #region Tag-based state discrimination tests

    [Test]
    public void default_result_matches_success_not_error()
    {
        // default(Result<T>) has _isError = false, so it lands on the success track.
        // This is the expected behavior for a struct with a bool tag defaulting to false.
        var result = default(Result<string>);
        
        Assert.That(result.MatchSuccess(out _), Is.True);
        Assert.That(result.MatchError(out _), Is.False);
    }
    
    [Test]
    public void default_result_value_is_default_of_T()
    {
        var result = default(Result<int>);
        
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(0));
    }
    
    [Test]
    public void explicit_error_constructor_always_matches_error()
    {
        // Even if Error were somehow null-like, the tag field ensures correctness.
        var error = Error.Create("test", "msg");
        var result = new Result<string>(error);
        
        Assert.That(result.MatchError(out var e), Is.True);
        Assert.That(e, Is.EqualTo(error));
        Assert.That(result.MatchSuccess(out _), Is.False);
    }
    
    [Test]
    public void explicit_success_constructor_always_matches_success()
    {
        var result = new Result<string>("hello");
        
        Assert.That(result.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo("hello"));
        Assert.That(result.MatchError(out _), Is.False);
    }
    
    [Test]
    public void with_validation_messages_preserves_error_state()
    {
        var error = Error.Create("test", "msg");
        var result = new Result<string>(error);
        var warning = new ValidationMessage("w1", ValidationLevel.Warning, "warn");
        
        var resultWithMessages = result.WithValidationMessages(warning);
        
        Assert.That(resultWithMessages.MatchError(out _), Is.True);
        Assert.That(resultWithMessages.ValidationMessages, Has.Length.EqualTo(1));
    }
    
    [Test]
    public void with_validation_messages_preserves_success_state()
    {
        var result = Result.New("hello");
        var warning = new ValidationMessage("w1", ValidationLevel.Warning, "warn");
        
        var resultWithMessages = result.WithValidationMessages(warning);
        
        Assert.That(resultWithMessages.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo("hello"));
        Assert.That(resultWithMessages.ValidationMessages, Has.Length.EqualTo(1));
    }
    
    #endregion
}