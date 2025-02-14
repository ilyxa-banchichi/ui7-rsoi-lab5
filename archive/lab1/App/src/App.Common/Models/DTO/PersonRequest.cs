using System.ComponentModel.DataAnnotations;

namespace App.Common.Models.DTO;

public class PersonRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } 
    public string? Address { get; set; }
    public string? Work { get; set; }
    public int? Age { get; set; }
}

public static class PersonRequestExtensions
{
    public static bool IsValid(this PersonRequest request)
    {
        if (request.Name == "")
            return false;

        if (request.Age < 0)
            return false;

        return true;
    }
}