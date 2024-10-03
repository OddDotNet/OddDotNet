FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 4317

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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
ENV ASPNETCORE_URLS=http://+:4317
ENTRYPOINT ["dotnet", "OddDotNet.dll"]
