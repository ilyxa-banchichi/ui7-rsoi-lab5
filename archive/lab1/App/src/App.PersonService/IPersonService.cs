using App.Common.Models;

namespace App.PersonService;

public interface IPersonService
{
    IEnumerable<Person> GetAllPersons();
    Person GetPerson(long id);
    Task<long> CreatePersonAsync(Person person);
    Task DeletePersonAsync(long id);
    Task<Person> UpdatePersonAsync(Person person);
}