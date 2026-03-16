namespace FluentSolidity.FunctionalExtensions.Tests;

public class EitherBindTests
{
    [Test]
    public async Task bind_sync_either_async_mapper()
    {
        var e = Either.Right<string, int>(5);
        var result = await e.Bind(x => Task.FromResult(Either.Right<string, string>(x.ToString())));
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo("5"));
    }

    [Test]
    public async Task bind_async_either_sync_mapper()
    {
        var e = Task.FromResult(Either.Right<string, int>(5));
        var result = await e.Bind(x => Either.Right<string, string>(x.ToString()));
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo("5"));
    }

    [Test]
    public async Task bind_async_either_async_mapper()
    {
        var e = Task.FromResult(Either.Right<string, int>(5));
        var result = await e.Bind(x => Task.FromResult(Either.Right<string, string>(x.ToString())));
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo("5"));
    }
}
