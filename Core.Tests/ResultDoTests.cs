namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultDoTests : ResultTestBase
{
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
            .Do(async v => await Task.Run(() => runCount++))   // returning async in async chain
            .Match(s => s, e => e.ErrorMessage);
        Assert.That(actual, Is.EqualTo("value"));
        Assert.That(runCount, Is.EqualTo(3));
    }

    [Test]
    public async Task async_actions_not_executed_on_error_result()
    {
        var runCount = 0;
        await Result.Error<string>(someError)
            .Do(v => Task.Run(() => runCount++))               // starting async
            .Do(v => { runCount++; })                          // chaining async
            .Do(async v => await Task.Run(() => runCount++))   // returning async in async chain
            .Match(s => s, e => e.ErrorMessage);
        Assert.That(runCount, Is.EqualTo(0));
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
}
