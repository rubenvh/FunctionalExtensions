namespace FluentSolidity.FunctionalExtensions.Tests;

public class MaybeBindTests
{
    [Test]
    public void bind_some_to_some()
    {
        var result = Maybe.Some("hello").Bind(v => Maybe.Some(v.Length));
        Assert.That(result.MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo(5));
    }

    [Test]
    public void bind_some_to_none()
    {
        var result = Maybe.Some("hello").Bind(_ => Maybe.None<int>());
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public void bind_none_shortcircuits()
    {
        var binderCalled = false;
        var result = Maybe.None<string>().Bind(v => { binderCalled = true; return Maybe.Some(v.Length); });
        Assert.That(binderCalled, Is.False);
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_bind_some_with_async_binder()
    {
        var result = await Maybe.Some("hello").Bind(v => Task.FromResult(Maybe.Some(v.Length)));
        Assert.That(result.MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo(5));
    }

    [Test]
    public async Task async_bind_none_with_async_binder()
    {
        var result = await Maybe.None<string>().Bind(v => Task.FromResult(Maybe.Some(v.Length)));
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_maybe_with_sync_binder()
    {
        var result = await Task.FromResult(Maybe.Some("hello")).Bind(v => Maybe.Some(v.Length));
        Assert.That(result.MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo(5));
    }

    [Test]
    public async Task async_maybe_with_async_binder()
    {
        var result = await Task.FromResult(Maybe.Some("hello")).Bind(v => Task.FromResult(Maybe.Some(v.Length)));
        Assert.That(result.MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo(5));
    }

    [Test]
    public void bind_chains_with_map()
    {
        var result = Maybe.Some("hello")
            .Map(v => v.ToUpper())
            .Bind(v => v.Length > 3 ? Maybe.Some(v) : Maybe.None<string>())
            .Map(v => v.Length);
        Assert.That(result.MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo(5));
    }

    [Test]
    public void bind_chain_stops_at_none()
    {
        var secondBinderCalled = false;
        var result = Maybe.Some("hi")
            .Bind(_ => Maybe.None<int>())
            .Bind(v => { secondBinderCalled = true; return Maybe.Some(v * 2); });
        Assert.That(secondBinderCalled, Is.False);
        Assert.That(result.MatchNone(), Is.True);
    }
}
