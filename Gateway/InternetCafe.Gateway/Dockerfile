FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Gateway/InternetCafe.Gateway/InternetCafe.Gateway.csproj", "Gateway/InternetCafe.Gateway/"]
RUN dotnet restore "Gateway/InternetCafe.Gateway/InternetCafe.Gateway.csproj"
COPY . .
WORKDIR "/src/Gateway/InternetCafe.Gateway"
RUN dotnet build "InternetCafe.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InternetCafe.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InternetCafe.Gateway.dll"]