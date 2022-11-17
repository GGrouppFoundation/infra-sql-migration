using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra;

internal sealed class MigrationFileReader : IMigrationFileReader
{
    static MigrationFileReader()
        =>
        Instance = new();

    public static MigrationFileReader Instance { get; }

    private MigrationFileReader()
    {
    }

    public Task<string> ReadMigrationQueryAsync(string filePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
        return File.ReadAllTextAsync(fullPath, cancellationToken);
    }
}