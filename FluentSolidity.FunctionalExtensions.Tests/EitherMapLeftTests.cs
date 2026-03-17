namespace FluentSolidity.FunctionalExtensions.Tests;

public class EitherMapLeftTests
{
    [Test]
    public void map_left_transforms_left_value()
    {
        var e = Either.Left<string, int>("error");
        var result = e.MapLeft(l => l.Length);
        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo(5));
    }

    [Test]
    public void map_left_shortcircuits_on_right()
    {
        var e = Either.Right<string, int>(42);
        var result = e.MapLeft(l => l.Length);
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(42));
    }

    [Test]
    public async Task map_left_async_either()
    {
        var e = Task.FromResult(Either.Left<string, int>("error"));
        var result = await e.MapLeft(l => l.Length);
        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo(5));
    }

    [Test]
    public void bind_left_transforms_left_value()
    {
        var e = Either.Left<string, int>("error");
        var result = e.BindLeft(l => Either.Left<int, int>(l.Length));
        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo(5));
    }

    [Test]
    public void bind_left_shortcircuits_on_right()
    {
        var e = Either.Right<string, int>(42);
        var result = e.BindLeft(l => Either.Left<int, int>(l.Length));
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(42));
    }

    [Test]
    public async Task bind_left_async_either()
    {
        var e = Task.FromResult(Either.Left<string, int>("error"));
        var result = await e.BindLeft(l => Either.Left<int, int>(l.Length));
        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo(5));
    }
}
