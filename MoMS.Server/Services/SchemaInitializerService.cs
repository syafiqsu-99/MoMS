using Microsoft.Data.SqlClient;

namespace MoMS.Server.Data;

public class SchemaInitializerService(
    IConfiguration configuration,
    ILogger<SchemaInitializerService> logger) : IHostedService
{
    private readonly string _connectionString =
        configuration.GetConnectionString("MoMsConnection")
        ?? configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "No connection string found. Set ConnectionStrings__MoMsConnection or ConnectionStrings__DefaultConnection.");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var created = await EnsureListOptionTableAsync(connection, cancellationToken);

        if (created)
        {
            logger.LogInformation("Created and seeded 'list_option' table.");
        }
        else
        {
            logger.LogInformation("'list_option' table already exists; skipped.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task<bool> EnsureListOptionTableAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        if (await TableExistsAsync(connection, "list_option", cancellationToken))
        {
            return false;
        }

        const string ddl = @"
            CREATE TABLE list_option(
                id INT IDENTITY(1,1) PRIMARY KEY,
                category NVARCHAR(50) NOT NULL,
                value NVARCHAR(255) NOT NULL,
                sort_order INT NOT NULL DEFAULT 0,
                CONSTRAINT UQ_list_option_category_value UNIQUE (category, value)
            )";

        await using (var cmd = new SqlCommand(ddl, connection))
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await SeedListOptionsAsync(connection, cancellationToken);
        return true;
    }

    private static async Task SeedListOptionsAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        var seed = new Dictionary<string, string[]>
        {
            ["mould_maker"] = ["RB", "V. TOPLAS", "YH. ENG", "GS MOULD"],
            ["prepared"] = ["Hew CP", "Low PS", "Ameera", "Kamarul", "Alif"],
            ["production"] =
            [
                "A5", "A6", "A7", "A8", "A9", "A10", "A12", "A13", "A14",
                "A15", "A16", "A17", "A18", "A19", "A21"
            ],
            ["purpose"] = ["Sample/Development", "Repair", "Modification", "Service"],
            ["vendor"] = ["JLG", "SNS", "JUSEN", "MAGNUM", "SERVICE"],
            ["rack"] = BuildRackValues()
        };

        const string insertSql =
            "INSERT INTO list_option (category, value, sort_order) VALUES (@category, @value, @sortOrder)";

        foreach (var (category, values) in seed)
        {
            for (var i = 0; i < values.Length; i++)
            {
                await using var cmd = new SqlCommand(insertSql, connection);
                cmd.Parameters.AddWithValue("@category", category);
                cmd.Parameters.AddWithValue("@value", values[i]);
                cmd.Parameters.AddWithValue("@sortOrder", i + 1);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }

    private static string[] BuildRackValues()
    {
        var values = new List<string>();
        for (var n = 1; n <= 26; n++)
        {
            foreach (var suffix in new[] { "A", "B", "C", "D" })
            {
                values.Add($"{n}{suffix}");
            }
        }
        foreach (var letter in new[] { "A", "B", "C", "D", "E", "F" })
        {
            for (var n = 1; n <= 24; n++)
            {
                values.Add($"{letter}{n}");
            }
        }
        return [.. values];
    }

    private static async Task<bool> TableExistsAsync(
        SqlConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        const string sql =
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @table";

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@table", tableName);
        return (int)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0) > 0;
    }
}