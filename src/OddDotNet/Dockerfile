FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 4317

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OddDotNet.csproj", "."]
RUN dotnet restore "OddDotNet.csproj"
COPY . .
RUN pwd
RUN ls
RUN dotnet build "OddDotNet.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN pwd
RUN ls
RUN dotnet publish "OddDotNet.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN pwd
RUN ls
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:4317
ENTRYPOINT ["dotnet", "OddDotNet.dll"]
