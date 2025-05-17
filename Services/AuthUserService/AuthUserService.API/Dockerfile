FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Services/AuthUserService/AuthUserService.API/AuthUserService.API.csproj", "Services/AuthUserService/AuthUserService.API/"]
COPY ["Services/AuthUserService/AuthUserService.Application/AuthUserService.Application.csproj", "Services/AuthUserService/AuthUserService.Application/"]
COPY ["Services/AuthUserService/AuthUserService.Domain/AuthUserService.Domain.csproj", "Services/AuthUserService/AuthUserService.Domain/"]
COPY ["Services/AuthUserService/AuthUserService.Infrastructure/AuthUserService.Infrastructure.csproj", "Services/AuthUserService/AuthUserService.Infrastructure/"]
COPY ["Common/InternetCafe.Common/InternetCafe.Common.csproj", "Common/InternetCafe.Common/"]
COPY ["Common/InternetCafe.Common.Api/InternetCafe.Common.Api.csproj", "Common/InternetCafe.Common.Api/"]

# Restore dependencies
RUN dotnet restore "Services/AuthUserService/AuthUserService.API/AuthUserService.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/Services/AuthUserService/AuthUserService.API"
RUN dotnet build "AuthUserService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthUserService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "AuthUserService.API.dll"]