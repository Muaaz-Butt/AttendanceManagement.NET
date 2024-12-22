FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy the .csproj and restore dependencies
COPY ["AttendanceManagementSystem.csproj", "./"]
RUN dotnet restore

# Copy everything else
COPY . .

# Publish the application
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# If you have SQLite database, uncomment and adjust the path
# COPY Database/app.db .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "AttendanceManagementSystem.dll"]