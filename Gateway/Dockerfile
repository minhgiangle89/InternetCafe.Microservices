FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Gateway/InternetCafe.Gateway/InternetCafe.Gateway.csproj", "Gateway/InternetCafe.Gateway/"]

# Restore dependencies
RUN dotnet restore "Gateway/InternetCafe.Gateway/InternetCafe.Gateway.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/Gateway/InternetCafe.Gateway"
RUN dotnet build "InternetCafe.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InternetCafe.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "InternetCafe.Gateway.dll"]