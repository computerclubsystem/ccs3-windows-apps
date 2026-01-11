# Build Docker image
- Navigate to the folder where the Ccs3ClientAppWindowsService.csproj file is
- Execute
```bash
docker buildx build --load -t computerclubsystem/client-app-windows-service:dev -f Dockerfile .
```
