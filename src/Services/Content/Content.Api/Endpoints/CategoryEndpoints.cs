using Content.Application.Categories;
using Content.Application.Categories.Create;
using Content.Application.Categories.Delete;
using Content.Application.Categories.GetById;
using Content.Application.Categories.List;
using Content.Application.Categories.Rename;
using MediatR;
using Monster.BuildingBlocks;
using Monster.BuildingBlocks.Http;
using Monster.BuildingBlocks.Messages;

namespace Content.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/categories").WithTags("Categories");

        // CREATE
        g.MapPost("/", async (CreateCategoryCommand cmd, ISender sender, CancellationToken ct) =>
        {
            var r = await sender.Send(cmd, ct);
            return r.ToHttp(okMessage: MessageCatalog.Created, successCode: StatusCodes.Status201Created);
        });

        // GET BY ID
        g.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var r = await sender.Send(new GetCategoryByIdQuery(id), ct);
            // If you want a 404 on not found, you can special-case here:
            if (!r.IsSuccess) return Results.NotFound(ResponseDto<string>.NotFound("Category not found."));
            return Results.Ok(ResponseDto<CategoryDto>.Ok(r.Value!, MessageCatalog.Ok));
        });

        // LIST (paged)
        g.MapGet("/", async (int pageNumber, int pageSize, string? search, ISender sender, CancellationToken ct) =>
        {
            var r = await sender.Send(new ListCategoriesQuery(pageNumber, pageSize, search), ct);
            return r.ToHttp(okMessage: MessageCatalog.Ok);
        });

        // RENAME
        g.MapPut("/{id:guid}/name", async (Guid id, RenameCategoryCommand body, ISender sender, CancellationToken ct) =>
        {
            var r = await sender.Send(body with { Id = id }, ct);
            return r.ToHttp(okMessage: MessageCatalog.Updated);
        });

        // DELETE
        g.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var r = await sender.Send(new DeleteCategoryCommand(id), ct);
            return r.ToHttp(okMessage: MessageCatalog.Deleted);
        });

        return g;
    }
}
