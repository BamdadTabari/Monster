namespace Monster.BuildingBlocks.Errors;

/// <summary>Structured application error with a stable code.</summary>
public sealed record AppError(string Code, string Message, IDictionary<string, string[]>? Details = null)
{
    public static readonly AppError Unknown = new("unknown", "An unexpected error occurred.");
    public static AppError Validation(IDictionary<string, string[]> details) => new("validation.failed", "Validation failed.", details);
    public static AppError NotFound(string what) => new("not_found", $"{what} was not found.");
    public static AppError Conflict(string what) => new("conflict", $"{what} already exists.");
}
