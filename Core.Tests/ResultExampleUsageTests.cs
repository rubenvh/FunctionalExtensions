namespace FluentSolidity.FunctionalExtensions.Tests;

/// <summary>
/// Example tests demonstrating typical usage patterns of Result and helpers.
/// These are intentionally simple and serve as living documentation.
/// </summary>
public class ResultExampleUsageTests
{
    [Test]
    public async Task happy_path_chaining_with_map_and_bind()
    {
        Task<Result<int>> CreateAsync(string raw) => Task.FromResult(int.TryParse(raw, out var v)
            ? Result.New(v)
            : Result.Error<int>(Error.Create("ParseError", $"Cannot parse '{raw}'")));

        var final = await CreateAsync("21")
            .Map(v => v * 2)              // 42
            .Bind(v => Result.New(v + 1)) // 43
            .Map(async v => await Task.FromResult(v * 2)); // 86

        Assert.That(final.MatchSuccess(out var value), Is.True, () => final.Error!.ErrorMessage);
        Assert.That(value, Is.EqualTo(86));
    }

    [Test]
    public async Task short_circuiting_stops_execution_on_error()
    {
        var error = Error.Create("BadInput", "Boom");

        var result = Result.New(10)
            .Bind(_ => Result.Error<int>(error)) // becomes error Result<int>
            .Map(v => v * 10) // never executed
            .Match(v => v.ToString(), e => e.ErrorIdentifier);

        Assert.That(result, Is.EqualTo("BadInput"));
    }

    [Test]
    public void flattening_multiple_results_accumulates_validation_messages()
    {
        var inputs = new[] { Result.New(1), Error.Create("E1", "bad"), Result.New(2) };
        var flattened = inputs.FlattenValues(); // always success, errors become validation messages

        Assert.That(flattened.MatchSuccess(out var values), Is.True);
        Assert.That(values, Is.EquivalentTo(new[] {1,2}));
        Assert.That(flattened.ValidationMessages.Length, Is.EqualTo(1));
    }
}