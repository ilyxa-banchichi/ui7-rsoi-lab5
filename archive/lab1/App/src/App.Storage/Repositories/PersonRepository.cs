using App.Common.Models;
using App.Storage.DbContexts;

namespace App.Storage.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly PostgresContext _db;

    public PersonRepository(PostgresContext db)
    {
        _db = db;
    }

    public IEnumerable<Person> GetAll()
    {
        return _db.Persons.ToList();
    }

    public Person? Get(long id)
    {
        return _db.Persons.FirstOrDefault(p => p.Id == id);
    }

    public async Task<long> CreateAsync(Person person)
    {
        _db.Persons.Add(person);
        await _db.SaveChangesAsync();
        return await Task.FromResult(person.Id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var person = _db.Persons.FirstOrDefault(p => p.Id == id);
        if (person == null)
            return await Task.FromResult(false);

        _db.Persons.Remove(person);
        await _db.SaveChangesAsync();
        return await Task.FromResult(true);
    }

    public async Task<bool> UpdateAsync(Person person)
    {
        var existingEntity = await _db.Persons.FindAsync(person.Id);
        if (existingEntity != null)
        {
            _db.Entry(existingEntity).CurrentValues.SetValues(person);
            foreach (var property in _db.Entry(existingEntity).Properties)
            {
                if (property.CurrentValue == null)
                    property.IsModified = false;
            }
            
            await _db.SaveChangesAsync();
            return await Task.FromResult(true);
        }
        
        return await Task.FromResult(false);
    }
}