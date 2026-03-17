using System.Globalization;

namespace FluentSolidity.FunctionalExtensions.Tests;

/// <summary>
/// Shared helpers for all Result test fixtures.
/// Inherit from this class to get access to common helper methods and fields.
/// </summary>
public abstract class ResultTestBase
{
    protected Error someError = null!;
    protected bool wasAwaited = false;

    [SetUp]
    public void BaseSetup()
    {
        someError = Error.Create("error", "message");
        wasAwaited = false;
    }

    protected static string DoubleString(string x) => $"{x}{x}";
    protected static Task<string> DoubleStringAsync(string x) => Task.FromResult($"{x}{x}");
    protected static Result<string> DoubleStringResult(string x) => Result.New($"{x}{x}");
    protected static Task<Result<string>> DoubleStringResultAsync(string x) => Task.FromResult(Result.New($"{x}{x}"));
    protected static Result<T> CreateDefaultResult<T>() => default;

    protected async Task ReturnsNothingAsync()
    {
        await Task.Delay(100);
        wasAwaited = true;
    }

    protected static async Task<Result<string>> WrapExceptionHelper(string exMessage)
    {
        try
        {
            await Task.Delay(1);
            throw new Exception(exMessage);
        }
        catch (Exception ex)
        {
            return Error.Create("UnhandledException", ex.Message);
        }
    }

    /// <summary>
    /// Helper that creates a Result with optional pipeline messages (used by message propagation tests).
    /// </summary>
    protected Result<string> DoubleStringWithMessages(string input, string[]? warnings = null, bool isSuccessful = true)
    {
        if (!isSuccessful) return someError;
        var result = input + input;
        return warnings != null
            ? (result, warnings.Select(w => new PipelineMessage(w, MessageLevel.Warning, w)).ToArray())
            : result;
    }
}
