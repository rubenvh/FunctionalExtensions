namespace FluentSolidity.FunctionalExtensions.Tests;

[TestFixture]
public class InteropConversionTests
{
    private static readonly Error TestError = Error.Create("TEST_ERR", "Something went wrong");

    #region Maybe -> Result

    [Test]
    public void MaybeToResult_Some_WithError_ReturnsSuccess()
    {
        var maybe = Maybe.Some(42);

        var result = maybe.ToResult(TestError);

        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void MaybeToResult_None_WithError_ReturnsError()
    {
        var maybe = Maybe.None<int>();

        var result = maybe.ToResult(TestError);

        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo("TEST_ERR"));
    }

    [Test]
    public void MaybeToResult_Some_WithErrorFactory_ReturnsSuccess()
    {
        var maybe = Maybe.Some("hello");

        var result = maybe.ToResult(() => TestError);

        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("hello"));
    }

    [Test]
    public void MaybeToResult_None_WithErrorFactory_ReturnsError()
    {
        var maybe = Maybe.None<string>();

        var result = maybe.ToResult(() => Error.Create("LAZY_ERR", "Lazy error"));

        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo("LAZY_ERR"));
    }

    [Test]
    public async Task MaybeToResult_AsyncMaybe_WithError_Some()
    {
        var maybe = Task.FromResult(Maybe.Some(42));

        var result = await maybe.ToResult(TestError);

        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public async Task MaybeToResult_AsyncMaybe_WithError_None()
    {
        var maybe = Task.FromResult(Maybe.None<int>());

        var result = await maybe.ToResult(TestError);

        Assert.That(result.MatchError(out _), Is.True);
    }

    [Test]
    public async Task MaybeToResult_AsyncMaybe_WithErrorFactory_Some()
    {
        var maybe = Task.FromResult(Maybe.Some("async"));

        var result = await maybe.ToResult(() => TestError);

        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("async"));
    }

    [Test]
    public async Task MaybeToResult_AsyncMaybe_WithErrorFactory_None()
    {
        var maybe = Task.FromResult(Maybe.None<string>());

        var result = await maybe.ToResult(() => TestError);

        Assert.That(result.MatchError(out _), Is.True);
    }

    #endregion

    #region Result -> Maybe

    [Test]
    public void ResultToMaybe_Success_ReturnsSome()
    {
        var result = Result.New(42);

        var maybe = result.ToMaybe();

        Assert.That(maybe.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void ResultToMaybe_Error_ReturnsNone()
    {
        var result = Result.Error<int>(TestError);

        var maybe = result.ToMaybe();

        Assert.That(maybe.MatchNone(), Is.True);
    }

    [Test]
    public async Task ResultToMaybe_AsyncSuccess_ReturnsSome()
    {
        var result = Task.FromResult(Result.New("hello"));

        var maybe = await result.ToMaybe();

        Assert.That(maybe.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo("hello"));
    }

    [Test]
    public async Task ResultToMaybe_AsyncError_ReturnsNone()
    {
        var result = Task.FromResult(Result.Error<string>(TestError));

        var maybe = await result.ToMaybe();

        Assert.That(maybe.MatchNone(), Is.True);
    }

    #endregion

    #region Validation -> Result

    [Test]
    public void ValidationToResult_Valid_ReturnsSuccess()
    {
        var validation = Validation.Valid<string, int>(42);

        var result = validation.ToResult(errors => Error.Create("VAL_ERR", string.Join(", ", errors)));

        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void ValidationToResult_Invalid_ReturnsError()
    {
        var validation = Validation.Invalid<string, int>("err1", "err2");

        var result = validation.ToResult(errors => Error.Create("VAL_ERR", string.Join(", ", errors)));

        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo("VAL_ERR"));
        Assert.That(error.ErrorMessage, Is.EqualTo("err1, err2"));
    }

    [Test]
    public async Task ValidationToResult_AsyncValid_ReturnsSuccess()
    {
        var validation = Task.FromResult(Validation.Valid<string, int>(42));

        var result = await validation.ToResult(errors => Error.Create("VAL_ERR", string.Join(", ", errors)));

        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public async Task ValidationToResult_AsyncInvalid_ReturnsError()
    {
        var validation = Task.FromResult(Validation.Invalid<string, int>("e1"));

        var result = await validation.ToResult(errors => Error.Create("VAL_ERR", string.Join(", ", errors)));

        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorMessage, Is.EqualTo("e1"));
    }

    #endregion

    #region Result -> Validation

    [Test]
    public void ResultToValidation_Success_ReturnsValid()
    {
        var result = Result.New(42);

        var validation = result.ToValidation(err => err.ErrorMessage);

        Assert.That(validation.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void ResultToValidation_Error_ReturnsInvalid()
    {
        var result = Result.Error<int>(TestError);

        var validation = result.ToValidation(err => err.ErrorMessage);

        Assert.That(validation.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0], Is.EqualTo("Something went wrong"));
    }

    [Test]
    public async Task ResultToValidation_AsyncSuccess_ReturnsValid()
    {
        var result = Task.FromResult(Result.New("hello"));

        var validation = await result.ToValidation(err => err.ErrorMessage);

        Assert.That(validation.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo("hello"));
    }

    [Test]
    public async Task ResultToValidation_AsyncError_ReturnsInvalid()
    {
        var result = Task.FromResult(Result.Error<string>(TestError));

        var validation = await result.ToValidation(err => err.ErrorMessage);

        Assert.That(validation.MatchErrors(out var errors), Is.True);
        Assert.That(errors[0], Is.EqualTo("Something went wrong"));
    }

    #endregion

    #region Maybe -> Validation

    [Test]
    public void MaybeToValidation_Some_ReturnsValid()
    {
        var maybe = Maybe.Some(42);

        var validation = maybe.ToValidation(() => "missing");

        Assert.That(validation.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void MaybeToValidation_None_ReturnsInvalid()
    {
        var maybe = Maybe.None<int>();

        var validation = maybe.ToValidation(() => "missing value");

        Assert.That(validation.MatchErrors(out var errors), Is.True);
        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0], Is.EqualTo("missing value"));
    }

    [Test]
    public async Task MaybeToValidation_AsyncSome_ReturnsValid()
    {
        var maybe = Task.FromResult(Maybe.Some(42));

        var validation = await maybe.ToValidation(() => "missing");

        Assert.That(validation.MatchValid(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public async Task MaybeToValidation_AsyncNone_ReturnsInvalid()
    {
        var maybe = Task.FromResult(Maybe.None<int>());

        var validation = await maybe.ToValidation(() => "missing value");

        Assert.That(validation.MatchErrors(out var errors), Is.True);
        Assert.That(errors[0], Is.EqualTo("missing value"));
    }

    #endregion

    #region Round-trip conversions

    [Test]
    public void MaybeToResultToMaybe_Some_RoundTrips()
    {
        var original = Maybe.Some(42);

        var roundTripped = original.ToResult(TestError).ToMaybe();

        Assert.That(roundTripped.MatchSome(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void MaybeToResultToMaybe_None_RoundTrips()
    {
        var original = Maybe.None<int>();

        var roundTripped = original.ToResult(TestError).ToMaybe();

        Assert.That(roundTripped.MatchNone(), Is.True);
    }

    [Test]
    public void ResultToMaybeToResult_Success_RoundTrips()
    {
        var original = Result.New(42);

        var roundTripped = original.ToMaybe().ToResult(TestError);

        Assert.That(roundTripped.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void ResultToMaybeToResult_Error_LosesErrorInfo()
    {
        var original = Result.Error<int>(Error.Create("ORIG", "Original error"));

        var roundTripped = original.ToMaybe().ToResult(Error.Create("NEW", "New error"));

        // Error info is lost when going through Maybe (None has no error details)
        Assert.That(roundTripped.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo("NEW"));
    }

    [Test]
    public void MaybeToValidationToResult_Some_Converts()
    {
        var maybe = Maybe.Some(42);

        var result = maybe
            .ToValidation(() => "missing")
            .ToResult(errors => Error.Create("VAL_ERR", string.Join(", ", errors)));

        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void MaybeToValidationToResult_None_Converts()
    {
        var maybe = Maybe.None<int>();

        var result = maybe
            .ToValidation(() => "missing")
            .ToResult(errors => Error.Create("VAL_ERR", string.Join(", ", errors)));

        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorMessage, Is.EqualTo("missing"));
    }

    #endregion
}
