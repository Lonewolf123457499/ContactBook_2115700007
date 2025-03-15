using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Entity;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<AddressBookEntry> AddressBookEntries { get; set; }
}
