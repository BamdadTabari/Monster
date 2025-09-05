using Content.Application.Categories.Create;
using MediatR;
using Monster.BuildingBlocks.Http;
using Monster.BuildingBlocks.Messages;

namespace Content.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/categories").WithTags("Categories");

        g.MapPost("/", async (CreateCategoryCommand cmd, ISender sender, CancellationToken ct) =>
        {
            var r = await sender.Send(cmd, ct);
            return r.ToHttp(okMessage: MessageCatalog.Created, successCode: StatusCodes.Status201Created);
        });

        return g;
    }
}
