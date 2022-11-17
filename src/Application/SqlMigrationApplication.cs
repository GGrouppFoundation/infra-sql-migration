using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra;

internal static class SqlMigrationApplication
{
    public static async Task RunAsync(string[] args)
    {
        using var serviceProvider = CreateServiceProvider(args);
        await RunMigrationsAsync(serviceProvider).ConfigureAwait(false);
    }

    private static Task RunMigrationsAsync(IServiceProvider serviceProvider)
        =>
        MicrosoftDbProvider.Configure(ResolveMicrosoftDbProviderOption)
        .UseSqlApi()
        .Map(ResolveMigration)
        .Resolve(serviceProvider)
        .RunAsync();

    private static SqlMigration ResolveMigration(IServiceProvider serviceProvider, ISqlApi sqlApi)
        =>
        SqlMigration.InternalCreate(
            changeLogApi: new DbChangeLogApi(sqlApi, MigrationFileReader.Instance),
            configReader: ConfigReader.Instance,
            loggerFactory: serviceProvider.GetRequiredService<ILoggerFactory>(),
            option: serviceProvider.GetRequiredService<IConfiguration>().Get<SqlMigrationOption>() ?? new());

    private static MicrosoftDbProviderOption ResolveMicrosoftDbProviderOption(IServiceProvider serviceProvider)
    {
        var connectionString = serviceProvider.GetRequiredService<IConfiguration>()["ConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("ConnectionString must be specified");
        }

        return new(connectionString);
    }

    private static ServiceProvider CreateServiceProvider(string[] args)
        =>
        new ServiceCollection()
        .AddLogging(
            static builder => builder.AddConsole())
        .AddSingleton(
            BuildConfiguration(args))
        .BuildServiceProvider();

    private static IConfiguration BuildConfiguration(string[] args)
        =>
        new ConfigurationBuilder().AddEnvironmentVariables().AddCommandLine(args).Build();
}