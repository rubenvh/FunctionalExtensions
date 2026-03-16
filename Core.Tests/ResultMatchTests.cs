namespace FluentSolidity.FunctionalExtensions.Tests;

public class ResultMatchTests : ResultTestBase
{
    [Test]
    public void matching_successful_result_executes_success_lambda()
    {
        Assert.That(Result.New("success").Match(_ => true, _ => false), Is.True);
    }

    [Test]
    public void matching_error_result_executes_error_lambda()
    {
        Assert.That(Result.Error<string>(someError).Match(_ => true, _ => false), Is.False);
    }

    [Test]
    public async Task async_matching_successful_result_executes_success_lambda()
    {
        Assert.That(await Result.New("success")
                .Map(Task.FromResult)
                .Match(_ => Task.FromResult(true), _ => Task.FromResult(false)),
            Is.True);
    }

    [Test]
    public async Task async_matching_error_result_executes_error_lambda()
    {
        Assert.That(await Result.Error<string>(someError)
                .Map(Task.FromResult)
                .Match(_ => Task.FromResult(true), _ => Task.FromResult(false)),
            Is.False);
    }
}
