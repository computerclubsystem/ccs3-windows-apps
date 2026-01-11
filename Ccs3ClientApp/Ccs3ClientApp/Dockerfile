FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
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
           -p:EnableWindowsTargeting=true

# Create scratch file-hosting only from the published output
FROM scratch AS artifact
COPY --from=build /app/dist/ /app/dist