using MoMS.Server.Data;
using MoMS.Server.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Services;

public class PreparationService(MoMsDbContext context) : BaseService(context)
{
    public async Task<List<Preparation>> GetPreparationAsync(CancellationToken cancellationToken)
    {
        return await Context.Preparations
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Dictionary<string, object?>>> GetSharingProductAsync(
        string? type,
        CancellationToken cancellationToken)
    {
        var rows = new List<Dictionary<string, object?>>();

        var connection = (SqlConnection)Context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "GetSharingProduct";
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Type", (object?)type ?? DBNull.Value));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>(reader.FieldCount);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[reader.GetName(i)] = value is DBNull ? null : value;
                }
                rows.Add(row);
            }
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }

        return rows;
    }
}