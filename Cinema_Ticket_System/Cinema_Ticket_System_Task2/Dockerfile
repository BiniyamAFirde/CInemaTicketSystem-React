#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CinemaTicketSystem.csproj", "."]
COPY .config .config
RUN dotnet restore "./CinemaTicketSystem.csproj"


COPY . .
WORKDIR "/src/."
RUN dotnet build "CinemaTicketSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CinemaTicketSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM build AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CinemaTicketSystem.dll"]
