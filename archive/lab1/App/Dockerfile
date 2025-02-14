ARG BUILD_CONFIGURATION
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ARG BUILD_CONFIGURATION
RUN echo ${BUILD_CONFIGURATION}
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./*.sln ./
COPY . .

ARG BUILD_CONFIGURATION
RUN echo ${BUILD_CONFIGURATION}

RUN dotnet restore
WORKDIR /src/src/App.API
RUN dotnet build "App.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

ARG BUILD_CONFIGURATION
RUN echo ${BUILD_CONFIGURATION}

FROM build AS publish
RUN dotnet publish "App.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App.API.dll"]
