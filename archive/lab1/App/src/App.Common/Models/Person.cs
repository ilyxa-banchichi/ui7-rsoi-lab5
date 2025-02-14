namespace App.Common.Models;

/// <summary>
/// Данные о человеке
/// </summary>
public class Person
{
    public long Id { get; set; }
    public string? Name { get; set; } 
    public string? Address { get; set; }
    public string? Work { get; set; }
    public int? Age { get; set; }
}