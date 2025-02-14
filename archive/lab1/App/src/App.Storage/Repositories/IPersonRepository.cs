using App.Common.Models;

namespace App.Storage.Repositories;

public interface IPersonRepository
{
    IEnumerable<Person> GetAll();
    Person? Get(long id);
    Task<long> CreateAsync(Person person);
    Task<bool> DeleteAsync(long id);
    Task<bool> UpdateAsync(Person person);
}