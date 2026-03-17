namespace FluentSolidity.FunctionalExtensions.Tests;

public class MaybeEnsureTests
{
    [Test]
    public void ensure_passes_when_predicate_holds()
    {
        var result = Maybe.Some(42).Ensure(v => v > 0);
        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void ensure_flips_to_none_when_predicate_fails()
    {
        var result = Maybe.Some(-1).Ensure(v => v > 0);
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public void ensure_shortcircuits_on_none()
    {
        var predicateCalled = false;
        var result = Maybe.None<int>().Ensure(v => { predicateCalled = true; return v > 0; });
        Assert.That(predicateCalled, Is.False);
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_ensure_sync_maybe_async_predicate_passes()
    {
        var result = await Maybe.Some(42).Ensure(async v => { await Task.Yield(); return v > 0; });
        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public async Task async_ensure_sync_maybe_async_predicate_fails()
    {
        var result = await Maybe.Some(-1).Ensure(async v => { await Task.Yield(); return v > 0; });
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_ensure_async_maybe_sync_predicate()
    {
        var result = await Task.FromResult(Maybe.Some(42)).Ensure(v => v > 0);
        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public async Task async_ensure_async_maybe_async_predicate()
    {
        var result = await Task.FromResult(Maybe.Some(42)).Ensure(async v => { await Task.Yield(); return v > 0; });
        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void ensure_chains_with_map_and_bind()
    {
        var result = Maybe.Some("hello")
            .Ensure(s => s.Length > 0)
            .Map(s => s.ToUpper())
            .Ensure(s => s.Contains('H'));
        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo("HELLO"));
    }

    [Test]
    public void ensure_chain_stops_at_first_failure()
    {
        var secondCalled = false;
        var result = Maybe.Some("")
            .Ensure(s => s.Length > 0)
            .Ensure(s => { secondCalled = true; return s.Contains('H'); });
        Assert.That(secondCalled, Is.False);
        Assert.That(result.MatchNone(), Is.True);
    }
}
