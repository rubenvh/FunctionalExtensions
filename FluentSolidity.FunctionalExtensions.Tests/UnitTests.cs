namespace FluentSolidity.FunctionalExtensions.Tests;

public class UnitTests
{
    [Test]
    public void unit_value_equals_default()
    {
        Assert.That(Unit.Value, Is.EqualTo(default(Unit)));
    }

    [Test]
    public void unit_equals_unit()
    {
        Assert.That(Unit.Value.Equals(Unit.Value), Is.True);
    }

    [Test]
    public void unit_equals_boxed_unit()
    {
        object boxed = Unit.Value;
        Assert.That(Unit.Value.Equals(boxed), Is.True);
    }

    [Test]
    public void unit_does_not_equal_non_unit()
    {
        Assert.That(Unit.Value.Equals("hello"), Is.False);
    }

    [Test]
    public void unit_equality_operators()
    {
        var a = Unit.Value;
        var b = default(Unit);
        Assert.That(a == b, Is.True);
        Assert.That(a != b, Is.False);
    }

    [Test]
    public void unit_hashcode_is_zero()
    {
        Assert.That(Unit.Value.GetHashCode(), Is.EqualTo(0));
    }

    [Test]
    public void unit_tostring_is_parentheses()
    {
        Assert.That(Unit.Value.ToString(), Is.EqualTo("()"));
    }

    [Test]
    public void unit_compareto_is_zero()
    {
        Assert.That(Unit.Value.CompareTo(Unit.Value), Is.EqualTo(0));
    }

    [Test]
    public void unit_implicit_conversion_to_valuetuple()
    {
        ValueTuple vt = Unit.Value;
        Assert.That(vt, Is.EqualTo(default(ValueTuple)));
    }

    [Test]
    public void unit_implicit_conversion_from_valuetuple()
    {
        Unit u = default(ValueTuple);
        Assert.That(u, Is.EqualTo(Unit.Value));
    }

    [Test]
    public async Task unit_task_is_completed()
    {
        var result = await Unit.Task;
        Assert.That(result, Is.EqualTo(Unit.Value));
    }

    [Test]
    public void unit_works_with_result()
    {
        var result = Result.New(Unit.Value);
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(Unit.Value));
    }

    [Test]
    public void unit_works_with_error_result()
    {
        var error = Error.Create("ERR", "fail");
        var result = Result.Error<Unit>(error);
        Assert.That(result.MatchError(out var e), Is.True);
        Assert.That(e, Is.EqualTo(error));
    }

    [Test]
    public void unit_works_with_either()
    {
        var either = Either.Right<string, Unit>(Unit.Value);
        Assert.That(either.MatchRight(out var value), Is.True);
        Assert.That(value, Is.EqualTo(Unit.Value));
    }
}
