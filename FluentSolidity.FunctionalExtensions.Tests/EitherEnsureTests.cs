namespace FluentSolidity.FunctionalExtensions.Tests;

public class EitherEnsureTests
{
    [Test]
    public void ensure_passes_when_predicate_holds()
    {
        var e = Either.Right<string, int>(42);
        var result = e.Ensure(v => v > 0, v => $"{v} is not positive");
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(42));
    }

    [Test]
    public void ensure_flips_to_left_when_predicate_fails()
    {
        var e = Either.Right<string, int>(-1);
        var result = e.Ensure(v => v > 0, v => $"{v} is not positive");
        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("-1 is not positive"));
    }

    [Test]
    public void ensure_shortcircuits_on_left()
    {
        var predicateCalled = false;
        var e = Either.Left<string, int>("already bad");
        var result = e.Ensure(v => { predicateCalled = true; return v > 0; }, v => "nope");
        Assert.That(predicateCalled, Is.False);
        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("already bad"));
    }

    [Test]
    public async Task ensure_async_predicate_passes()
    {
        var e = Either.Right<string, int>(42);
        var result = await e.Ensure(
            async v => { await Task.Yield(); return v > 0; },
            v => "nope");
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(42));
    }

    [Test]
    public async Task ensure_async_predicate_fails()
    {
        var e = Either.Right<string, int>(-1);
        var result = await e.Ensure(
            async v => { await Task.Yield(); return v > 0; },
            v => $"{v} is not positive");
        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("-1 is not positive"));
    }

    [Test]
    public async Task ensure_async_either_sync_predicate()
    {
        var e = Task.FromResult(Either.Right<string, int>(42));
        var result = await e.Ensure(v => v > 0, v => "nope");
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(42));
    }

    [Test]
    public async Task ensure_async_either_async_predicate()
    {
        var e = Task.FromResult(Either.Right<string, int>(42));
        var result = await e.Ensure(
            async v => { await Task.Yield(); return v > 0; },
            v => "nope");
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(42));
    }

    [Test]
    public void ensure_chains_with_map()
    {
        var result = Either.Right<string, int>(5)
            .Ensure(v => v > 0, _ => "not positive")
            .Map(v => v * 2)
            .Ensure(v => v < 100, v => $"{v} too large");
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(10));
    }

    [Test]
    public void ensure_chain_stops_at_first_failure()
    {
        var secondCalled = false;
        var result = Either.Right<string, int>(-1)
            .Ensure(v => v > 0, _ => "not positive")
            .Ensure(v => { secondCalled = true; return v < 100; }, _ => "too large");
        Assert.That(secondCalled, Is.False);
        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("not positive"));
    }
}
