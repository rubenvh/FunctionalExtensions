namespace FluentSolidity.FunctionalExtensions.Tests;

public class MaybeOrDefaultOrElseTests
{
    [Test]
    public void ordefault_returns_value_on_some()
    {
        Assert.That(Maybe.Some(42).OrDefault(), Is.EqualTo(42));
    }

    [Test]
    public void ordefault_returns_default_on_none()
    {
        Assert.That(Maybe.None<int>().OrDefault(), Is.EqualTo(0));
    }

    [Test]
    public void ordefault_returns_null_on_none_reference_type()
    {
        Assert.That(Maybe.None<string>().OrDefault(), Is.Null);
    }

    [Test]
    public void orelse_returns_value_on_some()
    {
        Assert.That(Maybe.Some(42).OrElse(() => 99), Is.EqualTo(42));
    }

    [Test]
    public void orelse_returns_fallback_on_none()
    {
        Assert.That(Maybe.None<int>().OrElse(() => 99), Is.EqualTo(99));
    }

    [Test]
    public void orelse_does_not_invoke_fallback_on_some()
    {
        var fallbackCalled = false;
        Maybe.Some(42).OrElse(() => { fallbackCalled = true; return 99; });
        Assert.That(fallbackCalled, Is.False);
    }

    [Test]
    public async Task async_orelse_async_maybe_sync_fallback()
    {
        var result = await Task.FromResult(Maybe.None<int>()).OrElse(() => 99);
        Assert.That(result, Is.EqualTo(99));
    }

    [Test]
    public async Task async_orelse_sync_maybe_async_fallback()
    {
        var result = await Maybe.None<int>().OrElse(() => Task.FromResult(99));
        Assert.That(result, Is.EqualTo(99));
    }

    [Test]
    public async Task async_orelse_async_maybe_async_fallback()
    {
        var result = await Task.FromResult(Maybe.None<int>()).OrElse(() => Task.FromResult(99));
        Assert.That(result, Is.EqualTo(99));
    }

    [Test]
    public async Task async_orelse_some_does_not_invoke_async_fallback()
    {
        var fallbackCalled = false;
        var result = await Maybe.Some(42).OrElse(async () => { fallbackCalled = true; return await Task.FromResult(99); });
        Assert.That(result, Is.EqualTo(42));
        Assert.That(fallbackCalled, Is.False);
    }
}
