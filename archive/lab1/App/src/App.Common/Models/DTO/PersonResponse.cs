using System.ComponentModel.DataAnnotations;

namespace App.Common.Models.DTO;

public class PersonResponse
{
    [Required(AllowEmptyStrings = false)]
    public long Id { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } 
    public string Address { get; set; }
    public string Work { get; set; }
    public int Age { get; set; }
}