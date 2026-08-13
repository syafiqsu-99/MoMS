using MoMS.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Data;

public class MoMsDbContext(DbContextOptions<MoMsDbContext> options) : DbContext(options)
{
    public DbSet<FullList> FullList => Set<FullList>();
    public DbSet<Location> Locations => Set<Location>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USAGE is a reserved word in SQL Server; ensure it is always bracketed.
        modelBuilder.Entity<FullList>()
            .Property(f => f.Usage)
            .HasColumnName("USAGE");
    }
}
