FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ClubBooking.Seats/ClubBooking.Seats.csproj", "ClubBooking.Seats/"]
RUN dotnet restore "./ClubBooking.Seats/ClubBooking.Seats.csproj"
COPY . .
WORKDIR "/src/ClubBooking.Seats"
RUN dotnet build "./ClubBooking.Seats.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ClubBooking.Seats.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClubBooking.Seats.dll"]
