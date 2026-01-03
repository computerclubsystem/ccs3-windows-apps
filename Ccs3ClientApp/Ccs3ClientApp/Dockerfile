FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
# Publish
RUN mkdir dist \
    && dotnet publish -o dist -r win-x64 -c Release -p:EnableWindowsTargeting=true

# Create scratch file-hosting only from the published output
FROM scratch AS artifact
COPY --from=build /app/dist/ /app/dist