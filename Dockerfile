# Build y ejecución en Render con variable PORT
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CletaEatsBackend/CletaEatsBackend.csproj CletaEatsBackend/
RUN dotnet restore CletaEatsBackend/CletaEatsBackend.csproj

COPY CletaEatsBackend/ CletaEatsBackend/
WORKDIR /src/CletaEatsBackend
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
CMD dotnet CletaEatsBackend.dll --urls "http://0.0.0.0:${PORT}"
