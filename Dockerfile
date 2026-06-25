# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy and restore
COPY MovieApi/MovieApi.csproj MovieApi/
RUN dotnet restore MovieApi/MovieApi.csproj

# Copy everything and publish
COPY . ./
RUN dotnet publish MovieApi/MovieApi.csproj -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MovieApi.dll"]
