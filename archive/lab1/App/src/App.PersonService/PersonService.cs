using App.Common.Exceptions;
using App.Common.Models;
using App.Storage.Repositories;

namespace App.PersonService;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;

    public PersonService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }
    
    public IEnumerable<Person> GetAllPersons()
    {
        return _personRepository.GetAll();
    }
    
    public Person GetPerson(long id)
    {
        var person = _personRepository.Get(id);
        return person ?? throw new NotFoundEntityByIdException($"{id}");;
    }
    
    public async Task<long> CreatePersonAsync(Person person)
    {
        return await _personRepository.CreateAsync(person);
    }
    
    public async Task DeletePersonAsync(long id)
    {
        var success = await _personRepository.DeleteAsync(id);
        if (!success)
            throw new NotFoundEntityByIdException($"{id}");
    }
    
    public async Task<Person> UpdatePersonAsync(Person person)
    {
        Person? updatedPerson = null;
        var exist = await _personRepository.UpdateAsync(person);
        if (exist)
            updatedPerson = _personRepository.Get(person.Id);
        
        return updatedPerson ?? throw new NotFoundEntityByIdException($"{person.Id}");
    }
}