namespace FluentSolidity.FunctionalExtensions.Tests;

public class ValidationConstructionTests
{
    [Test]
    public void valid_matches_valid()
    {
        var v = Validation.Valid<string, int>(42);
        Assert.That(v.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void valid_does_not_match_errors()
    {
        var v = Validation.Valid<string, int>(42);
        Assert.That(v.MatchErrors(out _), Is.False);
    }

    [Test]
    public void invalid_matches_errors()
    {
        var v = Validation.Invalid<string, int>("err1", "err2");
        Assert.That(v.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(2));
        Assert.That(errors[0], Is.EqualTo("err1"));
        Assert.That(errors[1], Is.EqualTo("err2"));
    }

    [Test]
    public void invalid_does_not_match_valid()
    {
        var v = Validation.Invalid<string, int>("err");
        Assert.That(v.MatchValid(out _), Is.False);
    }

    [Test]
    public void invalid_from_list()
    {
        IReadOnlyList<string> errors = new[] { "a", "b" };
        var v = Validation.Invalid<string, int>(errors);
        Assert.That(v.MatchErrors(out var errs), Is.True);
        Assert.That(errs, Has.Count.EqualTo(2));
    }

    [Test]
    public void implicit_conversion_from_value()
    {
        Validation<string, int> v = 42;
        Assert.That(v.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void default_validation_is_valid()
    {
        var v = default(Validation<string, int>);
        Assert.That(v.MatchValid(out _), Is.True);
        Assert.That(v.MatchErrors(out _), Is.False);
    }

    [Test]
    public void tostring_valid_shows_value()
    {
        Assert.That(Validation.Valid<string, int>(42).ToString(), Is.EqualTo("Valid(42)"));
    }

    [Test]
    public void tostring_invalid_shows_errors()
    {
        Assert.That(Validation.Invalid<string, int>("err1", "err2").ToString(), Is.EqualTo("Invalid([err1, err2])"));
    }

    [Test]
    public void valid_errors_collection_is_empty()
    {
        var v = Validation.Valid<string, int>(42);
        Assert.That(v.Errors, Is.Empty);
    }

    [Test]
    public void works_with_error_record_as_terror()
    {
        var err = Error.Create("NOT_FOUND", "Item not found");
        var v = Validation.Invalid<Error, string>(err);
        Assert.That(v.MatchErrors(out var errors), Is.True);
        Assert.That(errors[0].ErrorIdentifier, Is.EqualTo("NOT_FOUND"));
    }
}
