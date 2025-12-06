# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Disable file watchers to avoid inotify limit crash
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_WATCH_RELOAD_ON_CHANGE=false
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TeacherScheduleAPI.dll"]
