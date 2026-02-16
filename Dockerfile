FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/*.csproj backend/
RUN dotnet restore backend/
COPY backend/ backend/
RUN dotnet publish backend/ -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /publish
COPY --from=build /publish .
RUN mkdir -p /data && chmod 777 /data
ENV ASPNETCORE_URLS=http://*:$PORT
ENV DATABASE_CONNECTION_STRING=Data Source=/data/fleetfuel.db
EXPOSE $PORT
ENTRYPOINT ["dotnet", "FleetFuel.Api.dll"]
