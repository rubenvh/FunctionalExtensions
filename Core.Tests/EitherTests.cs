using System.Globalization;

namespace FluentSolidity.FunctionalExtensions.Tests;

public class EitherTests
{
    #region Construction & Tag-Based Discrimination

    [Test]
    public void left_matches_left()
    {
        var e = Either.Left<string, object>("error");
        Assert.That(e.MatchLeft(out var _), Is.True);
        Assert.That(e.MatchRight(out var _), Is.False);
    }

    [Test]
    public void right_matches_right()
    {
        var e = Either.Right<string, int>(0);
        Assert.That(e.MatchLeft(out var _), Is.False);
        Assert.That(e.MatchRight(out var _), Is.True);
    }

    [Test]
    public void left_of_zero_is_still_left()
    {
        // This was the old bug: null-based discriminator treated Left(0) as Right
        var e = Either.Left<int, string>(0);
        Assert.That(e.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo(0));
    }

    [Test]
    public void left_of_false_is_still_left()
    {
        var e = Either.Left<bool, string>(false);
        Assert.That(e.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.False);
    }

    [Test]
    public void left_of_null_reference_is_still_left()
    {
        var e = Either.Left<string?, int>(null);
        Assert.That(e.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.Null);
    }

    [Test]
    public void right_of_zero_is_still_right()
    {
        var e = Either.Right<string, int>(0);
        Assert.That(e.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(0));
    }

    [Test]
    public void right_of_null_reference_is_still_right()
    {
        var e = Either.Right<int, string?>(null);
        Assert.That(e.MatchRight(out var right), Is.True);
        Assert.That(right, Is.Null);
    }

    #endregion

    #region Implicit Conversions

    [Test]
    public void implicit_conversion_from_left_type()
    {
        Either<string, int> e = "error";
        Assert.That(e.MatchLeft(out var left), Is.True);
        Assert.That(left, Is.EqualTo("error"));
    }

    [Test]
    public void implicit_conversion_from_right_type()
    {
        Either<string, int> e = 42;
        Assert.That(e.MatchRight(out var right), Is.True);
        Assert.That(right, Is.EqualTo(42));
    }

    #endregion

    #region Map / Bind chains (sync)

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

    #endregion

    #region Match (fold)

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

    #endregion

    #region Async Map

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

    #endregion

    #region Async Bind

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

    #endregion

    #region MapLeft / BindLeft

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

    #endregion

    #region Do

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

    #endregion

    #region Query Syntax (LINQ)

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

    #endregion
}
