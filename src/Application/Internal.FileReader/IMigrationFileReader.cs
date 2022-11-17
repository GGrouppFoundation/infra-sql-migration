using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra;

internal interface IMigrationFileReader
{
    Task<string> ReadMigrationQueryAsync(string filePath, CancellationToken cancellationToken);
}