FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SubseOpro.sln SubseOpro.slnx ./
COPY src/ ./src/
RUN dotnet restore src/SubseOpro.Api/SubseOpro.Api.csproj
RUN dotnet publish src/SubseOpro.Api/SubseOpro.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SubseOpro.Api.dll"]
