FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y curl

# Unset this var, as http config is handled by the kestrel section of appsettings.
# The aspnet:8.0 image this set to 8080 out of the box, but we don't want that.
# Rider shows an error here, but this does in fact work correctly.
ENV ASPNETCORE_HTTP_PORTS=

USER $APP_UID
WORKDIR /app
EXPOSE 4317
EXPOSE 4318


FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src
COPY ["OddDotNet.csproj", "."]
RUN dotnet restore --arch $TARGETARCH "OddDotNet.csproj"
COPY . .
RUN dotnet build --arch $TARGETARCH "OddDotNet.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src
RUN dotnet publish --arch $TARGETARCH "OddDotNet.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK CMD curl --fail http://localhost:4318/healthz || exit
ENTRYPOINT ["dotnet", "OddDotNet.dll"]
