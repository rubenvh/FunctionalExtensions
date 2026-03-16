using System.Globalization;

namespace FluentSolidity.FunctionalExtensions.Tests;

public class EitherMapTests
{
    [Test]
    public void left_shortcircuits_chain()
    {
        var e = Either.Left<string, int>("error");

        var result = e
            .Map(i => i + 1.0m)
            .Map(x => (object)x.ToString(CultureInfo.InvariantCulture))
            .Map(x => x)
            .Bind(x => Either.Right<string, string>("6"));

        Assert.That(result.MatchLeft(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo("error"));
    }

    [Test]
    public void right_flows_through_chain()
    {
        var e = Either.Right<string, int>(5);

        var result = e
            .Map(i => i + 1.0m)
            .Map(x => (object)x.ToString(CultureInfo.InvariantCulture))
            .Map(x => x)
            .Bind(x => Either.Right<string, string>(x + "6"));

        Assert.That(result.MatchRight(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo("6.06"));
    }

    [Test]
    public void left_shortcircuits_in_middle_of_chain()
    {
        var e = Either.Right<string, int>(5);

        var result = e
            .Map(i => i + 1.0m)
            .Map(x => (object)x.ToString(CultureInfo.InvariantCulture))
            .Bind(x => Either.Left<string, object>("error"))
            .Bind(x => Either.Right<string, string>(x + "6"));

        Assert.That(result.MatchLeft(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo("error"));
    }

    [Test]
    public async Task map_sync_either_async_mapper()
    {
        var e = Either.Right<string, int>(5);
        var result = await e.Map(x => Task.FromResult(x + 10));
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(15));
    }

    [Test]
    public async Task map_async_either_sync_mapper()
    {
        var e = Task.FromResult(Either.Right<string, int>(5));
        var result = await e.Map(x => x + 10);
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(15));
    }

    [Test]
    public async Task map_async_either_async_mapper()
    {
        var e = Task.FromResult(Either.Right<string, int>(5));
        var result = await e.Map(x => Task.FromResult(x + 10));
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(15));
    }

    [Test]
    public async Task map_left_either_shortcircuits_async()
    {
        var e = Task.FromResult(Either.Left<string, int>("err"));
        var result = await e.Map(x => Task.FromResult(x + 10));
        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("err"));
    }
}
