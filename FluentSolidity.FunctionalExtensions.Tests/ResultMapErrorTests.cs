namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultMapErrorTests : ResultTestBase
{
    [Test]
    public void mapping_error_result_matches_error()
    {
        var mappedError = Result.Error<string>(someError).MapError(e => Error.Create("identifier", "data"));

        Assert.That(mappedError.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo("identifier"));
        Assert.That(error.ErrorMessage, Is.EqualTo("data"));
    }

    [Test]
    public void mapping_error_on_successful_result_matches_success()
    {
        var unmodifiedResult = Result.New("value")
            .MapError(e => Error.Create("identifier", "data"));

        Assert.That(unmodifiedResult.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo("value"));
    }

    [Test]
    public async Task async_mapping_error_on_error_result_matches_mapped_Error()
    {
        var actual = await Result.Error<string>(someError)
            .Map(v => Task.FromResult(v.ToUpper()))
            .MapError(e => Error.Create(e.ErrorIdentifier, e.ErrorMessage));

        Assert.That(actual.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo(someError.ErrorIdentifier));
        Assert.That(error.ErrorMessage, Is.EqualTo(someError.ErrorMessage));
    }
}
