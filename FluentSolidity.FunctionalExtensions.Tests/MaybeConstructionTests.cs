namespace FluentSolidity.FunctionalExtensions.Tests;

public class MaybeConstructionTests
{
    [Test]
    public void some_matches_some()
    {
        Assert.That(Maybe.Some("hello").MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo("hello"));
    }

    [Test]
    public void some_does_not_match_none()
    {
        Assert.That(Maybe.Some("hello").MatchNone(), Is.False);
    }

    [Test]
    public void none_matches_none()
    {
        Assert.That(Maybe.None<string>().MatchNone(), Is.True);
    }

    [Test]
    public void none_does_not_match_some()
    {
        Assert.That(Maybe.None<string>().MatchSome(out _), Is.False);
    }

    [Test]
    public void implicit_conversion_from_value_works()
    {
        Maybe<string> maybe = "hello";
        Assert.That(maybe.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo("hello"));
    }

    [Test]
    public void default_maybe_is_none()
    {
        var maybe = default(Maybe<string>);
        Assert.That(maybe.MatchNone(), Is.True);
        Assert.That(maybe.MatchSome(out _), Is.False);
    }

    [Test]
    public void default_maybe_of_value_type_is_none()
    {
        var maybe = default(Maybe<int>);
        Assert.That(maybe.MatchNone(), Is.True);
    }

    [Test]
    public void some_of_zero_is_still_some()
    {
        var maybe = Maybe.Some(0);
        Assert.That(maybe.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(0));
    }

    [Test]
    public void some_of_false_is_still_some()
    {
        var maybe = Maybe.Some(false);
        Assert.That(maybe.MatchSome(out var value), Is.True);
        Assert.That(value, Is.False);
    }

    [Test]
    public void some_of_null_reference_is_still_some()
    {
        var maybe = Maybe.Some<string?>(null);
        Assert.That(maybe.MatchSome(out var value), Is.True);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void tostring_some_shows_value()
    {
        Assert.That(Maybe.Some(42).ToString(), Is.EqualTo("Some(42)"));
    }

    [Test]
    public void tostring_none_shows_none()
    {
        Assert.That(Maybe.None<int>().ToString(), Is.EqualTo("None"));
    }

    [Test]
    public void tostring_some_null_shows_some_empty()
    {
        Assert.That(Maybe.Some<string?>(null).ToString(), Is.EqualTo("Some()"));
    }

    [Test]
    public void static_none_property_returns_none()
    {
        var maybe = Maybe<string>.None;
        Assert.That(maybe.MatchNone(), Is.True);
    }
}
