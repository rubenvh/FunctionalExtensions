namespace FluentSolidity.FunctionalExtensions.Tests;

public class MaybeDoNoneTests
{
    [Test]
    public void donone_executes_on_none()
    {
        var executed = false;
        var result = Maybe.None<string>().DoNone(() => executed = true);
        Assert.That(executed, Is.True);
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public void donone_does_not_execute_on_some()
    {
        var executed = false;
        var result = Maybe.Some("hello").DoNone(() => executed = true);
        Assert.That(executed, Is.False);
        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo("hello"));
    }

    [Test]
    public async Task async_donone_sync_maybe_async_action()
    {
        var executed = false;
        var result = await Maybe.None<string>().DoNone(async () => { await Task.Yield(); executed = true; });
        Assert.That(executed, Is.True);
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_donone_async_maybe_sync_action()
    {
        var executed = false;
        var result = await Task.FromResult(Maybe.None<string>()).DoNone(() => executed = true);
        Assert.That(executed, Is.True);
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_donone_async_maybe_async_action()
    {
        var executed = false;
        var result = await Task.FromResult(Maybe.None<string>()).DoNone(async () => { await Task.Yield(); executed = true; });
        Assert.That(executed, Is.True);
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_donone_some_does_not_execute()
    {
        var executed = false;
        var result = await Task.FromResult(Maybe.Some("hello")).DoNone(() => executed = true);
        Assert.That(executed, Is.False);
        Assert.That(result.MatchSome(out _), Is.True);
    }
}
