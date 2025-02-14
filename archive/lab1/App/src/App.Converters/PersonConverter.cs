using App.Common.Models;
using App.Common.Models.DTO;

namespace App.Converters;

public static class PersonConverter
{
    public static Person ConvertDtoToAppModel(this PersonRequest request, int id = 0)
    {
        return new Person()
        {
            Id = id,
            Name = request.Name,
            Address = request.Address,
            Work = request.Work,
            Age = request.Age,
        };
    }
    
    public static PersonResponse ConvertAppModelToDto(this Person person)
    {
        return new PersonResponse()
        {
            Id = person.Id,
            Name = person.Name ?? "",
            Address = person.Address ?? "",
            Work = person.Work ?? "",
            Age = person.Age ?? 0,
        };
    }
}