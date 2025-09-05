using System.Net;

namespace Monster.BuildingBlocks.Errors;

public class AppException : Exception
{
    public AppError Error { get; }
    public HttpStatusCode StatusCode { get; }

    public AppException(AppError error, HttpStatusCode status = HttpStatusCode.BadRequest) : base(error.Message)
    {
        Error = error; StatusCode = status;
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string what) : base(AppError.NotFound(what), HttpStatusCode.NotFound) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string what) : base(AppError.Conflict(what), HttpStatusCode.Conflict) { }
}
