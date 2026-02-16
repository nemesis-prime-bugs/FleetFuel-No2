FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first for better caching
COPY *.csproj ./
RUN dotnet restore ./

# Copy everything else
COPY . ./

# Publish
RUN dotnet publish . -c Release -o /publish -p:PublishSingleFile=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /publish

# Create data directory for SQLite
RUN mkdir -p /data && chmod 777 /data

# Copy published files
COPY --from=build /publish .

# Configure for Render
ENV ASPNETCORE_URLS=http://*:$PORT
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE $PORT

ENTRYPOINT ["dotnet", "FleetFuel.Api.dll"]
