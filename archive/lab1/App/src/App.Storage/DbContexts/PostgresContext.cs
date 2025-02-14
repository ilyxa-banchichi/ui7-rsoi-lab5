using App.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Storage.DbContexts;

public class PostgresContext : DbContext
{
    public DbSet<Person> Persons { get; set; }
    
    public PostgresContext(DbContextOptions<PostgresContext> options) : base(options) { }
}