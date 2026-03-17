namespace FluentSolidity.FunctionalExtensions.Tests;

public class ValidationCombineTests
{
    #region 2-arity

    [Test]
    public void combine2_both_valid()
    {
        var v1 = Validation.Valid<string, int>(1);
        var v2 = Validation.Valid<string, int>(2);

        var result = v1.Combine(v2, (a, b) => a + b);

        Assert.That(result.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(3));
    }

    [Test]
    public void combine2_first_invalid()
    {
        var v1 = Validation.Invalid<string, int>("err1");
        var v2 = Validation.Valid<string, int>(2);

        var result = v1.Combine(v2, (a, b) => a + b);

        Assert.That(result.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0], Is.EqualTo("err1"));
    }

    [Test]
    public void combine2_second_invalid()
    {
        var v1 = Validation.Valid<string, int>(1);
        var v2 = Validation.Invalid<string, int>("err2");

        var result = v1.Combine(v2, (a, b) => a + b);

        Assert.That(result.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0], Is.EqualTo("err2"));
    }

    [Test]
    public void combine2_both_invalid_accumulates_errors()
    {
        var v1 = Validation.Invalid<string, int>("err1");
        var v2 = Validation.Invalid<string, int>("err2");

        var result = v1.Combine(v2, (a, b) => a + b);

        Assert.That(result.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(2));
        Assert.That(errors[0], Is.EqualTo("err1"));
        Assert.That(errors[1], Is.EqualTo("err2"));
    }

    [Test]
    public void combine2_both_invalid_with_multiple_errors_each()
    {
        var v1 = Validation.Invalid<string, int>("e1", "e2");
        var v2 = Validation.Invalid<string, int>("e3");

        var result = v1.Combine(v2, (a, b) => a + b);

        Assert.That(result.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(3));
        Assert.That(errors, Is.EqualTo(new[] { "e1", "e2", "e3" }));
    }

    [Test]
    public void combine2_combiner_not_called_on_error()
    {
        var combinerCalled = false;
        var v1 = Validation.Invalid<string, int>("err");
        var v2 = Validation.Valid<string, int>(2);

        v1.Combine(v2, (a, b) => { combinerCalled = true; return a + b; });

        Assert.That(combinerCalled, Is.False);
    }

    [Test]
    public async Task combine2_async_combiner()
    {
        var v1 = Validation.Valid<string, int>(1);
        var v2 = Validation.Valid<string, int>(2);

        var result = await v1.Combine(v2, async (a, b) => { await Task.Yield(); return a + b; });

        Assert.That(result.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(3));
    }

    [Test]
    public async Task combine2_async_validations_sync_combiner()
    {
        var v1 = Task.FromResult(Validation.Valid<string, int>(1));
        var v2 = Task.FromResult(Validation.Valid<string, int>(2));

        var result = await v1.Combine(v2, (a, b) => a + b);

        Assert.That(result.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(3));
    }

    [Test]
    public async Task combine2_async_validations_async_combiner()
    {
        var v1 = Task.FromResult(Validation.Valid<string, int>(1));
        var v2 = Task.FromResult(Validation.Invalid<string, int>("err"));

        var result = await v1.Combine(v2, async (a, b) => { await Task.Yield(); return a + b; });

        Assert.That(result.MatchErrors(out var errors), Is.True);
        Assert.That(errors[0], Is.EqualTo("err"));
    }

    #endregion

    #region 3-arity

    [Test]
    public void combine3_all_valid()
    {
        var result = Validation.Valid<string, int>(1)
            .Combine(
                Validation.Valid<string, int>(2),
                Validation.Valid<string, int>(3),
                (a, b, c) => a + b + c);

        Assert.That(result.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(6));
    }

    [Test]
    public void combine3_some_invalid_accumulates()
    {
        var result = Validation.Invalid<string, int>("e1")
            .Combine(
                Validation.Valid<string, int>(2),
                Validation.Invalid<string, int>("e3"),
                (a, b, c) => a + b + c);

        Assert.That(result.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(2));
        Assert.That(errors, Is.EqualTo(new[] { "e1", "e3" }));
    }

    [Test]
    public async Task combine3_async_combiner()
    {
        var result = await Validation.Valid<string, int>(1)
            .Combine(
                Validation.Valid<string, int>(2),
                Validation.Valid<string, int>(3),
                async (a, b, c) => { await Task.Yield(); return a + b + c; });

        Assert.That(result.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(6));
    }

    #endregion

    #region 4-arity

    [Test]
    public void combine4_all_valid()
    {
        var result = Validation.Valid<string, int>(1)
            .Combine(
                Validation.Valid<string, int>(2),
                Validation.Valid<string, int>(3),
                Validation.Valid<string, int>(4),
                (a, b, c, d) => a + b + c + d);

        Assert.That(result.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(10));
    }

    [Test]
    public void combine4_accumulates_errors()
    {
        var result = Validation.Invalid<string, int>("e1")
            .Combine(
                Validation.Invalid<string, int>("e2"),
                Validation.Valid<string, int>(3),
                Validation.Invalid<string, int>("e4"),
                (a, b, c, d) => a + b + c + d);

        Assert.That(result.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(3));
        Assert.That(errors, Is.EqualTo(new[] { "e1", "e2", "e4" }));
    }

    #endregion

    #region 5-arity

    [Test]
    public void combine5_all_valid()
    {
        var result = Validation.Valid<string, int>(1)
            .Combine(
                Validation.Valid<string, int>(2),
                Validation.Valid<string, int>(3),
                Validation.Valid<string, int>(4),
                Validation.Valid<string, int>(5),
                (a, b, c, d, e) => a + b + c + d + e);

        Assert.That(result.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(15));
    }

    [Test]
    public void combine5_accumulates_errors()
    {
        var result = Validation.Invalid<string, int>("e1")
            .Combine(
                Validation.Valid<string, int>(2),
                Validation.Invalid<string, int>("e3"),
                Validation.Valid<string, int>(4),
                Validation.Invalid<string, int>("e5"),
                (a, b, c, d, e) => a + b + c + d + e);

        Assert.That(result.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(3));
        Assert.That(errors, Is.EqualTo(new[] { "e1", "e3", "e5" }));
    }

    #endregion

    #region 6-arity

    [Test]
    public void combine6_all_valid()
    {
        var result = Validation.Valid<string, int>(1)
            .Combine(
                Validation.Valid<string, int>(2),
                Validation.Valid<string, int>(3),
                Validation.Valid<string, int>(4),
                Validation.Valid<string, int>(5),
                Validation.Valid<string, int>(6),
                (a, b, c, d, e, f) => a + b + c + d + e + f);

        Assert.That(result.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(21));
    }

    [Test]
    public void combine6_accumulates_all_errors()
    {
        var result = Validation.Invalid<string, int>("e1")
            .Combine(
                Validation.Invalid<string, int>("e2"),
                Validation.Invalid<string, int>("e3"),
                Validation.Invalid<string, int>("e4"),
                Validation.Invalid<string, int>("e5"),
                Validation.Invalid<string, int>("e6"),
                (a, b, c, d, e, f) => a + b + c + d + e + f);

        Assert.That(result.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(6));
        Assert.That(errors, Is.EqualTo(new[] { "e1", "e2", "e3", "e4", "e5", "e6" }));
    }

    #endregion

    #region Mixed types

    [Test]
    public void combine_with_different_value_types()
    {
        var name = Validation.Valid<string, string>("Alice");
        var age = Validation.Valid<string, int>(30);

        var result = name.Combine(age, (n, a) => $"{n} is {a}");

        Assert.That(result.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo("Alice is 30"));
    }

    [Test]
    public void combine_with_error_record()
    {
        var name = Validation.Valid<Error, string>("Alice");
        var age = Validation.Invalid<Error, int>(Error.Create("AGE", "Age is required"));

        var result = name.Combine(age, (n, a) => $"{n} is {a}");

        Assert.That(result.MatchErrors(out var errors), Is.True);
        Assert.That(errors[0].ErrorIdentifier, Is.EqualTo("AGE"));
    }

    #endregion
}
