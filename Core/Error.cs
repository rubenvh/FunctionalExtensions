namespace FluentSolidity.FunctionalExtensions;

/// <summary>
/// Represents a domain error with a machine-readable identifier,
/// a human-readable message, and an optional context.
/// </summary>
/// <param name="ErrorIdentifier">The machine-readable identifier, handy for translations</param>
/// <param name="ErrorMessage">The human-readable message</param>
/// <param name="Context">Optional contextual information</param>
public record Error(string ErrorIdentifier, string ErrorMessage, string? Context = null)
{
    public static Error Create(string identifier, string message) => new(identifier, message);
    public static Error Create(string identifier, string message, string? context) => new(identifier, message, context);

    public ValidationMessage ToValidationMessage() => new(ErrorIdentifier, ValidationLevel.Error, ErrorMessage, Context);
}
