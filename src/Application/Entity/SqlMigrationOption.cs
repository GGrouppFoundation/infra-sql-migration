using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra;

internal sealed record class SqlMigrationOption
{
    private const string DefaultConfigPath = "migrations.yml";

    public SqlMigrationOption([AllowNull] string configPath = DefaultConfigPath, TimeSpan? timeout = null)
    {
        ConfigPath = string.IsNullOrEmpty(configPath) ? DefaultConfigPath : configPath;
        Timeout = timeout;
    }

    public string ConfigPath { get; }

    public TimeSpan? Timeout { get; }
}