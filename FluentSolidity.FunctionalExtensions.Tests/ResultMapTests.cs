namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultMapTests : ResultTestBase
{
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

    /// <summary>
    /// Example: happy-path chaining with Map and Bind demonstrates how Map transforms values
    /// through a pipeline, including async steps.
    /// </summary>
    [Test]
    public async Task happy_path_chaining_with_map_and_bind()
    {
        Task<Result<int>> CreateAsync(string raw) => Task.FromResult(int.TryParse(raw, out var v)
            ? Result.New(v)
            : Result.Error<int>(Error.Create("ParseError", $"Cannot parse '{raw}'")));

        var final = await CreateAsync("21")
            .Map(v => v * 2)              // 42
            .Bind(v => Result.New(v + 1)) // 43
            .Map(async v => await Task.FromResult(v * 2)); // 86

        Assert.That(final.MatchSuccess(out var value), Is.True, () => final.Error!.ErrorMessage);
        Assert.That(value, Is.EqualTo(86));
    }

    /// <summary>
    /// Example: short-circuiting stops execution on error, demonstrating that Map is skipped
    /// after a Bind produces an error.
    /// </summary>
    [Test]
    public void short_circuiting_stops_execution_on_error()
    {
        var error = Error.Create("BadInput", "Boom");

        var result = Result.New(10)
            .Bind(_ => Result.Error<int>(error)) // becomes error Result<int>
            .Map(v => v * 10) // never executed
            .Match(v => v.ToString(), e => e.ErrorIdentifier);

        Assert.That(result, Is.EqualTo("BadInput"));
    }
}
