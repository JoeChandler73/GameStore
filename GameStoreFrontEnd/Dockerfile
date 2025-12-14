# GameStoreFrontEnd/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["GameStoreFrontEnd/GameStoreFrontEnd.csproj", "GameStoreFrontEnd/"]
COPY ["GameStoreServiceDefaults/GameStoreServiceDefaults.csproj", "GameStoreServiceDefaults/"]
RUN dotnet restore "GameStoreFrontEnd/GameStoreFrontEnd.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/GameStoreFrontEnd"
RUN dotnet build "GameStoreFrontEnd.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameStoreFrontEnd.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameStoreFrontEnd.dll"]

# run from the root GameStore directory
# docker buildx create --use --name multi-arch
# docker buildx build --platform linux/amd64,linux/arm64 --push -t joechandler73/gamestorefrontend -f GameStoreFrontEnd/Dockerfile .
# docker buildx rm multi-arch
