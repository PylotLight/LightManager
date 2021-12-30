#FROM alpine:latest AS base
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 44316

ENV ASPNETCORE_URLS=http://+:44316

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
#RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
#USER appuser

#FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
#WORKDIR /src
#COPY ["Server/LightManager.Server.csproj", "Server/"]
#RUN dotnet restore "Server/LightManager.Server.csproj"
#COPY . .
#WORKDIR "/src/Server"
#RUN dotnet build "LightManager.Server.csproj" -c Release -o /app/build

#FROM build AS publish
#RUN dotnet publish "LightManager.Server.csproj" -c Release -o /app/publish

#FROM base AS final
WORKDIR /app
COPY ./Server/bin/Release/net6.0/publish .
#RUN chmod 777 "./LightManager.Server"
ENTRYPOINT ["./LightManager.Server"]
#D:\Documents\Scripting\CS\LightManager> docker build -t lightmanager:linux-demo -f Server/Dockerfile .