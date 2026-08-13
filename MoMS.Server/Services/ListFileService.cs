using MoMS.Server.Models.Dtos;

namespace MoMS.Server.Services;

public class ListFileService(IWebHostEnvironment environment, ILogger<ListFileService> logger)
{
    private readonly string _listDirectory =
        Path.Combine(environment.ContentRootPath, "public", "list");

    private static bool IsSafeFileName(string fileName) =>
        !string.IsNullOrWhiteSpace(fileName)
        && fileName.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    public async Task<string?> ReadAsync(string fileName, CancellationToken cancellationToken)
    {
        if (!IsSafeFileName(Path.GetFileNameWithoutExtension(fileName)))
        {
            return null;
        }

        var path = Path.Combine(_listDirectory, fileName);
        if (!File.Exists(path))
        {
            logger.LogWarning("List file not found: {File}", fileName);
            return null;
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public async Task<bool> SaveAsync(SaveFileDto dto, CancellationToken cancellationToken)
    {
        if (!IsSafeFileName(dto.FileName))
        {
            return false;
        }

        Directory.CreateDirectory(_listDirectory);
        var path = Path.Combine(_listDirectory, $"{dto.FileName}.txt");
        await File.WriteAllTextAsync(path, dto.Content, cancellationToken);
        return true;
    }
}