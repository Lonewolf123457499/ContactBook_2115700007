using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Entity;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<AddressBookEntry> AddressBookEntries { get; set; }
    public DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AddressBookEntry>()
            .HasOne(c => c.User)
            .WithMany(u => u.AddressBookEntries)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete contacts when a user is deleted
    }
}
