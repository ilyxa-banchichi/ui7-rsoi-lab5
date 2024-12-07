using Common.OauthService;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.API.Controllers;

[ApiController]
[Route("api/v1")]
public class OauthController : Controller
{
    private readonly IOauthService _oauthService;

    public OauthController(IOauthService oauthService)
    {
        _oauthService = oauthService;
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize()
    {
        var url = _oauthService.GetAuthorizeUrl();
        return Redirect(url);
    }
    
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var accessToken = await _oauthService.GetAccessToken(code);
        return Ok(accessToken);
    }
}