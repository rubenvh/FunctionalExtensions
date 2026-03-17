namespace FluentSolidity.FunctionalExtensions.Tests;

public class MaybeDoTests
{
    [Test]
    public void do_executes_on_some()
    {
        var executed = false;
        var result = Maybe.Some("hello").Do(v => executed = true);
        Assert.That(executed, Is.True);
        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo("hello"));
    }

    [Test]
    public void do_does_not_execute_on_none()
    {
        var executed = false;
        var result = Maybe.None<string>().Do(v => executed = true);
        Assert.That(executed, Is.False);
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_do_sync_maybe_async_action()
    {
        var executed = false;
        var result = await Maybe.Some("hello").Do(async v => { await Task.Yield(); executed = true; });
        Assert.That(executed, Is.True);
        Assert.That(result.MatchSome(out _), Is.True);
    }

    [Test]
    public async Task async_do_async_maybe_sync_action()
    {
        var executed = false;
        var result = await Task.FromResult(Maybe.Some("hello")).Do(v => executed = true);
        Assert.That(executed, Is.True);
        Assert.That(result.MatchSome(out _), Is.True);
    }

    [Test]
    public async Task async_do_async_maybe_async_action()
    {
        var executed = false;
        var result = await Task.FromResult(Maybe.Some("hello")).Do(async v => { await Task.Yield(); executed = true; });
        Assert.That(executed, Is.True);
        Assert.That(result.MatchSome(out _), Is.True);
    }

    [Test]
    public async Task async_do_none_does_not_execute()
    {
        var executed = false;
        var result = await Task.FromResult(Maybe.None<string>()).Do(v => executed = true);
        Assert.That(executed, Is.False);
        Assert.That(result.MatchNone(), Is.True);
    }
}
