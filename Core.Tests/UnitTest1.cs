using System.Globalization;

namespace Core.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void left_matches_left()
    {
        var e = Either.Left<string, object>("error");
        Assert.That(e.MatchLeft(out var _), Is.True);
        Assert.That(e.MatchRight(out var _), Is.False);
    }
    
    [Test]
    public void right_matches_right()
    {
        var e = Either.Right<string, int>(0);
        Assert.That(e.MatchLeft(out var _), Is.False);
        Assert.That(e.MatchRight(out var _), Is.True);
    }

    [Test]
    public void left_shortcircuits_chain()
    {
        var e = Either.Left<string, int>("error");

        var result = e
            .Map(i => i + 1.0m)
            .Map(x => (object)x.ToString(CultureInfo.InvariantCulture))
            .Map(x => x)
            .Bind(x => Either.Right<string, string>("6"));

        Assert.IsTrue(result.MatchLeft(out var actual));
        Assert.AreEqual("error", actual);
    }
    
    [Test]
    public void right_flows_through__chain()
    {
        var e = Either.Right<string, int>(5);

        var result = e
            .Map(i => i + 1.0m)
            .Map(x => (object)x.ToString(CultureInfo.InvariantCulture))
            .Map(x => x)
            .Bind(x => Either.Right<string, string>(x + "6"));

        Assert.IsTrue(result.MatchRight(out var actual));
        Assert.AreEqual("6.06", actual);
    }
    
    [Test]
    public void left_shortcircuits_in_middle_of_chain()
    {
        var e = Either.Right<string, int>(5);

        var result = e
            .Map(i => i + 1.0m)
            .Map(x => (object)x.ToString(CultureInfo.InvariantCulture))
            .Bind(x => Either.Left<string, object>("error"))
            .Bind(x => Either.Right<string, string>(x + "6"));

        Assert.IsTrue(result.MatchLeft(out var actual));
        Assert.AreEqual("error", actual);
    }
}