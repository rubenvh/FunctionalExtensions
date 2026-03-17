namespace FluentSolidity.FunctionalExtensions.Tests;

public class MaybeMatchTests
{
    [Test]
    public void match_some_invokes_some_mapper()
    {
        var result = Maybe.Some("hello").Match(v => v.ToUpper(), () => "nothing");
        Assert.That(result, Is.EqualTo("HELLO"));
    }

    [Test]
    public void match_none_invokes_none_mapper()
    {
        var result = Maybe.None<string>().Match(v => v.ToUpper(), () => "nothing");
        Assert.That(result, Is.EqualTo("nothing"));
    }

    [Test]
    public async Task async_match_sync_maybe_async_mappers()
    {
        var result = await Maybe.Some("hello").Match(
            v => Task.FromResult(v.ToUpper()),
            () => Task.FromResult("nothing"));
        Assert.That(result, Is.EqualTo("HELLO"));
    }

    [Test]
    public async Task async_match_sync_maybe_none_async_mappers()
    {
        var result = await Maybe.None<string>().Match(
            v => Task.FromResult(v.ToUpper()),
            () => Task.FromResult("nothing"));
        Assert.That(result, Is.EqualTo("nothing"));
    }

    [Test]
    public async Task async_match_async_maybe_sync_mappers()
    {
        var result = await Task.FromResult(Maybe.Some("hello")).Match(
            v => v.ToUpper(),
            () => "nothing");
        Assert.That(result, Is.EqualTo("HELLO"));
    }

    [Test]
    public async Task async_match_async_maybe_async_mappers()
    {
        var result = await Task.FromResult(Maybe.Some("hello")).Match(
            v => Task.FromResult(v.ToUpper()),
            () => Task.FromResult("nothing"));
        Assert.That(result, Is.EqualTo("HELLO"));
    }

    [Test]
    public void match_collapses_to_different_type()
    {
        var result = Maybe.Some(42).Match(v => v > 0, () => false);
        Assert.That(result, Is.True);
    }
}
