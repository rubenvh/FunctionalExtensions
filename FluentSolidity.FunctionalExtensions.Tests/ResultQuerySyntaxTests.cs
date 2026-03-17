namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultQuerySyntaxTests : ResultTestBase
{
    [Test]
    public void nesting_binds_using_query_syntax()
    {
        var actual = DoubleStringResult("1")
            .Bind(v1 => DoubleStringResult(v1)
                .Bind(v2 => DoubleStringResult(v2)
                    .Map(v3 => DoubleString(v3))));

        Assert.That(actual.MatchSuccess(out var v), Is.True);
        Assert.That(v, Is.EqualTo("1111111111111111"));


        Result<string> querySyntaxResult =
            from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResult(v1)
            from v3 in DoubleStringResult(v2)
            select DoubleString(v3);
        Assert.That(querySyntaxResult.MatchSuccess(out var querySyntax), Is.True, () => querySyntaxResult.Error!.ErrorMessage);
        Assert.That(querySyntax, Is.EqualTo("1111111111111111"));
    }

    [Test]
    public async Task nesting_binds_using_query_syntax_asynchronously()
    {
        var query =
            await from v1 in DoubleStringResultAsync("1")
                from v2 in DoubleStringResult(v1)
                from v3 in DoubleStringResult(v2)
                select DoubleString(v3);
        Assert.That(query.MatchSuccess(out var actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));

        query = await from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResultAsync(v1)
            from v3 in DoubleStringResult(v2)
            select DoubleString(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));

        query = await from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResult(v1)
            from v3 in DoubleStringResultAsync(v2)
            select DoubleString(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));

        query = await from v1 in DoubleStringResultAsync("1")
            from v2 in DoubleStringResultAsync(v1)
            from v3 in DoubleStringResultAsync(v2)
            select DoubleStringAsync(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));

        query = await from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResult(v1)
            from v3 in DoubleStringResult(v2)
            select DoubleStringAsync(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));

        query = from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResult(v1)
            from v3 in DoubleStringResult(v2)
            select DoubleStringResult(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));

        query = await from v1 in DoubleStringResult("1")
            from v2 in DoubleStringResult(v1)
            from v3 in DoubleStringResult(v2)
            select DoubleStringResultAsync(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));

        query = await from v1 in DoubleStringResultAsync("1")
            from v2 in DoubleStringResultAsync(v1)
            from v3 in DoubleStringResultAsync(v2)
            select DoubleStringResultAsync(v3);
        Assert.That(query.MatchSuccess(out actual), Is.True, () => query.Error!.ErrorMessage);
        Assert.That(actual, Is.EqualTo("1111111111111111"));
    }

    [Test]
    public void mapping_with_query_syntax()
    {
        var result1 = Result.New("1");

        var querySyntaxResult =
            from v1 in result1
            select DoubleString(v1);

        Assert.That(querySyntaxResult.MatchSuccess(out var querySyntax), Is.True, () => querySyntaxResult.Error!.ErrorMessage);
        Assert.That(querySyntax, Is.EqualTo("11"));
    }

    [Test]
    public async Task mapping_with_query_syntax_async()
    {
        var result1 = Task.FromResult(Result.New("1"));

        var querySyntaxResult =
            await (from v1 in result1
                select DoubleStringAsync(v1));

        Assert.That(querySyntaxResult.MatchSuccess(out var querySyntax), Is.True, () => querySyntaxResult.Error!.ErrorMessage);
        Assert.That(querySyntax, Is.EqualTo("11"));
    }
}
