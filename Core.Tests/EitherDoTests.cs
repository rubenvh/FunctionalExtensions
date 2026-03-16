namespace FluentSolidity.FunctionalExtensions.Tests;

public class EitherDoTests
{
    [Test]
    public void do_executes_action_on_right()
    {
        var sideEffect = 0;
        var e = Either.Right<string, int>(10);
        var returned = e.Do(r => sideEffect = r);
        Assert.That(sideEffect, Is.EqualTo(10));
        Assert.That(returned.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(10));
    }

    [Test]
    public void do_skips_action_on_left()
    {
        var sideEffect = 0;
        var e = Either.Left<string, int>("err");
        var returned = e.Do(r => sideEffect = r);
        Assert.That(sideEffect, Is.EqualTo(0));
        Assert.That(returned.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("err"));
    }

    [Test]
    public async Task do_async_action_on_right()
    {
        var sideEffect = 0;
        var e = Either.Right<string, int>(10);
        var returned = await e.Do(async r =>
        {
            await Task.Yield();
            sideEffect = r;
        });
        Assert.That(sideEffect, Is.EqualTo(10));
        Assert.That(returned.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(10));
    }

    [Test]
    public async Task do_async_either_sync_action()
    {
        var sideEffect = 0;
        var e = Task.FromResult(Either.Right<string, int>(10));
        var returned = await e.Do(r => sideEffect = r);
        Assert.That(sideEffect, Is.EqualTo(10));
        Assert.That(returned.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(10));
    }

    [Test]
    public async Task do_async_either_async_action()
    {
        var sideEffect = 0;
        var e = Task.FromResult(Either.Right<string, int>(10));
        var returned = await e.Do(async r =>
        {
            await Task.Yield();
            sideEffect = r;
        });
        Assert.That(sideEffect, Is.EqualTo(10));
        Assert.That(returned.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(10));
    }
}
