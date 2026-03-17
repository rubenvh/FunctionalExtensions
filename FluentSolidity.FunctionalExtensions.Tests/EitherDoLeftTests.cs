namespace FluentSolidity.FunctionalExtensions.Tests;

public class EitherDoLeftTests
{
    [Test]
    public void doleft_executes_action_on_left()
    {
        string? captured = null;
        var e = Either.Left<string, int>("err");
        var returned = e.DoLeft(l => captured = l);
        Assert.That(captured, Is.EqualTo("err"));
        Assert.That(returned.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("err"));
    }

    [Test]
    public void doleft_skips_action_on_right()
    {
        string? captured = null;
        var e = Either.Right<string, int>(10);
        var returned = e.DoLeft(l => captured = l);
        Assert.That(captured, Is.Null);
        Assert.That(returned.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(10));
    }

    [Test]
    public async Task doleft_async_action_on_left()
    {
        string? captured = null;
        var e = Either.Left<string, int>("err");
        var returned = await e.DoLeft(async l =>
        {
            await Task.Yield();
            captured = l;
        });
        Assert.That(captured, Is.EqualTo("err"));
        Assert.That(returned.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("err"));
    }

    [Test]
    public async Task doleft_async_action_skips_on_right()
    {
        string? captured = null;
        var e = Either.Right<string, int>(10);
        var returned = await e.DoLeft(async l =>
        {
            await Task.Yield();
            captured = l;
        });
        Assert.That(captured, Is.Null);
        Assert.That(returned.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(10));
    }

    [Test]
    public async Task doleft_async_either_sync_action()
    {
        string? captured = null;
        var e = Task.FromResult(Either.Left<string, int>("err"));
        var returned = await e.DoLeft(l => captured = l);
        Assert.That(captured, Is.EqualTo("err"));
        Assert.That(returned.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("err"));
    }

    [Test]
    public async Task doleft_async_either_async_action()
    {
        string? captured = null;
        var e = Task.FromResult(Either.Left<string, int>("err"));
        var returned = await e.DoLeft(async l =>
        {
            await Task.Yield();
            captured = l;
        });
        Assert.That(captured, Is.EqualTo("err"));
        Assert.That(returned.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("err"));
    }
}
