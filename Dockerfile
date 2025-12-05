# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj trước để cache
COPY TeacherScheduleAPI/*.csproj ./TeacherScheduleAPI/
RUN dotnet restore TeacherScheduleAPI/TeacherScheduleAPI.csproj

# Copy toàn bộ project
COPY . .

# Publish
RUN dotnet publish TeacherScheduleAPI/TeacherScheduleAPI.csproj -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TeacherScheduleAPI.dll"]
