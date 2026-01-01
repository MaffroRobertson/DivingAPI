# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Stage 2: Build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src
COPY ["DivingAPI/DivingAPI.csproj", "./"]
RUN dotnet restore "DivingAPI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "DivingAPI/DivingAPI.csproj" -c Release -o /app/build




# Stage 3: Publish the app
FROM build AS publish
RUN dotnet publish "DivingAPI/DivingAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false


# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DivingAPI.dll"]