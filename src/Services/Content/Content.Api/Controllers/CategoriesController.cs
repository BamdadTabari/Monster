using Content.Application.Categories;
using Content.Application.Categories.Create;
using Content.Application.Categories.Delete;
using Content.Application.Categories.GetById;
using Content.Application.Categories.List;
using Content.Application.Categories.Rename;
using Monster.BuildingBlocks.Messages; 

namespace Content.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categories")]
[Produces("application/json")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    // POST /api/categories
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand cmd, CancellationToken ct)
    {
        var r = await sender.Send(cmd, ct); // Result<Guid>
        // Option A: one-liner with your bridge (no Location header)
        return r.ToActionResult(MessageCatalog.Created, StatusCodes.Status201Created);

        // Option B (if you want a Location header):
        // if (!r.IsSuccess) return r.ToActionResult();
        // return CreatedAtAction(nameof(GetById), new { id = r.Value! },
        //     ResponseDto<Guid>.Created(r.Value!, MessageCatalog.Created));
    }

    // GET /api/categories/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponseDto<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var r = await sender.Send(new GetCategoryByIdQuery(id), ct); // Result<CategoryDto>
        if (!r.IsSuccess)
            return NotFound(ResponseDto<string>.NotFound("Category not found."));
        return Ok(ResponseDto<CategoryDto>.Ok(r.Value!, MessageCatalog.Ok));
    }

    // GET /api/categories?pageNumber=1&pageSize=20&search=foo
    // Binds your existing PageRequest from querystring.
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<PageResponse<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] PageRequest page,
        [FromQuery] string? search,
        CancellationToken ct = default)
    {
        // If your query only accepts (pageNumber, pageSize, search), keep it like this:
        var r = await sender.Send(new ListCategoriesQuery(page.PageNumber, page.PageSize, search), ct);

        // If you later add sorting to your query, pass page.SortBy and page.Desc too.
        return r.ToActionResult(MessageCatalog.Ok);
    }

    // PUT /api/categories/{id}/name
    [HttpPut("{id:guid}/name")]
    [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Rename([FromRoute] Guid id, [FromBody] RenameCategoryCommand body, CancellationToken ct)
    {
        var r = await sender.Send(body with { Id = id }, ct); // Result
        return r.ToActionResult(MessageCatalog.Updated);
    }

    // DELETE /api/categories/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var r = await sender.Send(new DeleteCategoryCommand(id), ct); // Result
        return r.ToActionResult(MessageCatalog.Deleted);
    }
}
