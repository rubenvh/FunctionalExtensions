namespace FluentSolidity.FunctionalExtensions;

/// <summary>
/// A structured message that can travel alongside a Result through a computation pipeline.
/// </summary>
/// <param name="Id">Machine-readable identifier for the message</param>
/// <param name="Level">Severity level (Info, Warning, Error)</param>
/// <param name="Message">Human-readable message text</param>
/// <param name="Context">Optional contextual information</param>
public record PipelineMessage(string Id, MessageLevel Level, string Message, string? Context = null);
