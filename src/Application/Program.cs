using System.Threading.Tasks;

namespace GGroupp.Infra.Sql.Migration.Application;

static class Program
{
    static Task Main() => SqlMigrationApplication.RunAsync();
}