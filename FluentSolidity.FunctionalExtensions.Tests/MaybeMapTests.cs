namespace FluentSolidity.FunctionalExtensions.Tests;

public class MaybeMapTests
{
    [Test]
    public void mapping_some_to_same_type()
    {
        Assert.That(Maybe.Some("value").Map(v => v.ToUpper()).MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo("VALUE"));
    }

    [Test]
    public void mapping_some_to_new_type()
    {
        Assert.That(Maybe.Some("value").Map(v => v.Length).MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo(5));
    }

    [Test]
    public void mapping_none_shortcircuits()
    {
        var mapperCalled = false;
        var result = Maybe.None<string>().Map(v => { mapperCalled = true; return v.ToUpper(); });
        Assert.That(mapperCalled, Is.False);
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_mapping_some_with_async_mapper()
    {
        var result = await Maybe.Some("value").Map(v => Task.FromResult(v.ToUpper()));
        Assert.That(result.MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo("VALUE"));
    }

    [Test]
    public async Task async_mapping_none_with_async_mapper()
    {
        var result = await Maybe.None<string>().Map(v => Task.FromResult(v.ToUpper()));
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task async_maybe_with_sync_mapper()
    {
        var result = await Task.FromResult(Maybe.Some("value")).Map(v => v.ToUpper());
        Assert.That(result.MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo("VALUE"));
    }

    [Test]
    public async Task async_maybe_with_async_mapper()
    {
        var result = await Task.FromResult(Maybe.Some("value")).Map(v => Task.FromResult(v.ToUpper()));
        Assert.That(result.MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo("VALUE"));
    }

    [Test]
    public async Task async_none_with_sync_mapper_shortcircuits()
    {
        var result = await Task.FromResult(Maybe.None<string>()).Map(v => v.ToUpper());
        Assert.That(result.MatchNone(), Is.True);
    }

    [Test]
    public async Task chaining_sync_and_async_maps()
    {
        var result = await Maybe.Some("value")
            .Map(v => v.ToUpper())
            .Map(v => Task.FromResult($"{v}!"))
            .Map(v => v.Length);
        Assert.That(result.MatchSome(out var actual), Is.True);
        Assert.That(actual, Is.EqualTo(6));
    }
}
