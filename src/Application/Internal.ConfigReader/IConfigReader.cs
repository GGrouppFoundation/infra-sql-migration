using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra;

internal interface IConfigReader
{
    Task<FlatArray<SqlMigrationItem>> ReadMigrationsAsync(string configPath, CancellationToken cancellationToken);
}