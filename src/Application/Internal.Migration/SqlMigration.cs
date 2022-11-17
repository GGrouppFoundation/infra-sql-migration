using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra;

internal sealed class SqlMigration
{
    internal static SqlMigration InternalCreate(
        IDbChangeLogApi changeLogApi, IConfigReader configReader, ILoggerFactory loggerFactory, SqlMigrationOption option)
        =>
        new(
            changeLogApi, configReader, loggerFactory.CreateLogger<SqlMigration>(), option);

    private readonly IDbChangeLogApi changeLogApi;

    private readonly IConfigReader configReader;

    private readonly ILogger logger;

    private readonly SqlMigrationOption option;

    private SqlMigration(IDbChangeLogApi changeLogApi, IConfigReader configReader, ILogger logger, SqlMigrationOption option)
    {
        this.changeLogApi = changeLogApi;
        this.configReader = configReader;
        this.logger = logger;
        this.option = option;
    }

    public async Task RunAsync()
    {
        try
        {
            using var cancellationTokenSource = GetCancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            logger.LogInformation("Read the migration config file '{path}'", option.ConfigPath);
            var migrations = await configReader.ReadMigrationsAsync(option.ConfigPath, cancellationToken).ConfigureAwait(false);

            if (migrations.IsEmpty)
            {
                logger.LogInformation("The config migrations list is empty. The operation has finished");
                return;
            }

            logger.LogInformation("Check if the change log table exists");
            await changeLogApi.EnsureTableAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Get the last change log from the database");
            var dbChangeLogId = await changeLogApi.GetLastChangeLogIdAsync(cancellationToken).ConfigureAwait(false);

            var notExecutedMigrations = GetNotExecutedMigrations(migrations, dbChangeLogId);
            if (notExecutedMigrations.Count is not > 0)
            {
                logger.LogInformation("All migrations were already executed");
                return;
            }

            foreach (var migration in migrations)
            {
                logger.LogInformation("Execute the migration {migrationId}", migration.Id);
                await changeLogApi.ExecuteMigrationAsync(migration, cancellationToken).ConfigureAwait(false);
            }

            logger.LogInformation("All migrations have been finished successfully");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "The operation was canceled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected exception was thrown");
            throw;
        }
    }

    private static IReadOnlyCollection<SqlMigrationItem> GetNotExecutedMigrations(
        FlatArray<SqlMigrationItem> migrations, DbChangeLogId? dbChangeLogId)
    {
        if (dbChangeLogId is null)
        {
            return migrations;
        }

        var lastMigration = migrations.FirstOrDefault(IsLastMigration);
        if (lastMigration is null)
        {
            return migrations;
        }

        var resultList = new List<SqlMigrationItem>(migrations.Length);
        var alreadyFound = false;

        foreach (var migrationItem in migrations)
        {
            if (alreadyFound)
            {
                resultList.Add(migrationItem);
                continue;
            }

            if (migrationItem == lastMigration)
            {
                alreadyFound = true;
            }
        }

        return resultList;

        bool IsLastMigration(SqlMigrationItem migrationItem)
            =>
            string.Equals(dbChangeLogId.Id, migrationItem.Id, StringComparison.InvariantCulture);
    }

    private CancellationTokenSource GetCancellationTokenSource()
        =>
        option.Timeout is null ? new() : new(option.Timeout.Value);
}