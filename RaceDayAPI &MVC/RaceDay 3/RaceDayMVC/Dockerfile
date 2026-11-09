# syntax=docker/dockerfile:1
# Build context is the solution root:  docker build -f RaceDayMVC/Dockerfile -t raceday-mvc .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy the project file first so the restore layer is cached until dependencies change.
COPY RaceDayMVC/RaceDayMVC.csproj RaceDayMVC/
RUN dotnet restore RaceDayMVC/RaceDayMVC.csproj
COPY RaceDayMVC/ RaceDayMVC/
RUN dotnet publish RaceDayMVC/RaceDayMVC.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# The container serves plain HTTP on 8080 (TLS is terminated outside the container),
# so the app's own HTTP-to-HTTPS redirect is switched off here.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    Hosting__UseHttpsRedirection=false
EXPOSE 8080
# Run as the non-root user built into the .NET images.
USER $APP_UID
ENTRYPOINT ["dotnet", "RaceDayMVC.dll"]
