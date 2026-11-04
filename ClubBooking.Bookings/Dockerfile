FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ClubBooking.Bookings/ClubBooking.Bookings.csproj", "ClubBooking.Bookings/"]
RUN dotnet restore "./ClubBooking.Bookings/ClubBooking.Bookings.csproj"
COPY . .
WORKDIR "/src/ClubBooking.Bookings"
RUN dotnet build "./ClubBooking.Bookings.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ClubBooking.Bookings.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClubBooking.Bookings.dll"]
