namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultBindTests : ResultTestBase
{
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
}
