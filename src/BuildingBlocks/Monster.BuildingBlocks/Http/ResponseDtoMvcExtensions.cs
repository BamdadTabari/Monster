using Microsoft.AspNetCore.Mvc;

namespace Monster.BuildingBlocks.Http;

public static class ResponseDtoMvcExtensions
{
    public static IActionResult ToActionResult<T>(this ResponseDto<T> dto)
    {
        var status = (int)dto.response_code;
        return new ObjectResult(dto) { StatusCode = status };
    }
}
