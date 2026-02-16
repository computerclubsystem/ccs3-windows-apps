# Build Docker image
- Navigate to the folder where the Ccs3ClientApp.csproj file is
- Execute
```bash
docker buildx build --output=type=docker -t computerclubsystem/client-app:dev -f Dockerfile .
```
