using MoMS.Server.Models;
using MoMS.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Data;

public class MoMsDbContext(DbContextOptions<MoMsDbContext> options) : DbContext(options)
{
    public DbSet<FullList> FullList => Set<FullList>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Preparation> Preparations => Set<Preparation>();
    public DbSet<ListDocket> ListDockets => Set<ListDocket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FullList>()
            .Property(f => f.Usage)
            .HasColumnName("USAGE");

        modelBuilder.Entity<Preparation>().HasNoKey();
    }
}