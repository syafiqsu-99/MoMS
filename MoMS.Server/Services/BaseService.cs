using MoMS.Server.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Services;

// Shared base for all domain services. Holds the scoped DbContext (used for
// EF-based reads/writes) and exposes a parameterized raw-SQL helper for the
// high-frequency / cross-database queries that bypass the ORM. Every raw call
// must pass values as SqlParameter — never interpolate into the command text.
public abstract class BaseService(MoMsDbContext context)
{
    protected MoMsDbContext Context { get; } = context;

    // Executes a parameterized query and projects each row via the mapper.
    protected async Task<List<T>> QueryRawAsync<T>(
        string sql,
        Func<SqlDataReader, T> map,
        CancellationToken cancellationToken,
        params SqlParameter[] parameters)
    {
        var results = new List<T>();

        var connection = (SqlConnection)Context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(map(reader));
            }
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }

        return results;
    }
}
