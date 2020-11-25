#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /app
# Copy csproj and restore as distinct layers
COPY src/KafkaDemo/*.csproj .
RUN dotnet restore
# Copy everything else and build
COPY src/KafkaDemo .
RUN dotnet build -c Release

# publish the API
FROM build AS publish
WORKDIR /app
RUN dotnet publish -c Release -o out

# run the api
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/out ./
ENTRYPOINT ["dotnet", "KafkaDemo.dll"]