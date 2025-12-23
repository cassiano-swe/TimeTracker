# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia csproj primeiro (cache)
COPY ["TimeTracker.Api/TimeTracker.Api.csproj", "TimeTracker.Api/"]
RUN dotnet restore "TimeTracker.Api/TimeTracker.Api.csproj"

# Copia o resto
COPY . .
WORKDIR /src/TimeTracker.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TimeTracker.Api.dll"]
