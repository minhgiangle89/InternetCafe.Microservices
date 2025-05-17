FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Services/AccountService/AccountService.API/AccountService.API.csproj", "Services/AccountService/AccountService.API/"]
COPY ["Services/AccountService/AccountService.Application/AccountService.Application.csproj", "Services/AccountService/AccountService.Application/"]
COPY ["Services/AccountService/AccountService.Domain/AccountService.Domain.csproj", "Services/AccountService/AccountService.Domain/"]
COPY ["Services/AccountService/AccountService.Infrastructure/AccountService.Infrastructure.csproj", "Services/AccountService/AccountService.Infrastructure/"]
COPY ["Common/InternetCafe.Common/InternetCafe.Common.csproj", "Common/InternetCafe.Common/"]
COPY ["Common/InternetCafe.Common.Api/InternetCafe.Common.Api.csproj", "Common/InternetCafe.Common.Api/"]

# Restore dependencies
RUN dotnet restore "Services/AccountService/AccountService.API/AccountService.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/Services/AccountService/AccountService.API"
RUN dotnet build "AccountService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccountService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "AccountService.API.dll"]