namespace FluentSolidity.FunctionalExtensions.Tests;

public class MaybeQuerySyntaxTests
{
    [Test]
    public void sync_query_syntax_with_map()
    {
        var result =
            from v in Maybe.Some(21)
            select v * 2;

        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void sync_query_syntax_with_bind()
    {
        var result =
            from a in Maybe.Some(10)
            from b in Maybe.Some(20)
            select a + b;

        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(30));
    }

    [Test]
    public void sync_query_syntax_shortcircuits_on_none()
    {
        var result =
            from a in Maybe.Some(10)
            from b in Maybe.None<int>()
            select a + b;

        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_query_syntax_with_async_from()
    {
        var result = await (
            from a in Maybe.Some(10)
            from b in Task.FromResult(Maybe.Some(20))
            select a + b);

        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(30));
    }

    [Test]
    public async Task async_query_syntax_with_async_select()
    {
        var result = await (
            from a in Maybe.Some(10)
            from b in Maybe.Some(20)
            select Task.FromResult(a + b));

        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(30));
    }

    [Test]
    public async Task async_query_syntax_with_task_maybe()
    {
        var result = await (
            from a in Task.FromResult(Maybe.Some(10))
            from b in Maybe.Some(20)
            select a + b);

        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(30));
    }

    [Test]
    public void query_syntax_returning_maybe()
    {
        var result =
            from a in Maybe.Some(10)
            from b in Maybe.Some(20)
            select a + b > 25 ? Maybe.Some(a + b) : Maybe.None<int>();

        Assert.That(result.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(30));
    }
}
