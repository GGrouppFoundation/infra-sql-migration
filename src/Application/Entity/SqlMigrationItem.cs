namespace GGroupp.Infra;

internal sealed record class SqlMigrationItem
{
    public required string? Id { get; init; }

    public required string? Path { get; init; }

    public string? Comment { get; init; }
}