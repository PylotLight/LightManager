#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.1-alpine3.14 AS base
WORKDIR /app
#EXPOSE 80
EXPOSE 9001
ENV ASPNETCORE_URLS=http://+:9001
#ENV ASPNETCORE_ENVIRONMENT=Development 
FROM mcr.microsoft.com/dotnet/sdk:6.0.101-alpine3.14 AS build
WORKDIR /src
COPY Server/*.csproj Server/
COPY Client/*.csproj Client/
COPY Shared/*.csproj Shared/
#COPY Server/LightManager.Server.csproj Server/

#COPY ["Shared/LightManager.Shared.csproj", "Shared/"]
#COPY ["Client/LightManager.Client.csproj", "Client/"]
RUN dotnet restore "Server/LightManager.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "LightManager.Server.csproj" -c Release -o /app/build
#
FROM build AS publish
#RUN dotnet publish "LightManager.Server.csproj" -c Release -o /app/publish
RUN dotnet publish "LightManager.Server.csproj" -c Release -o /app/publish -r alpine-x64 --self-contained true --no-restore
#
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./LightManager.Server"]