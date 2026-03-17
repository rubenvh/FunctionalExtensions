namespace FluentSolidity.FunctionalExtensions.Tests;

public class ValidationMatchTests
{
    [Test]
    public void match_valid_invokes_valid_mapper()
    {
        var result = Validation.Valid<string, int>(42)
            .Match(v => v * 2, errors => -1);
        Assert.That(result, Is.EqualTo(84));
    }

    [Test]
    public void match_invalid_invokes_error_mapper()
    {
        var result = Validation.Invalid<string, int>("err")
            .Match(v => v * 2, errors => errors.Count);
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task async_match_sync_validation_async_mappers()
    {
        var result = await Validation.Valid<string, int>(42)
            .Match(
                v => Task.FromResult(v * 2),
                errors => Task.FromResult(-1));
        Assert.That(result, Is.EqualTo(84));
    }

    [Test]
    public async Task async_match_async_validation_sync_mappers()
    {
        var result = await Task.FromResult(Validation.Valid<string, int>(42))
            .Match(v => v * 2, errors => -1);
        Assert.That(result, Is.EqualTo(84));
    }

    [Test]
    public async Task async_match_async_validation_async_mappers()
    {
        var result = await Task.FromResult(Validation.Valid<string, int>(42))
            .Match(
                v => Task.FromResult(v * 2),
                errors => Task.FromResult(-1));
        Assert.That(result, Is.EqualTo(84));
    }

    [Test]
    public void match_collapses_errors_to_string()
    {
        var result = Validation.Invalid<string, int>("a", "b")
            .Match(v => "ok", errors => string.Join(",", errors));
        Assert.That(result, Is.EqualTo("a,b"));
    }
}
