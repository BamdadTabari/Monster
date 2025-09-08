using System.Net;

namespace Monster.BuildingBlocks;

/// <summary>Uniform envelope for API responses.</summary>
public sealed record ResponseDto<T>(HttpStatusCode response_code, string message, T? data = default)
{
    public static ResponseDto<T> Ok(T data, string message = "OK") => new(HttpStatusCode.OK, message, data);
    public static ResponseDto<T> Created(T data, string message = "Created") => new(HttpStatusCode.Created, message, data);
    public static ResponseDto<T> NotFound(string message = "Not Found") => new(HttpStatusCode.NotFound, message);
    public static ResponseDto<T> Error(string message = "Error", HttpStatusCode code = HttpStatusCode.InternalServerError) => new(code, message);
}