# ===== STAGE 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và restore riêng để tận dụng cache
COPY TeacherScheduleAPI.csproj ./
RUN dotnet restore

# Copy toàn bộ source code
COPY . ./

# Publish
RUN dotnet publish -c Release -o /app/publish

# ===== STAGE 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy file publish từ stage build
COPY --from=build /app/publish .

# Railway sử dụng port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TeacherScheduleAPI.dll"]
