using MoMS.Server.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Services;

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

    protected static double? ToDoubleN(object value) =>
        value is DBNull or null ? null : Convert.ToDouble(value);

    protected static int? ToIntN(object value) =>
        value is DBNull or null ? null : Convert.ToInt32(value);

    protected static bool? ToBoolN(object value) =>
        value is DBNull or null ? null : Convert.ToBoolean(value);

    protected static DateTime? ToDateTimeN(object value) =>
        value is DBNull or null ? null : Convert.ToDateTime(value);

    protected static string? ToStringN(object value) =>
        value is DBNull or null ? null : value.ToString();
}