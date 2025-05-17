FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/StatisticsService/StatisticsService.API/StatisticsService.API.csproj", "Services/StatisticsService/StatisticsService.API/"]
COPY ["Services/StatisticsService/StatisticsService.Application/StatisticsService.Application.csproj", "Services/StatisticsService/StatisticsService.Application/"]
COPY ["Services/StatisticsService/StatisticsService.Domain/StatisticsService.Domain.csproj", "Services/StatisticsService/StatisticsService.Domain/"]
COPY ["Services/StatisticsService/StatisticsService.Infrastructure/StatisticsService.Infrastructure.csproj", "Services/StatisticsService/StatisticsService.Infrastructure/"]
COPY ["Common/InternetCafe.Common/InternetCafe.Common.csproj", "Common/InternetCafe.Common/"]
COPY ["Common/InternetCafe.Common.Api/InternetCafe.Common.Api.csproj", "Common/InternetCafe.Common.Api/"]
RUN dotnet restore "Services/StatisticsService/StatisticsService.API/StatisticsService.API.csproj"
COPY . .
WORKDIR "/src/Services/StatisticsService/StatisticsService.API"
RUN dotnet build "StatisticsService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatisticsService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StatisticsService.API.dll"]