using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Monster.BuildingBlocks.Http;

/// <summary>
/// Bridges Result/Result<T> to MVC IActionResult (for controllers).
/// Success -> ResponseDto; Failure -> ProblemDetails with mapped status.
/// </summary>
public static class ResultMvcExtensions
{
    public static IActionResult ToActionResult<T>(
        this Result<T> r,
        string okMessage = "OK",
        int successCode = StatusCodes.Status200OK)
    {
        if (r.IsSuccess)
        {
            var dto = new ResponseDto<T>((HttpStatusCode)successCode, okMessage, r.Value);
            return new OkObjectResult(dto) { StatusCode = successCode };
        }

        var (status, title) = MapErrorToStatus(r.Error);
        return new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title  = title,
            Detail = string.IsNullOrWhiteSpace(r.Error) ? "Unknown error." : r.Error,
            Type   = "https://httpstatuses.io/" + status
        }) { StatusCode = status };
    }

    public static IActionResult ToActionResult(
        this Result r,
        string okMessage = "OK",
        int successCode = StatusCodes.Status200OK)
    {
        if (r.IsSuccess)
        {
            var dto = new ResponseDto<string>((HttpStatusCode)successCode, okMessage, null);
            return new OkObjectResult(dto) { StatusCode = successCode };
        }

        var (status, title) = MapErrorToStatus(r.Error);
        return new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title  = title,
            Detail = string.IsNullOrWhiteSpace(r.Error) ? "Unknown error." : r.Error,
            Type   = "https://httpstatuses.io/" + status
        }) { StatusCode = status };
    }

    /// <summary>
    /// Very light heuristic mapping. Replace with your own policy if you carry error codes.
    /// </summary>
    private static (int status, string title) MapErrorToStatus(string? error)
    {
        var e = (error ?? string.Empty).ToLowerInvariant();

        if (e.Contains("unauthorized") || e.Contains("not authorized") || e.Contains("invalid credentials"))
            return (StatusCodes.Status401Unauthorized, "auth.unauthorized");

        if (e.Contains("forbidden") || e.Contains("not allowed"))
            return (StatusCodes.Status403Forbidden, "auth.forbidden");

        if (e.Contains("not found") || e.Contains("missing"))
            return (StatusCodes.Status404NotFound, "resource.not_found");

        // Handle unique constraint / conflicts (e.g., Postgres 23505 hints)
        if (e.Contains("conflict") || e.Contains("duplicate") || e.Contains("23505"))
            return (StatusCodes.Status409Conflict, "resource.conflict");

        if (e.Contains("validation") || e.Contains("invalid") || e.Contains("bad request"))
            return (StatusCodes.Status422UnprocessableEntity, "request.validation_failed");

        return (StatusCodes.Status400BadRequest, "request.failed");
    }
}
