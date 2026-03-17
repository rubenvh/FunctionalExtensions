namespace FluentSolidity.FunctionalExtensions.Tests;

public class EitherQuerySyntaxTests
{
    [Test]
    public void query_syntax_select_on_right()
    {
        var e = Either.Right<string, int>(5);
        var result =
            from x in e
            select x * 2;
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(10));
    }

    [Test]
    public void query_syntax_select_shortcircuits_on_left()
    {
        var e = Either.Left<string, int>("fail");
        var result =
            from x in e
            select x * 2;
        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("fail"));
    }

    [Test]
    public void query_syntax_selectmany_two_froms()
    {
        var a = Either.Right<string, int>(3);
        var b = Either.Right<string, int>(4);

        var result =
            from x in a
            from y in b
            select x + y;

        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(7));
    }

    [Test]
    public void query_syntax_selectmany_shortcircuits_on_left()
    {
        var a = Either.Right<string, int>(3);
        var b = Either.Left<string, int>("fail");

        var result =
            from x in a
            from y in b
            select x + y;

        Assert.That(result.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("fail"));
    }

    [Test]
    public async Task query_syntax_async_select()
    {
        var e = Task.FromResult(Either.Right<string, int>(5));
        var result = await (
            from x in e
            select x * 2);
        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(10));
    }

    [Test]
    public async Task query_syntax_async_selectmany()
    {
        var a = Task.FromResult(Either.Right<string, int>(3));

        var result = await (
            from x in a
            from y in Either.Right<string, int>(4)
            select x + y);

        Assert.That(result.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(7));
    }
}
