using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GGroupp.Infra;

internal sealed class ConfigReader : IConfigReader
{
    static ConfigReader()
    {
        Instance = new();
        YamlDeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
    }

    public static ConfigReader Instance { get; }

    private static readonly IDeserializer YamlDeserializer;

    private ConfigReader()
    {
    }

    public async Task<FlatArray<SqlMigrationItem>> ReadMigrationsAsync(string configPath, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configPath);
        var yaml = await File.ReadAllTextAsync(fullPath, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrEmpty(yaml))
        {
            return default;
        }

        return YamlDeserializer.Deserialize<SqlMigrationConfig>(yaml)?.Migrations;
    }
}