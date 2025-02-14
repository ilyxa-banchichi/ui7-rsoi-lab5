using App.Common.Exceptions;
using App.Common.Models;
using App.PersonService;
using App.Storage.Repositories;
using Moq;

namespace App.UnitTests.Services;

public class PersonServiceUnitTests
{
    private readonly Person[] _persons = new Person[]
    {
        new()
        {
            Id = 1,
            Name = "Person1 Name",
            Address = "Person1 Address",
            Age = 10,
            Work = "Person1 Work",
        },
        new()
        {
            Id = 2,
            Name = "Person2 Name",
            Address = "Person2 Address",
            Age = 11,
            Work = "Person2 Work",
        },
    };
    
    private readonly IPersonService _personService;
    private readonly Mock<IPersonRepository> _mockPersonRepository = new();
    
    public PersonServiceUnitTests()
    {
        _personService = new PersonService.PersonService(_mockPersonRepository.Object);
    }
    
    [Test]
    public void GetAllPersons_Ok()
    {
        IEnumerable<Person> expect = _persons;
        _mockPersonRepository.Setup(r => r.GetAll()).Returns(expect);
        
        var persons = _personService.GetAllPersons();
        Assert.That(persons, Is.EqualTo(expect));
    }
    
    [Test]
    public void GetAllPersons_Nothing()
    {
        IEnumerable<Person> expect = Array.Empty<Person>();
        _mockPersonRepository.Setup(r => r.GetAll()).Returns(expect);
        
        var persons = _personService.GetAllPersons();
        Assert.That(persons, Is.EqualTo(expect));
    }
    
    [Test]
    public void GetPerson_Ok()
    {
        var id = 1;
        Person expect = _persons[id];
        _mockPersonRepository.Setup(r => r.Get(id)).Returns(expect);
        
        var persons = _personService.GetPerson(id);
        Assert.That(persons, Is.EqualTo(expect));
    }
    
    [Test]
    public void GetPerson_NegativeId()
    {
        var id = -1;
        Person expect = null;
        _mockPersonRepository.Setup(r => r.Get(id)).Returns(expect);
        
        Assert.Throws<NotFoundEntityByIdException>(() => _personService.GetPerson(id));
    }
    
    [Test]
    public void GetPerson_IncorrectId()
    {
        var id = 10000;
        Person expect = null;
        _mockPersonRepository.Setup(r => r.Get(id)).Returns(expect);
        
        Assert.Throws<NotFoundEntityByIdException>(() => _personService.GetPerson(id));
    }
    
    [Test]
    public async Task CreatePerson_Ok()
    {
        var expect = 1;
        var person = new Person()
        {
            Id = 0,
            Name = "Test",
            Address = "Test",
            Age = 10,
            Work = "Test",
        };
        
        _mockPersonRepository
            .Setup(r => r.CreateAsync(person))
            .ReturnsAsync(expect);
        
        var createdId = await _personService.CreatePersonAsync(person);
        Assert.That(createdId, Is.EqualTo(expect));
    }
    
    [Test]
    public async Task DeletePerson_Ok()
    {
        var id = 1;
        _mockPersonRepository.Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);
        
        await _personService.DeletePersonAsync(id);
        Assert.Pass();
    }
    
    [Test]
    public void DeletePerson_NotExisting()
    {
        var id = 1;
        _mockPersonRepository.Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(false);
        
        Assert.ThrowsAsync<NotFoundEntityByIdException>(async () => await _personService.DeletePersonAsync(id));

    }
}