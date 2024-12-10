# Publish
Either simply:
```bash
dotnet publish
```
Or use preconfigured publish profile:
```bash
dotnet publish -p:PublishProfile=FolderProfile
```

# Build Docker image
```bash
docker buildx build -t ccs3/caws-static-files:latest -f Dockerfile bin\Release\net9.0-windows\win-x64\publish
```