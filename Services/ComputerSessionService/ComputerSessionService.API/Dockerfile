FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/ComputerSessionService/ComputerSessionService.API/ComputerSessionService.API.csproj", "Services/ComputerSessionService/ComputerSessionService.API/"]
COPY ["Services/ComputerSessionService/ComputerSessionService.Application/ComputerSessionService.Application.csproj", "Services/ComputerSessionService/ComputerSessionService.Application/"]
COPY ["Services/ComputerSessionService/ComputerSessionService.Domain/ComputerSessionService.Domain.csproj", "Services/ComputerSessionService/ComputerSessionService.Domain/"]
COPY ["Services/ComputerSessionService/ComputerSessionService.Infrastructure/ComputerSessionService.Infrastructure.csproj", "Services/ComputerSessionService/ComputerSessionService.Infrastructure/"]
COPY ["Common/InternetCafe.Common/InternetCafe.Common.csproj", "Common/InternetCafe.Common/"]
COPY ["Common/InternetCafe.Common.Api/InternetCafe.Common.Api.csproj", "Common/InternetCafe.Common.Api/"]
RUN dotnet restore "Services/ComputerSessionService/ComputerSessionService.API/ComputerSessionService.API.csproj"
COPY . .
WORKDIR "/src/Services/ComputerSessionService/ComputerSessionService.API"
RUN dotnet build "ComputerSessionService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComputerSessionService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComputerSessionService.API.dll"]