namespace FluentSolidity.FunctionalExtensions.Tests;

public class EitherConstructionTests
{
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

    [Test]
    public void tostring_right_shows_right_value()
    {
        var e = Either.Right<string, int>(42);
        Assert.That(e.ToString(), Is.EqualTo("Right(42)"));
    }

    [Test]
    public void tostring_left_shows_left_value()
    {
        var e = Either.Left<string, int>("error");
        Assert.That(e.ToString(), Is.EqualTo("Left(error)"));
    }

    [Test]
    public void tostring_left_of_null_shows_left_null()
    {
        var e = Either.Left<string?, int>(null);
        Assert.That(e.ToString(), Is.EqualTo("Left()"));
    }

    [Test]
    public void tostring_right_of_null_shows_right_null()
    {
        var e = Either.Right<int, string?>(null);
        Assert.That(e.ToString(), Is.EqualTo("Right()"));
    }
}
