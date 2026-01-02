using kz_webshop_be.DTOs;
using kz_webshop_be.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace kz_webshop_be.Controllers;

[Route("api/enums")]
[ApiController]
public class EnumController : ControllerBase
{
    private readonly IEnumService _enumService;

    public EnumController(IEnumService enumService)
    {
        _enumService = enumService;
    }

    [HttpGet("all")]
    public ActionResult<EnumListsDto> GetAllEnums()
    {
        var result = new EnumListsDto
        {
            Categories = _enumService.GetFunkoCategoryList(),
            Franchises = _enumService.GetFranchiseList(),
            Badges = _enumService.GetBadgeList(),
            FunkoPopTagTypes = _enumService.GetFunkoPopTagTypeList()
        };
        return Ok(result);
    }
}