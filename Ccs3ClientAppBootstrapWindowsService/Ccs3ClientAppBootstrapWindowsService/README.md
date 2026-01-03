# Build Docker image
- Navigate to the folder where the Ccs3ClientAppWindowsService.csproj file is
- Execute
```bash
docker buildx build --no-cache -t computerclubsystem/client-app-bootstrap-windows-service:dev -f Dockerfile .
```