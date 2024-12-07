using System.Net;
using Common.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RatingService.Common.Converters;
using RatingService.Storage.Repositories;

namespace RatingService.API.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class RatingController(IRatingsRepository ratingsRepository) : Controller
{
    /// <summary>
    /// Получить рейтинг пользователя
    /// </summary>
    /// <response code="200">Рейтинг пользователя</response>
    [HttpGet()]
    [ProducesResponseType(typeof(UserRatingResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetUserRating()
    { 
        try
        {
            var xUserName = HttpContext.User.Identity.Name;
            var rating = await ratingsRepository.GetUserRatingAsync(xUserName);
            if (rating == null)
                rating = await ratingsRepository.AddNewUserAsync(xUserName);

            return Ok(rating.ConvertAppModelToDto());
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e);
        }
    }
    
    /// <summary>
    /// Увеличить рейтинг пользователя
    /// </summary>
    /// <response code="200">Рейтинг пользователя</response>
    /// <response code="404">Пользователь не найден</response>
    [ProducesResponseType(typeof(UserRatingResponse), (int)HttpStatusCode.OK)]
    [HttpPatch("increase")]
    public async Task<IActionResult> IncreaseRating()
    { 
        try
        {
            var xUserName = HttpContext.User.Identity.Name;
            var rating = await ratingsRepository.IncreaseRatingAsync(xUserName);
            if (rating == null)
                return NotFound("User not found");
            
            return Ok(rating.ConvertAppModelToDto());
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e);
        }
    }
    
    /// <summary>
    /// Уменьшить рейтинг пользователя
    /// </summary>
    /// <response code="200">Рейтинг пользователя</response>
    /// <response code="404">Пользователь не найден</response>
    [ProducesResponseType(typeof(UserRatingResponse), (int)HttpStatusCode.OK)]
    [HttpPatch("decrease")]
    public async Task<IActionResult> DecreaseRating()
    { 
        try
        {
            var xUserName = HttpContext.User.Identity.Name;
            var rating = await ratingsRepository.DecreaseRatingAsync(xUserName);
            if (rating == null)
                return NotFound("User not found");
            
            return Ok(rating.ConvertAppModelToDto());
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e);
        }
    }
}