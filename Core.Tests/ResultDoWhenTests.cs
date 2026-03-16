namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultDoWhenTests : ResultTestBase
{
    [Test]
    public void dowhen_executes_action_when_predicate_is_true()
    {
        string? captured = null;
        var result = Result.New("hello").DoWhen(v => v.Length > 3, v => captured = v);
        Assert.That(captured, Is.EqualTo("hello"));
        Assert.That(result.MatchSuccess(out var val), Is.True);
        Assert.That(val, Is.EqualTo("hello"));
    }

    [Test]
    public void dowhen_skips_action_when_predicate_is_false()
    {
        string? captured = null;
        var result = Result.New("hi").DoWhen(v => v.Length > 3, v => captured = v);
        Assert.That(captured, Is.Null);
        Assert.That(result.MatchSuccess(out var val), Is.True);
        Assert.That(val, Is.EqualTo("hi"));
    }

    [Test]
    public void dowhen_short_circuits_on_error()
    {
        bool predicateCalled = false;
        bool actionCalled = false;
        var result = Result.Error<string>(someError).DoWhen(
            v => { predicateCalled = true; return true; },
            v => actionCalled = true);
        Assert.That(predicateCalled, Is.False);
        Assert.That(actionCalled, Is.False);
        Assert.That(result.MatchError(out var err), Is.True);
        Assert.That(err, Is.EqualTo(someError));
    }

    [Test]
    public void dowhen_preserves_pipeline_messages()
    {
        var msg = new PipelineMessage("v1", MessageLevel.Warning, "heads up");
        var result = Result.New(42, msg).DoWhen(v => v > 0, _ => { });
        Assert.That(result.Messages, Has.Count.EqualTo(1));
        Assert.That(result.Messages[0], Is.EqualTo(msg));
    }

    [Test]
    public async Task dowhen_async_action_executes_when_predicate_true()
    {
        string? captured = null;
        var result = await Result.New("hello").DoWhen(v => v.Length > 3, async v =>
        {
            await Task.Yield();
            captured = v;
        });
        Assert.That(captured, Is.EqualTo("hello"));
        Assert.That(result.MatchSuccess(out var val), Is.True);
        Assert.That(val, Is.EqualTo("hello"));
    }

    [Test]
    public async Task dowhen_async_action_skips_when_predicate_false()
    {
        string? captured = null;
        var result = await Result.New("hi").DoWhen(v => v.Length > 3, async v =>
        {
            await Task.Yield();
            captured = v;
        });
        Assert.That(captured, Is.Null);
        Assert.That(result.MatchSuccess(out var val), Is.True);
        Assert.That(val, Is.EqualTo("hi"));
    }

    [Test]
    public async Task dowhen_async_result_sync_action_executes_when_predicate_true()
    {
        string? captured = null;
        var result = await Task.FromResult(Result.New("hello"))
            .DoWhen(v => v.Length > 3, v => captured = v);
        Assert.That(captured, Is.EqualTo("hello"));
        Assert.That(result.MatchSuccess(out var val), Is.True);
        Assert.That(val, Is.EqualTo("hello"));
    }

    [Test]
    public async Task dowhen_async_result_sync_action_short_circuits_on_error()
    {
        bool actionCalled = false;
        var result = await Task.FromResult(Result.Error<string>(someError))
            .DoWhen(v => true, v => actionCalled = true);
        Assert.That(actionCalled, Is.False);
        Assert.That(result.MatchError(out _), Is.True);
    }

    [Test]
    public async Task dowhen_async_result_async_action_executes_when_predicate_true()
    {
        string? captured = null;
        var result = await Task.FromResult(Result.New("hello"))
            .DoWhen(v => v.Length > 3, async v =>
            {
                await Task.Yield();
                captured = v;
            });
        Assert.That(captured, Is.EqualTo("hello"));
        Assert.That(result.MatchSuccess(out var val), Is.True);
        Assert.That(val, Is.EqualTo("hello"));
    }

    [Test]
    public async Task dowhen_async_result_async_action_short_circuits_on_error()
    {
        bool actionCalled = false;
        var result = await Task.FromResult(Result.Error<string>(someError))
            .DoWhen(v => true, async v =>
            {
                await Task.Yield();
                actionCalled = true;
            });
        Assert.That(actionCalled, Is.False);
        Assert.That(result.MatchError(out _), Is.True);
    }

    [Test]
    public async Task dowhen_chains_with_other_combinators()
    {
        var log = new List<string>();
        var actual = await Result.New(10)
            .DoWhen(v => v > 5, v => log.Add($"big:{v}"))
            .DoWhen(v => v > 20, v => log.Add($"huge:{v}"))
            .Do(v => log.Add($"always:{v}"))
            .Map(v => Task.FromResult(v * 2))
            .DoWhen(v => v == 20, v => log.Add($"doubled:{v}"))
            .Match(v => v, e => -1);
        Assert.That(actual, Is.EqualTo(20));
        Assert.That(log, Is.EqualTo(new[] { "big:10", "always:10", "doubled:20" }));
    }
}
