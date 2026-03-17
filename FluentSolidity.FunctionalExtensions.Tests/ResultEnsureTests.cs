namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultEnsureTests : ResultTestBase
{
    [Test]
    public void ensure_passes_when_predicate_holds()
    {
        var result = Result.New(42).Ensure(v => v > 0, v => Error.Create("NEG", $"{v} is not positive"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void ensure_flips_to_error_when_predicate_fails()
    {
        var result = Result.New(-1).Ensure(v => v > 0, v => Error.Create("NEG", $"{v} is not positive"));
        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo("NEG"));
        Assert.That(error.ErrorMessage, Is.EqualTo("-1 is not positive"));
    }

    [Test]
    public void ensure_shortcircuits_on_error()
    {
        var predicateCalled = false;
        var result = Result.Error<int>(Error.Create("ERR", "already bad"))
            .Ensure(v => { predicateCalled = true; return v > 0; }, v => Error.Create("NEG", "nope"));
        Assert.That(predicateCalled, Is.False);
        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo("ERR"));
    }

    [Test]
    public void ensure_preserves_pipeline_messages_on_pass()
    {
        var msg = new PipelineMessage("v1", MessageLevel.Info, "info");
        var result = Result.New(42).WithMessages(msg)
            .Ensure(v => v > 0, v => Error.Create("NEG", "nope"));
        Assert.That(result.MatchSuccess(out _), Is.True);
        Assert.That(result.Messages, Has.Count.EqualTo(1));
        Assert.That(result.Messages[0], Is.EqualTo(msg));
    }

    [Test]
    public void ensure_preserves_pipeline_messages_on_fail()
    {
        var msg = new PipelineMessage("v1", MessageLevel.Info, "info");
        var result = Result.New(-1).WithMessages(msg)
            .Ensure(v => v > 0, v => Error.Create("NEG", "nope"));
        Assert.That(result.MatchError(out _), Is.True);
        Assert.That(result.Messages, Has.Count.EqualTo(1));
        Assert.That(result.Messages[0], Is.EqualTo(msg));
    }

    [Test]
    public async Task ensure_async_predicate_passes()
    {
        var result = await Result.New(42).Ensure(
            async v => { await Task.Yield(); return v > 0; },
            v => Error.Create("NEG", "nope"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public async Task ensure_async_predicate_fails()
    {
        var result = await Result.New(-1).Ensure(
            async v => { await Task.Yield(); return v > 0; },
            v => Error.Create("NEG", $"{v} is not positive"));
        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo("NEG"));
    }

    [Test]
    public async Task ensure_async_result_sync_predicate_passes()
    {
        var result = await Task.FromResult(Result.New(42))
            .Ensure(v => v > 0, v => Error.Create("NEG", "nope"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public async Task ensure_async_result_sync_predicate_fails()
    {
        var result = await Task.FromResult(Result.New(-1))
            .Ensure(v => v > 0, v => Error.Create("NEG", "nope"));
        Assert.That(result.MatchError(out _), Is.True);
    }

    [Test]
    public async Task ensure_async_result_async_predicate()
    {
        var result = await Task.FromResult(Result.New(42))
            .Ensure(async v => { await Task.Yield(); return v > 0; }, v => Error.Create("NEG", "nope"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void ensure_chains_with_map_and_bind()
    {
        var result = Result.New("hello")
            .Ensure(s => s.Length > 0, _ => Error.Create("EMPTY", "empty string"))
            .Map(s => s.ToUpper())
            .Ensure(s => s.Contains('H'), _ => Error.Create("NO_H", "must contain H"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("HELLO"));
    }

    [Test]
    public void ensure_chain_stops_at_first_failure()
    {
        var secondCalled = false;
        var result = Result.New("")
            .Ensure(s => s.Length > 0, _ => Error.Create("EMPTY", "empty string"))
            .Ensure(s => { secondCalled = true; return s.Contains('H'); }, _ => Error.Create("NO_H", "must contain H"));
        Assert.That(secondCalled, Is.False);
        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo("EMPTY"));
    }
}
