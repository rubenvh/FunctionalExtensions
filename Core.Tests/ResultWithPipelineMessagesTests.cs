using System.Globalization;

namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultWithPipelineMessagesTests
{
    private Error someError = null!;

    [SetUp]
    public void Setup()
    {
        someError = Error.Create("error", "message");
    }

    [Test]
    public void successful_result_without_warnings()
    {
        var result = DoubleString("a")
            .Map(r => r.ToUpper())
            .Bind(r => DoubleString(r));
        
        Assert.That(result.Messages.Length, Is.EqualTo(0));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("AAAA"));
    }
    
    [Test]
    public void mapping_success_propagates_warnings_correctly()
    {
        var result = DoubleString("1", new []{"warning1"})
            .Map(int.Parse)
            .Map(r => r*2.1)
            .Map(r => r.ToString(CultureInfo.InvariantCulture));
        
        Assert.That(result.Messages.Length, Is.EqualTo(1));
        Assert.That(result.Messages.Single().Id, Is.EqualTo("warning1"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("23.1"));
    }
    
    [Test]
    public void mapping_after_error_propagates_warnings_correctly()
    {
        var result = DoubleString("1", new []{"warning1"})
            .Bind(r => DoubleString(r, isSuccessful: false)) // <-- ERROR OCCURS HERE
            .Map(int.Parse)
            .Map(r => r*2.1)
            .Map(r => r.ToString(CultureInfo.InvariantCulture));
        
        Assert.That(result.Messages.Length, Is.EqualTo(2));
        Assert.That(result.Messages.Single(v => v.Level == MessageLevel.Warning).Id, Is.EqualTo("warning1"));
        Assert.That(result.MatchError(out var _), Is.True);
    }
    
    [Test]
    public void mapping_error_propagates_warnings_correctly()
    {
        var result = DoubleString("1", new[] { "warning1" })
            .Bind(r => DoubleString(r, isSuccessful: false)) // <-- ERROR OCCURS HERE
            .Map(int.Parse)
            .Map(r => r * 2.1)
            .Map(r => r.ToString(CultureInfo.InvariantCulture))
            .MapError(e => Error.Create("mappedError", e.ErrorMessage)); // <-- ERROR IS MAPPED HERE
        
        Assert.That(result.Messages.Length, Is.EqualTo(2));
        Assert.That(result.Messages.Single(v => v.Level == MessageLevel.Warning).Id, Is.EqualTo("warning1"));
        Assert.That(result.MatchError(out var _), Is.True);
    }
    
    [Test]
    public void successful_result_with_warning_in_start_of_chain()
    {
        var result = DoubleString("a", warnings: new []{"warning1"})
            .Map(r => r.ToUpper())
            .Bind(r => DoubleString(r));
        
        Assert.That(result.Messages.Length, Is.EqualTo(1));
        Assert.That(result.Messages.Single().Id, Is.EqualTo("warning1"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("AAAA"));
    }
    
    [Test]
    public void successful_result_with_warning_in_all_parts_of_chain()
    {
        var result = DoubleString("a", warnings: new []{"warning1"})
            .Map(r => r.ToUpper())
            .Bind(r => DoubleString(r, warnings: new []{"warning2"}));
        
        Assert.That(result.Messages.Length, Is.EqualTo(2));
        Assert.That(result.Messages.First().Id, Is.EqualTo("warning1"));
        Assert.That(result.Messages.Skip(1).First().Id, Is.EqualTo("warning2"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("AAAA"));
    }
    
    [Test]
    public void binding_successful_result_propagates_all_warnings()
    {
        var result = DoubleString("a", warnings: new []{"warning1"})
            .Bind(r => DoubleString(r, warnings: new []{"warning2"}))
            .Bind(r => DoubleString(r, warnings: new []{"warning3"}))
            .Bind(r => DoubleString(r, warnings: new []{"warning4"}));
        
        Assert.That(result.Messages.Length, Is.EqualTo(4));
        Assert.That(result.Messages.First().Id, Is.EqualTo("warning1"));
        Assert.That(result.Messages.Last().Id, Is.EqualTo("warning4"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("aaaaaaaaaaaaaaaa"));
    }
    
    [Test]
    public void binding_default_result_does_not_crash()
    {
        var result = CreateDefaultResult<string>()
            .Bind(r => DoubleString(r, warnings: new []{"warning2"}))
            .Bind(r => DoubleString(r, warnings: new []{"warning3"}))
            .Bind(r => DoubleString(r, warnings: new []{"warning4"}));
        
        Assert.That(result.Messages.Length, Is.EqualTo(3));
        Assert.That(result.Messages.First().Id, Is.EqualTo("warning2"));
        Assert.That(result.Messages.Last().Id, Is.EqualTo("warning4"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("")); // doubling string.Empty => string.Empty
    }

    [Test]
    public async Task flattening_ienumerable_of_results_respects_warning_messages()
    {
        var input = new[] { "1", "2", "3", "4", "5"};

        var result = input.Select(i => DoubleString(i, new []{$"warning{i}"})
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : someError))
            .FlattenResults(someError.ErrorIdentifier)
            .Map(x => x.Sum());

        Assert.That(result.Messages.Length, Is.EqualTo(5));
        Assert.That(result.Messages.First().Id, Is.EqualTo("warning1"));
        Assert.That(result.Messages.Last().Id, Is.EqualTo("warning5"));
        Assert.That(result.MatchSuccess(out var sum), Is.True);
        Assert.That(sum, Is.EqualTo(11+22+33+44+55));
    }
    
    private Result<string> DoubleString(string input, string[]? warnings = null, bool isSuccessful = true)
    {
        if (!isSuccessful) return someError;
        var result = input + input;
        return warnings != null
            ? (result, warnings.Select(w => new PipelineMessage(w, MessageLevel.Warning, w)).ToArray())
            : result;
    }

    private Result<T> CreateDefaultResult<T>() => default;
}