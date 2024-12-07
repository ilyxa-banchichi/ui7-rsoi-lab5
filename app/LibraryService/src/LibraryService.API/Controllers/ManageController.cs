using Microsoft.AspNetCore.Mvc;

namespace LibraryService.API.Controllers;

[Route("manage")]
[ApiController]
public class ManageController : Controller
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok();
    }
}