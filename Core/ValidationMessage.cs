namespace FluentSolidity.FunctionalExtensions;

/// <summary>
/// A structured validation message that can travel alongside a Result through a computation chain.
/// </summary>
/// <param name="Id">Machine-readable identifier for the message</param>
/// <param name="Level">Severity level (Info, Warning, Error)</param>
/// <param name="Message">Human-readable message text</param>
/// <param name="Context">Optional contextual information</param>
public record ValidationMessage(string Id, ValidationLevel Level, string Message, string? Context = null);
