FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /build

# Copy all source
COPY . ./

# Restore all packages
RUN dotnet restore

# Build all projects
RUN dotnet publish ColorTurbine.Framework -c Release -o /app
RUN dotnet publish ColorTurbine -c Release -o /app

# Run
CMD ["dotnet", "/app/ColorTurbine.dll"]