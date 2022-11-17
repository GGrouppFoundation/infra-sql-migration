FROM ghcr.io/ggrouppfoundation/infra-sql-migration-app:v0.0.1-build.3
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "GGroupp.Infra.Sql.Migration.Application.dll" ]