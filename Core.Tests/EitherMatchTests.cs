namespace FluentSolidity.FunctionalExtensions.Tests;

public class EitherMatchTests
{
    [Test]
    public void match_calls_right_mapper_for_right()
    {
        var e = Either.Right<string, int>(10);
        var result = e.Match(r => r * 2, l => -1);
        Assert.That(result, Is.EqualTo(20));
    }

    [Test]
    public void match_calls_left_mapper_for_left()
    {
        var e = Either.Left<string, int>("fail");
        var result = e.Match(r => r * 2, l => -1);
        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public async Task match_async_either_sync_mappers()
    {
        var e = Task.FromResult(Either.Right<string, int>(10));
        var result = await e.Match(r => r * 2, l => -1);
        Assert.That(result, Is.EqualTo(20));
    }

    [Test]
    public async Task match_async_either_async_mappers()
    {
        var e = Task.FromResult(Either.Left<string, int>("fail"));
        var result = await e.Match(
            r => Task.FromResult(r * 2),
            l => Task.FromResult(-1));
        Assert.That(result, Is.EqualTo(-1));
    }
}
