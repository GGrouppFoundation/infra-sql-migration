namespace GGroupp.Infra;

internal sealed partial class DbChangeLogId : IDbEntity<DbChangeLogId>
{
    public static DbChangeLogId ReadEntity(IDbItem dbItem)
        =>
        new(
            id: dbItem.GetFieldValueOrThrow("Id").CastToString() ?? string.Empty);

    private DbChangeLogId(string id)
        =>
        Id = id ?? string.Empty;

    public string Id { get; }
}