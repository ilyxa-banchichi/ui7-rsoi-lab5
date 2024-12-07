using System.ComponentModel.DataAnnotations;
using System.Net;
using Common.Models.DTO;
using Common.Models.Enums;
using Common.OauthService;
using Gateway.Common.Models.DTO;
using Gateway.Services;
using Gateway.Services.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.API.Controllers;

[Route("api/v1")]
[Authorize]
[ApiController]
public class GatewayController(
    ILibraryService libraryService, IReservationService reservationService,
    IRatingService ratingService) : Controller
{
    /// <summary>
    /// Получить список библиотек в городе
    /// </summary>
    /// <param name="city">Город</param>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <response code="200">Список библиотек в городе</response>
    [HttpGet("libraries")]
    [ProducesResponseType(typeof(LibraryPaginationResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetLibrariesInCity([Required]string city, int page = 1, [Range(1, 100)]int size = 1)
    {
        try
        {
            var accessToken = TokenUtils.GetToken(HttpContext);
            var response = await libraryService.GetLibrariesInCityAsync(city, page, size, accessToken);
            return Ok(response);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == null || e.StatusCode == HttpStatusCode.ServiceUnavailable)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new { Message = "Bonus Service unavailable" });

            return StatusCode((int)e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
    
    /// <summary>
    /// Получить список книг в выбранной библиотеке
    /// </summary>
    /// <param name="libraryUid">UUID библиотеки</param>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="showAll"></param>
    /// <response code="200">Список книг библиотеке</response>
    [HttpGet("libraries/{libraryUid}/books")]
    [ProducesResponseType(typeof(LibraryBookPaginationResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetBooksInLibrary([Required]string libraryUid, 
        int page = 1, [Range(1, 100)]int size = 1, bool showAll = false)
    {
        try
        {
            var accessToken = TokenUtils.GetToken(HttpContext);
            var response = await libraryService.GetBooksInLibraryAsync(libraryUid, page, size, accessToken, showAll);
            return Ok(response);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == null || e.StatusCode == HttpStatusCode.ServiceUnavailable)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new { Message = "Bonus Service unavailable" });

            return StatusCode((int)e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
    
    /// <summary>
    /// Получить информацию по всем взятым в прокат книгам пользователя
    /// </summary>
    /// <response code="200">Информация по всем взятым в прокат книгам</response>
    [HttpGet("reservations")]
    [ProducesResponseType(typeof(List<BookReservationResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetUserReservations()
    {
        try
        {
            var accessToken = TokenUtils.GetToken(HttpContext);
            var rawReservations = await reservationService.GetUserReservationsAsync(accessToken);
            
            var booksUid = rawReservations.Select(r => r.BookUid);
            var librariesUid = rawReservations.Select(r => r.LibraryUid);

            var booksTask = libraryService.GetBooksListAsync(booksUid, accessToken);
            var librariesTask = libraryService.GetLibrariesListAsync(librariesUid, accessToken);

            var books = await booksTask;
            var libraries = await librariesTask;
            
            var reservations = new List<BookReservationResponse>(rawReservations.Count);
            
            for (int i = 0; i < rawReservations.Count; i++)
            {
                var rawReservation = rawReservations[i];
                reservations.Add(new BookReservationResponse()
                {
                    ReservationUid = rawReservation.ReservationUid,
                    Status = rawReservation.Status,
                    StartDate = rawReservation.StartDate,
                    TillDate = rawReservation.TillDate,
                    Book = books[i],
                    Library = libraries[i]
                });
            }
            
            return Ok(reservations);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == null || e.StatusCode == HttpStatusCode.ServiceUnavailable)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new { Message = "Bonus Service unavailable" });

            return StatusCode((int)e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
    
    /// <summary>
    /// Взять книгу в библиотеке
    /// </summary>
    /// <param name="body"></param>
    /// <response code="200">Информация о бронировании</response>
    /// <response code="400">Ошибка валидации данных</response>
    [HttpPost("reservations")]
    [ProducesResponseType(typeof(TakeBookResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(List<BookReservationResponse>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> TakeBook([FromBody][Required] TakeBookRequest body)
    {
        RawBookReservationResponse? reservation = null;
        var accessToken = TokenUtils.GetToken(HttpContext);
        try
        {
            var rawReservations = await reservationService.GetUserReservationsAsync(accessToken);
            var rentedCount = rawReservations.Count(r => r.Status == ReservationStatus.RENTED);

            var userRating = await ratingService.GetUserRating(accessToken);
            var maxRentedCount = Math.Ceiling((double)(userRating.Stars / 10));

            if (rentedCount > maxRentedCount)
                return Ok(null);

            reservation = await reservationService.TakeBook(accessToken, body);

            await libraryService.TakeBookAsync(body.LibraryUid, body.BookUid, accessToken);

            var library = (await libraryService.GetLibrariesListAsync(new[] { body.LibraryUid }, accessToken))[0];
            var book = (await libraryService.GetBooksListAsync(new[] { body.BookUid }, accessToken))[0];

            var response = new TakeBookResponse()
            {
                ReservationUid = reservation.ReservationUid,
                Status = reservation.Status,
                StartDate = reservation.StartDate,
                TillDate = reservation.TillDate,
                Book = new BookInfo()
                {
                    BookUid = book.BookUid,
                    Name = book.Name,
                    Author = book.Author,
                    Genre = book.Genre
                },
                Library = library,
                Rating = userRating,
            };

            return Ok(response);
        }
        catch (LibraryServiceUnavailableException e)
        {
            if (reservation != null)
                await reservationService.TakeBookRollback(reservation.ReservationUid, accessToken);
            
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, new { Message = "Library Service unavailable" });
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == null || e.StatusCode == HttpStatusCode.ServiceUnavailable)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new { Message = "Bonus Service unavailable" });

            return StatusCode((int)e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
    
    /// <summary>
    /// Вернуть книгу
    /// </summary>
    /// <param name="reservationUid">UUID бронирования</param>
    /// <param name="body"></param>
    /// <response code="204">Книга успешно возвращена</response>
    /// <response code="404">Бронирование не найдено</response>
    [HttpPost("reservations/{reservationUid}/return")]
    [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> ReturnBook([FromRoute] Guid reservationUid, [FromBody] ReturnBookRequest body)
    {
        try
        {
            var accessToken = TokenUtils.GetToken(HttpContext);
            
            var reservation = await reservationService.ReturnBook(reservationUid, body.Date, accessToken);
            if (reservation == null)
                return NotFound(new ErrorResponse("Бронирование не найдено"));
            
            var updateBook = await libraryService.ReturnBookAsync(
                reservation.LibraryUid, reservation.BookUid, body.Condition, accessToken);

            bool isConditionChanged = updateBook.NewCondition != updateBook.OldCondition;
            bool isExpired = reservation.Status == ReservationStatus.EXPIRED;

            if (!isConditionChanged && !isExpired)
            {
                await ratingService.IncreaseRating(accessToken);
            }
            else
            {
                if (isConditionChanged)
                    await ratingService.DecreaseRating(accessToken);

                if (isExpired)
                    await ratingService.DecreaseRating(accessToken);
            }
            
            return Ok(null);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == null || e.StatusCode == HttpStatusCode.ServiceUnavailable)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new { Message = "Bonus Service unavailable" });

            return StatusCode((int)e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
    
    /// <summary>
    /// Получить рейтинг пользователя
    /// </summary>
    /// <response code="200">Рейтинг пользователя</response>
    [HttpGet("rating")]
    [ProducesResponseType(typeof(UserRatingResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetUserRating()
    {
        try
        {
            var accessToken = TokenUtils.GetToken(HttpContext);
            var response = await ratingService.GetUserRating(accessToken);
            return Ok(response);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == null || e.StatusCode == HttpStatusCode.ServiceUnavailable)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new { Message = "Bonus Service unavailable" });
            
            return StatusCode((int)e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
}