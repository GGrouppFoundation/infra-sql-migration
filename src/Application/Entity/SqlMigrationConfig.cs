namespace GGroupp.Infra;

internal sealed record class SqlMigrationConfig
{
    public required SqlMigrationItem[]? Migrations { get; init; }
}