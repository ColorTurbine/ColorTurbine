FROM microsoft/dotnet:2.1-sdk

# Copy all source
COPY . ./

# Build app
WORKDIR /ColorTurbine
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build tests
WORKDIR /ColorTurbine.tests
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Return to release
WORKDIR /ColorTurbine

# Run
CMD ["dotnet", "out/ColorTurbine.dll"]
