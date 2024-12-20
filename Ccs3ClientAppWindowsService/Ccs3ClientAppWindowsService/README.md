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

## On Windows
- Using `nerdtcl` (using `--namespace buildkit` is necessary to allow using this image from other Dockerfile `FROM` commands, because the image is local)
```bash
nerdctl --namespace buildkit build -t ccs3/caws-static-files:latest -f Dockerfile bin\Release\net9.0-windows\win-x64\publish
```
- Using `docker`
```bash
docker buildx build -t ccs3/caws-static-files:latest -f Dockerfile bin\Release\net9.0-windows\win-x64\publish
```

## On Linux
- Using `nerdtcl` (using `--namespace buildkit` is necessary to allow using this image from other Dockerfile `FROM` commands, because the image is local)
```bash
nerdctl --namespace buildkit build -t ccs3/caws-static-files:latest -f Dockerfile bin/Release/net9.0-windows/win-x64/publish
```
- Using `docker`
```bash
docker buildx build -t ccs3/caws-static-files:latest -f Dockerfile bin/Release/net9.0-windows/win-x64/publish
```