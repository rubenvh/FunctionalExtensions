namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultConstructionTests : ResultTestBase
{
    [Test]
    public void successful_result_matches_success()
    {
        Assert.That(Result.New("success").MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("success"));
    }

    [Test]
    public void successful_result_does_not_match_error()
    {
        Assert.That(Result.New("success").MatchError(out _), Is.False);
    }

    [Test]
    public void error_result_matches_error()
    {
        Assert.That(Result.Error<string>(someError).MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo(someError.ErrorIdentifier));
        Assert.That(error.ErrorMessage, Is.EqualTo(someError.ErrorMessage));
    }

    [Test]
    public void error_result_does_not_match_success()
    {
        Assert.That(Result.Error<string>(someError).MatchSuccess(out _), Is.False);
    }

    [Test]
    public void implicit_conversion_from_error_works()
    {
        Result<string> result = someError;
        Assert.That(result.MatchError(out var e), Is.True);
        Assert.That(e, Is.EqualTo(someError));
    }

    [Test]
    public void implicit_conversion_from_value_works()
    {
        Result<string> result = "success";
        Assert.That(result.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo("success"));
    }

    [Test]
    public void default_result_matches_success_not_error()
    {
        var result = default(Result<string>);

        Assert.That(result.MatchSuccess(out _), Is.True);
        Assert.That(result.MatchError(out _), Is.False);
    }

    [Test]
    public void default_result_value_is_default_of_T()
    {
        var result = default(Result<int>);

        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(0));
    }

    [Test]
    public void explicit_error_constructor_always_matches_error()
    {
        var error = Error.Create("test", "msg");
        var result = new Result<string>(error);

        Assert.That(result.MatchError(out var e), Is.True);
        Assert.That(e, Is.EqualTo(error));
        Assert.That(result.MatchSuccess(out _), Is.False);
    }

    [Test]
    public void explicit_success_constructor_always_matches_success()
    {
        var result = new Result<string>("hello");

        Assert.That(result.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo("hello"));
        Assert.That(result.MatchError(out _), Is.False);
    }

    [Test]
    public void with_messages_preserves_error_state()
    {
        var error = Error.Create("test", "msg");
        var result = new Result<string>(error);
        var warning = new PipelineMessage("w1", MessageLevel.Warning, "warn");

        var resultWithMessages = result.WithMessages(warning);

        Assert.That(resultWithMessages.MatchError(out _), Is.True);
        Assert.That(resultWithMessages.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public void with_messages_preserves_success_state()
    {
        var result = Result.New("hello");
        var warning = new PipelineMessage("w1", MessageLevel.Warning, "warn");

        var resultWithMessages = result.WithMessages(warning);

        Assert.That(resultWithMessages.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo("hello"));
        Assert.That(resultWithMessages.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public void tostring_success_shows_value()
    {
        var result = Result.New(42);
        Assert.That(result.ToString(), Is.EqualTo("Success(42)"));
    }

    [Test]
    public void tostring_error_shows_identifier_and_message()
    {
        var result = Result.Error<int>(new Error("NOT_FOUND", "Item not found"));
        Assert.That(result.ToString(), Is.EqualTo("Error(NOT_FOUND: Item not found)"));
    }

    [Test]
    public void tostring_success_null_value()
    {
        var result = Result.New<string?>(null);
        Assert.That(result.ToString(), Is.EqualTo("Success()"));
    }

    [Test]
    public void json_deserialize_error_will_work_net_version()
    {
        var error = System.Text.Json.JsonSerializer.Deserialize<Error>(
            "{\"ErrorIdentifier\":\"GenericError\",\"ErrorMessage\":\"Cannot interpret id string as a valid numerical identifier.\", \"Context\":\"some context\" }");

        Assert.That(error.ErrorIdentifier, Is.EqualTo("GenericError"));
        Assert.That(error.ErrorMessage, Is.EqualTo("Cannot interpret id string as a valid numerical identifier."));
        Assert.That(error.Context, Is.EqualTo("some context"));
    }

    [Test]
    public async Task exception_is_translated_to_error_result()
    {
        var exMessage = "I throw some exception";
        var result = await WrapExceptionHelper(exMessage);
        Assert.That(result.MatchError(out var e), Is.True);
        Assert.That(e.ErrorIdentifier, Is.EqualTo("UnhandledException"));
        Assert.That(e.ErrorMessage, Is.EqualTo(exMessage));
    }
}
