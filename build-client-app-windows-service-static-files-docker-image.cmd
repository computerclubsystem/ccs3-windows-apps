REM The starting folder for this script must contain Ccs3ClientAppWindowsService folder

cd Ccs3ClientAppWindowsService\Ccs3ClientAppWindowsService
git pull
dotnet publish
docker buildx build -t ccs3/caws-static-files:latest -f Dockerfile bin\Release\net9.0-windows\win-x64\publish

REM navigate back to the starting folder
cd ..\..\
