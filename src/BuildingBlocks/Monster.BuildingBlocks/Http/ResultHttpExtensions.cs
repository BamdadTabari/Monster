using Microsoft.AspNetCore.Http;

namespace Monster.BuildingBlocks.Http;

/// <summary>
/// Helpers to turn Result / Result&lt;T&gt; into minimal-API responses,
/// using ResponseDto on success and ProblemDetails on failure.
/// </summary>
public static class ResultHttpExtensions
{
    /// <summary>200/201/... with ResponseDto&lt;T&gt; on success; ProblemDetails(400) on failure.</summary>
    public static IResult ToHttp<T>(
        this Result<T> r,
        string okMessage = "OK",
        int successCode = StatusCodes.Status200OK)
    {
        if (r.IsSuccess)
            return Results.Json(ResponseDto<T>.Ok(r.Value!, okMessage), statusCode: successCode);

        // Use ProblemDetails for failures (consistent with Hellang middleware)
        return Results.Problem(
            title: "request.failed",
            detail: string.IsNullOrWhiteSpace(r.Error) ? "Unknown error." : r.Error,
            statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>200/... with ResponseDto on success; ProblemDetails(400) on failure.</summary>
    public static IResult ToHttp(
        this Result r,
        string okMessage = "OK",
        int successCode = StatusCodes.Status200OK)
    {
       
        if (r.IsSuccess)
            return Results.Json(ResponseDto<string>.Ok(okMessage), statusCode: successCode);

        return Results.Problem(
            title: "request.failed",
            detail: string.IsNullOrWhiteSpace(r.Error) ? "Unknown error." : r.Error,
            statusCode: StatusCodes.Status400BadRequest);
    }
}
