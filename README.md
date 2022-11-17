# GG SQL Migration docker action

This action runs SQL migrations

## Inputs

## `connection-string`

**Required** The database connection string.

## `config-path`

**Required** The configuration yaml file path.

## `timeout`

Maximum time of the action.

## Example usage

uses: actions/gg-sql-migration-action@v2
with:
  connection-string: 'Server=tcp:{serverName}.windows.net,1433;Initial Catalog={dbName};Persist Security Info=False;User ID={appId};Password={appSecret};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Authentication=ActiveDirectoryServicePrincipal;'
  config-path: 'migrations/migrations.yml'
  timeout: 00:05:00