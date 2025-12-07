# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the project files and restore dependencies
COPY GamesPlatform/*.csproj ./
RUN dotnet restore

# Copy remaining files and build the application
COPY . ./
RUN dotnet publish -c Release -o out

# Use the runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose the port and define the entry point
EXPOSE 80
CMD ["dotnet", "GamesPlatform.dll"]