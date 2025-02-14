using System.Net;
using App.Common.Exceptions;
using App.Common.Models.DTO;
using App.Converters;
using App.PersonService;
using Microsoft.AspNetCore.Mvc;

namespace App.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PersonsController : Controller
{
    private readonly IPersonService _personService;

    public PersonsController(IPersonService personService)
    {
        _personService = personService;
    }
    
    /// <summary>
    /// Get all Persons
    /// </summary>
    /// <response code="200">All Persons</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PersonResponse>), (int)HttpStatusCode.OK)]
    public IActionResult GetAllPersons()
    {
        try
        {
            var persons = _personService.GetAllPersons();
            return Ok(persons);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e);
        }
    }
    
    /// <summary>
    /// Get Person by ID
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">Person for ID</response>
    /// <response code="404">Not found Person for ID</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PersonResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
    public IActionResult GetPerson(int id)
    {
        try
        {
            var person = _personService.GetPerson(id);
            return Ok(person.ConvertAppModelToDto());
        }
        catch (NotFoundEntityByIdException e)
        {
            return NotFound(new ErrorResponse("Not found Person for ID"));
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e);
        }
    }
    
    /// <summary>
    /// Create new Person
    /// </summary>
    /// <param name="person">Person data</param>
    /// <response code="201">Created new Person</response>
    /// <response code="400">Invalid data</response>
    [HttpPost()]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreatePerson([FromBody] PersonRequest person)
    {
        try
        {
            if (!person.IsValid())
                return BadRequest("Invalid data");
        
            var id = await _personService.CreatePersonAsync(person.ConvertDtoToAppModel());
            return Created($"/api/v1/persons/{id}", null);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e);
        }
    }
    
    /// <summary>
    /// Remove Person by ID
    /// </summary>
    /// <param name="id">Person ID</param>
    /// <response code="204">Person for ID was removed</response>
    /// <response code="404">Not found Person for ID</response>
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeletePerson(int id)
    {
        try
        {
            await _personService.DeletePersonAsync(id);
            return NoContent();
        }
        catch (NotFoundEntityByIdException e)
        {
            return NotFound(new ErrorResponse("Not found Person for ID"));
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e);
        }
    }
    
    /// <summary>
    /// Update Person by ID
    /// </summary>
    /// <param name="id">Person ID</param>
    /// <param name="person">Person data</param>
    /// <response code="200">Person for ID was updated</response>
    /// <response code="400">Invalid data</response>
    /// <response code="404">Not found Person for ID</response>
    [HttpPatch("{id}")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdatePerson(int id, [FromBody] PersonRequest person)
    {
        try
        {
            if (!person.IsValid())
                return BadRequest("Invalid data");

            var model = person.ConvertDtoToAppModel(id);
            var updatedPerson = await _personService.UpdatePersonAsync(model);
            return Ok(updatedPerson);
        }
        catch (NotFoundEntityByIdException e)
        {
            return NotFound(new ErrorResponse("Not found Person for ID"));
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e);
        }
    }
}