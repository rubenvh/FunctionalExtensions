using System.Globalization;

namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultPipelineMessageTests : ResultTestBase
{
    [Test]
    public void successful_result_without_messages()
    {
        var result = DoubleStringWithMessages("a")
            .Map(r => r.ToUpper())
            .Bind(r => DoubleStringWithMessages(r));

        Assert.That(result.Messages.Count, Is.EqualTo(0));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("AAAA"));
    }

    [Test]
    public void mapping_success_propagates_messages_correctly()
    {
        var result = DoubleStringWithMessages("1", new[] { "warning1" })
            .Map(int.Parse)
            .Map(r => r * 2.1)
            .Map(r => r.ToString(CultureInfo.InvariantCulture));

        Assert.That(result.Messages.Count, Is.EqualTo(1));
        Assert.That(result.Messages.Single().Id, Is.EqualTo("warning1"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("23.1"));
    }

    [Test]
    public void mapping_after_error_propagates_messages_correctly()
    {
        var result = DoubleStringWithMessages("1", new[] { "warning1" })
            .Bind(r => DoubleStringWithMessages(r, isSuccessful: false)) // <-- ERROR OCCURS HERE
            .Map(int.Parse)
            .Map(r => r * 2.1)
            .Map(r => r.ToString(CultureInfo.InvariantCulture));

        Assert.That(result.Messages.Count, Is.EqualTo(2));
        Assert.That(result.Messages.Single(v => v.Level == MessageLevel.Warning).Id, Is.EqualTo("warning1"));
        Assert.That(result.MatchError(out var _), Is.True);
    }

    [Test]
    public void mapping_error_propagates_messages_correctly()
    {
        var result = DoubleStringWithMessages("1", new[] { "warning1" })
            .Bind(r => DoubleStringWithMessages(r, isSuccessful: false)) // <-- ERROR OCCURS HERE
            .Map(int.Parse)
            .Map(r => r * 2.1)
            .Map(r => r.ToString(CultureInfo.InvariantCulture))
            .MapError(e => Error.Create("mappedError", e.ErrorMessage)); // <-- ERROR IS MAPPED HERE

        Assert.That(result.Messages.Count, Is.EqualTo(2));
        Assert.That(result.Messages.Single(v => v.Level == MessageLevel.Warning).Id, Is.EqualTo("warning1"));
        Assert.That(result.MatchError(out var _), Is.True);
    }

    [Test]
    public void successful_result_with_message_in_start_of_chain()
    {
        var result = DoubleStringWithMessages("a", warnings: new[] { "warning1" })
            .Map(r => r.ToUpper())
            .Bind(r => DoubleStringWithMessages(r));

        Assert.That(result.Messages.Count, Is.EqualTo(1));
        Assert.That(result.Messages.Single().Id, Is.EqualTo("warning1"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("AAAA"));
    }

    [Test]
    public void successful_result_with_messages_in_all_parts_of_chain()
    {
        var result = DoubleStringWithMessages("a", warnings: new[] { "warning1" })
            .Map(r => r.ToUpper())
            .Bind(r => DoubleStringWithMessages(r, warnings: new[] { "warning2" }));

        Assert.That(result.Messages.Count, Is.EqualTo(2));
        Assert.That(result.Messages.First().Id, Is.EqualTo("warning1"));
        Assert.That(result.Messages.Skip(1).First().Id, Is.EqualTo("warning2"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("AAAA"));
    }

    [Test]
    public void binding_successful_result_propagates_all_messages()
    {
        var result = DoubleStringWithMessages("a", warnings: new[] { "warning1" })
            .Bind(r => DoubleStringWithMessages(r, warnings: new[] { "warning2" }))
            .Bind(r => DoubleStringWithMessages(r, warnings: new[] { "warning3" }))
            .Bind(r => DoubleStringWithMessages(r, warnings: new[] { "warning4" }));

        Assert.That(result.Messages.Count, Is.EqualTo(4));
        Assert.That(result.Messages.First().Id, Is.EqualTo("warning1"));
        Assert.That(result.Messages.Last().Id, Is.EqualTo("warning4"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("aaaaaaaaaaaaaaaa"));
    }

    [Test]
    public void binding_default_result_does_not_crash()
    {
        var result = CreateDefaultResult<string>()
            .Bind(r => DoubleStringWithMessages(r, warnings: new[] { "warning2" }))
            .Bind(r => DoubleStringWithMessages(r, warnings: new[] { "warning3" }))
            .Bind(r => DoubleStringWithMessages(r, warnings: new[] { "warning4" }));

        Assert.That(result.Messages.Count, Is.EqualTo(3));
        Assert.That(result.Messages.First().Id, Is.EqualTo("warning2"));
        Assert.That(result.Messages.Last().Id, Is.EqualTo("warning4"));
        Assert.That(result.MatchSuccess(out var value), Is.True);
        Assert.That(value, Is.EqualTo("")); // doubling string.Empty => string.Empty
    }

    [Test]
    public async Task flattening_ienumerable_of_results_respects_pipeline_messages()
    {
        var input = new[] { "1", "2", "3", "4", "5" };

        var result = input.Select(i => DoubleStringWithMessages(i, new[] { $"warning{i}" })
                .Bind<string, int>(i => int.TryParse(i, out var result) ? result : someError))
            .FlattenResults(someError.ErrorIdentifier)
            .Map(x => x.Sum());

        Assert.That(result.Messages.Count, Is.EqualTo(5));
        Assert.That(result.Messages.First().Id, Is.EqualTo("warning1"));
        Assert.That(result.Messages.Last().Id, Is.EqualTo("warning5"));
        Assert.That(result.MatchSuccess(out var sum), Is.True);
        Assert.That(sum, Is.EqualTo(11 + 22 + 33 + 44 + 55));
    }
}
