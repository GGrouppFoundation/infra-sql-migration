using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra;

internal sealed class DbChangeLogApi : IDbChangeLogApi
{
    private const string DbChangeLogCreateTableQuery = """
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='_ChangeLogHistory' and xtype='U')
            BEGIN
            CREATE TABLE [_ChangeLogHistory]( 
                [Id] varchar(100) NOT NULL,
                [Comment] nvarchar (255) NULL,
                [CreationTime] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
                PRIMARY KEY (Id)
            );
            CREATE INDEX ChangeLogHistoryCreationTimeIndex ON [_ChangeLogHistory](CreationTime DESC);
            END
        """;

    private const string DbChangeLogIdLastGetQuery = "SELECT TOP 1 Id From [_ChangeLogHistory] ORDER BY CreationTime DESC;";

    private const string DbChangeLogInsertQuery = "INSERT INTO [_ChangeLogHistory](Id, Comment) VALUES (@Id, @Comment);";

    private readonly ISqlApi sqlApi;

    private readonly IMigrationFileReader fileReader;

    internal DbChangeLogApi(ISqlApi sqlApi, IMigrationFileReader fileReader)
    {
        this.sqlApi = sqlApi;
        this.fileReader = fileReader;
    }

    public async ValueTask EnsureTableAsync(CancellationToken cancellationToken)
        =>
        await sqlApi.ExecuteNonQueryAsync(new(DbChangeLogCreateTableQuery), cancellationToken).ConfigureAwait(false);

    public async ValueTask<DbChangeLogId?> GetLastChangeLogIdAsync(CancellationToken cancellationToken)
    {
        var sqlRequest = new DbRequest(DbChangeLogIdLastGetQuery);
        var dbResult = await sqlApi.QueryEntityOrAbsentAsync<DbChangeLogId>(sqlRequest, cancellationToken).ConfigureAwait(false);

        return dbResult.Fold(AsNullable, GetNull);

        static DbChangeLogId? AsNullable(DbChangeLogId dbChangeLogId)
            =>
            dbChangeLogId;

        static DbChangeLogId? GetNull(Unit _)
            =>
            null;
    }

    public async ValueTask ExecuteMigrationAsync(SqlMigrationItem migrationItem, CancellationToken cancellationToken)
    {
        var migrationQuery = await ReadMigrationQueryAsync(migrationItem, cancellationToken).ConfigureAwait(false);

        var sqlRequest = new DbRequest(
            query: BuildMigrationTransactionQuery(migrationQuery),
            parameters: new FlatArray<DbParameter>(
                new("Id", migrationItem.Id ?? string.Empty),
                new("Comment", migrationItem.Comment ?? string.Empty)));

        _ = await sqlApi.ExecuteNonQueryAsync(sqlRequest, cancellationToken).ConfigureAwait(false);
    }

    private ValueTask<string> ReadMigrationQueryAsync(SqlMigrationItem migrationItem, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(migrationItem.Path))
        {
            return new(string.Empty);
        }

        return new(fileReader.ReadMigrationQueryAsync(migrationItem.Path, cancellationToken));
    }

    private static string BuildMigrationTransactionQuery(string migrationQuery)
    {
        if (string.IsNullOrEmpty(migrationQuery))
        {
            return DbChangeLogInsertQuery;
        }

        return $"""
            BEGIN TRANSACTION;
                {migrationQuery}
                {DbChangeLogInsertQuery}
            COMMIT;
            """;
    }
}