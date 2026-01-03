FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app
COPY . .
# Publish and clean up unnecessary files from the output
RUN dotnet publish -r win-x64 -c Release -o dist -p:UseAppHost=true \
    && rm dist/aspnetcorev2_inprocess.dll \
    && rm dist/appsettings.Development.json \
    && rm dist/web.config

# Create scratch file-hosting only from the published output
FROM scratch AS artifact
COPY --from=build /app/dist/ /app/dist