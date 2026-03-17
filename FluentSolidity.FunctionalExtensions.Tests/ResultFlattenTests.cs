namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultFlattenTests : ResultTestBase
{
    [Test]
    public void flattening_ienumerable_of_results_with_error_works()
    {
        var input = new[] { "1", "2", "3", "triggersError", "5" };

        var result = input.Select(i => DoubleStringResult(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : someError))
            .FlattenResults(someError.ErrorIdentifier);

        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo(someError.ErrorIdentifier));
        Assert.That(error.ErrorMessage, Is.EqualTo(someError.ErrorMessage));
    }

    [Test]
    public void flattening_ienumerable_of_results_with_multiple_errors_works()
    {
        var input = new[] { "1", "x", "3", "y", "5", "x", "z" };

        var result = input.Select(i => new Result<string>(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : Error.Create(someError.ErrorIdentifier, $"cannot parse {i}")))
            .FlattenResults(someError.ErrorIdentifier, "someMessage");

        Assert.That(result.MatchError(out var error), Is.True);
        Assert.That(error.ErrorIdentifier, Is.EqualTo(someError.ErrorIdentifier));
        Assert.That(error.ErrorMessage, Is.EqualTo("someMessage"));
        Assert.That(result.Messages, Has.Count.EqualTo(3));
        var messages = result.Messages.Select(m => m.Message).ToArray();
        Assert.That(messages, Does.Contain("cannot parse x"));
        Assert.That(messages, Does.Contain("cannot parse y"));
        Assert.That(messages, Does.Contain("cannot parse z"));
    }

    [Test]
    public void flattening_ienumerable_of_results_without_error_works()
    {
        var input = new[] { "1", "2", "3", "4", "5" };

        var result = input.Select(i => DoubleStringResult(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : someError))
            .FlattenResults(someError.ErrorIdentifier)
            .Map(x => x.Sum());

        Assert.That(result.MatchSuccess(out var sum), Is.True);
        Assert.That(sum, Is.EqualTo(11 + 22 + 33 + 44 + 55));
    }

    [Test]
    public void flattening_values_ienumerable_of_results_with_error_works()
    {
        var input = new[] { "1", "2", "3", "triggersError", "5" };

        var result = input.Select(i => DoubleStringResult(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : someError))
            .FlattenValues()
            .Map(x => x.Sum());

        Assert.That(result.MatchSuccess(out var sum), Is.True);
        Assert.That(sum, Is.EqualTo(11 + 22 + 33 + 55));
        Assert.That(result.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public void flattening_values_ienumerable_of_results_with_multiple_errors_works()
    {
        var input = new[] { "1", "x", "3", "y", "5", "x", "z" };

        var result = input.Select(i => new Result<string>(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : Error.Create(someError.ErrorIdentifier, $"cannot parse {i}")))
            .FlattenValues()
            .Map(x => x.Sum());

        Assert.That(result.MatchSuccess(out var sum), Is.True);
        Assert.That(sum, Is.EqualTo(1 + 3 + 5));
        Assert.That(result.Messages, Has.Count.EqualTo(3));
    }

    [Test]
    public void flattening_values_ienumerable_of_results_with_all_errors_works()
    {
        var input = new[] { "x", "y", "x", "z" };

        var result = input.Select(i => new Result<string>(i)
                .Bind<string, int>(i =>
                    int.TryParse(i, out var result)
                        ? result
                        : Error.Create(someError.ErrorIdentifier, $"cannot parse {i}")))
            .FlattenValues();

        Assert.That(result.MatchSuccess(out var parsedValues), Is.True);
        Assert.That(parsedValues, Is.Empty);

        Assert.That(result.Messages, Has.Count.EqualTo(3));
    }

    [Test]
    public void flattening_values_ienumerable_of_results_without_error_works()
    {
        var input = new[] { "1", "2", "3", "4", "5" };

        var result = input.Select(i => DoubleStringResult(i)
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : someError))
            .FlattenValues()
            .Map(x => x.Sum());

        Assert.That(result.MatchSuccess(out var sum), Is.True);
        Assert.That(sum, Is.EqualTo(11 + 22 + 33 + 44 + 55));
        Assert.That(result.Messages, Has.Count.EqualTo(0));
    }

    /// <summary>
    /// Example: FlattenValues accumulates errors as pipeline messages,
    /// returning only the successful values.
    /// </summary>
    [Test]
    public void flattening_multiple_results_accumulates_pipeline_messages()
    {
        var inputs = new[] { Result.New(1), Error.Create("E1", "bad"), Result.New(2) };
        var flattened = inputs.FlattenValues(); // always success, errors become pipeline messages

        Assert.That(flattened.MatchSuccess(out var values), Is.True);
        Assert.That(values, Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(flattened.Messages.Count, Is.EqualTo(1));
    }
}
