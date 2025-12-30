# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
ARG APP_UID=1000
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project file first to leverage Docker layer caching for restores
COPY ["DivingAPI/DivingAPI.csproj", "DivingAPI/"]
WORKDIR /src/DivingAPI
RUN dotnet restore "DivingAPI.csproj"

# Copy the rest of the sources and build
WORKDIR /src
COPY . .
WORKDIR /src/DivingAPI
RUN dotnet build "DivingAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build


# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/DivingAPI
RUN dotnet publish "DivingAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
ARG APP_UID=1000
# install ICU for proper globalization support
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends libicu-dev \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DivingAPI.dll"]