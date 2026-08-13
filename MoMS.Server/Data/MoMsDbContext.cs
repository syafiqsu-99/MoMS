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
    public DbSet<ListOption> ListOptions => Set<ListOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FullList>()
            .Property(f => f.Usage)
            .HasColumnName("USAGE");

        modelBuilder.Entity<Preparation>().HasNoKey();

        modelBuilder.Entity<ListOption>()
            .HasIndex(o => new { o.Category, o.Value })
            .IsUnique();
    }
}