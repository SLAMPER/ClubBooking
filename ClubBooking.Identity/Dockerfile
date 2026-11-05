FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ClubBooking.Identity/ClubBooking.Identity.csproj", "ClubBooking.Identity/"]
RUN dotnet restore "./ClubBooking.Identity/ClubBooking.Identity.csproj"
COPY . .
WORKDIR "/src/ClubBooking.Identity"
RUN dotnet build "./ClubBooking.Identity.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ClubBooking.Identity.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClubBooking.Identity.dll"]
