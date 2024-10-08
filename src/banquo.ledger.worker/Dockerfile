FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet restore banquo.ledger.worker.csproj
RUN dotnet publish banquo.ledger.worker.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "banquo.ledger.worker.dll"]


