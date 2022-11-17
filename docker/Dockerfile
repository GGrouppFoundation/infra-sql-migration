FROM mcr.microsoft.com/dotnet/aspnet:7.0

RUN mkdir /app
WORKDIR /app

COPY ./publish ./

ENTRYPOINT ["dotnet", "GGroupp.Infra.Sql.Migration.Application.dll"]