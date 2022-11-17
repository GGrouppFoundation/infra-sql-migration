using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra;

internal interface IDbChangeLogApi
{
    ValueTask EnsureTableAsync(CancellationToken cancellationToken);

    ValueTask<DbChangeLogId?> GetLastChangeLogIdAsync(CancellationToken cancellationToken);

    ValueTask ExecuteMigrationAsync(SqlMigrationItem migrationItem, CancellationToken cancellationToken);
}