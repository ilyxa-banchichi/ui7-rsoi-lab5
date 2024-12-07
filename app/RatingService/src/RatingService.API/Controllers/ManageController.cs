using Microsoft.AspNetCore.Mvc;

namespace RatingService.API.Controllers;

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