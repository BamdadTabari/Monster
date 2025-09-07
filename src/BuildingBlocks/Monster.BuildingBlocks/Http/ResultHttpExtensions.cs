using System.Net;
using Microsoft.AspNetCore.Http;

namespace Monster.BuildingBlocks.Http;

/// <summary>
/// Bridges Result / Result&lt;T&gt; to HTTP (usable from minimal endpoints or controllers).
/// Success → ResponseDto; Failure → ProblemDetails(400).
/// </summary>
public static class ResultHttpExtensions
{
    public static IResult ToHttp<T>(
        this Result<T> r,
        string okMessage = "OK",
        int successCode = StatusCodes.Status200OK)
    {
        if (r.IsSuccess)
        {
            // Centralized envelope. Respect desired successCode.
            var dto = new ResponseDto<T>((HttpStatusCode)successCode, okMessage, r.Value);
            return Results.Json(dto, statusCode: (int)dto.response_code);
        }

        return Results.Problem(
            title: "request.failed",
            detail: string.IsNullOrWhiteSpace(r.Error) ? "Unknown error." : r.Error,
            statusCode: StatusCodes.Status400BadRequest);
    }

    public static IResult ToHttp(
        this Result r,
        string okMessage = "OK",
        int successCode = StatusCodes.Status200OK)
    {
        if (r.IsSuccess)
        {
            var dto = new ResponseDto<string>((HttpStatusCode)successCode, okMessage, null);
            return Results.Json(dto, statusCode: (int)dto.response_code);
        }

        return Results.Problem(
            title: "request.failed",
            detail: string.IsNullOrWhiteSpace(r.Error) ? "Unknown error." : r.Error,
            statusCode: StatusCodes.Status400BadRequest);
    }
}
