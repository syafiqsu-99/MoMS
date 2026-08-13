using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MoMS.Server.Data;

public class MoMsDbContextFactory : IDesignTimeDbContextFactory<MoMsDbContext>
{
    public MoMsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json",
                optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("MoMsConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No connection string found. Set the ConnectionStrings__MoMsConnection " +
                "or ConnectionStrings__DefaultConnection environment variable before running dotnet ef.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<MoMsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new MoMsDbContext(optionsBuilder.Options);
    }
}
