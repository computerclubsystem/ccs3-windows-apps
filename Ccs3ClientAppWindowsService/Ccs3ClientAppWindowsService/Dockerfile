FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore \
           -p:EnableWindowsTargeting=true
COPY . .
RUN dotnet publish \
           --no-restore \
           -o /app/dist \
           -r win-x64 \
           -c Release \
           -p:EnableWindowsTargeting=true \
           -p:UseAppHost=true \
    && rm /app/dist/aspnetcorev2_inprocess.dll \
    && rm /app/dist/appsettings.Development.json \
    && rm /app/dist/web.config

# Create scratch file-hosting only from the published output
FROM scratch AS artifact
COPY --from=build /app/dist/ /app/dist
