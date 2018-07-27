FROM microsoft/dotnet:2.1-runtime
COPY ./app /app
CMD ["dotnet", "app/ColorTurbine.dll"]