namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultDoErrorTests : ResultTestBase
{
    [Test]
    public void doerror_executes_action_on_error()
    {
        Error? captured = null;
        var error = Error.Create("ERR", "something broke");
        var result = Result.Error<int>(error).DoError(e => captured = e);
        Assert.That(captured, Is.EqualTo(error));
        Assert.That(result.MatchError(out var e2), Is.True);
        Assert.That(e2, Is.EqualTo(error));
    }

    [Test]
    public void doerror_skips_action_on_success()
    {
        Error? captured = null;
        var result = Result.New(42).DoError(e => captured = e);
        Assert.That(captured, Is.Null);
        Assert.That(result.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo(42));
    }

    [Test]
    public async Task doerror_async_action_on_error()
    {
        Error? captured = null;
        var error = Error.Create("ERR", "something broke");
        var result = await Result.Error<int>(error).DoError(async e =>
        {
            await Task.Yield();
            captured = e;
        });
        Assert.That(captured, Is.EqualTo(error));
        Assert.That(result.MatchError(out _), Is.True);
    }

    [Test]
    public async Task doerror_async_action_skips_on_success()
    {
        Error? captured = null;
        var result = await Result.New(42).DoError(async e =>
        {
            await Task.Yield();
            captured = e;
        });
        Assert.That(captured, Is.Null);
        Assert.That(result.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo(42));
    }

    [Test]
    public async Task doerror_async_result_sync_action()
    {
        Error? captured = null;
        var error = Error.Create("ERR", "something broke");
        var result = await Task.FromResult(Result.Error<int>(error)).DoError(e => captured = e);
        Assert.That(captured, Is.EqualTo(error));
        Assert.That(result.MatchError(out _), Is.True);
    }

    [Test]
    public async Task doerror_async_result_async_action()
    {
        Error? captured = null;
        var error = Error.Create("ERR", "something broke");
        var result = await Task.FromResult(Result.Error<int>(error)).DoError(async e =>
        {
            await Task.Yield();
            captured = e;
        });
        Assert.That(captured, Is.EqualTo(error));
        Assert.That(result.MatchError(out _), Is.True);
    }

    [Test]
    public void doerror_preserves_pipeline_messages()
    {
        var msg = new PipelineMessage("v1", MessageLevel.Warning, "heads up");
        var error = Error.Create("ERR", "broke");
        var result = Result.Error<int>(error, msg).DoError(_ => { });
        Assert.That(result.Messages, Has.Count.EqualTo(1));
        Assert.That(result.Messages[0], Is.EqualTo(msg));
    }
}
