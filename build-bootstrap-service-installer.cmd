REM The starting folder for this script must contain Ccs3ClientAppBootstrapWindowsService and Ccs3ClientAppBootstrapWindowsServiceInstaller folders

REM navigate to the folder where the Ccs3ClientAppBootstraWindowsService.proj file is located, git pull and dotnet publish
cd Ccs3ClientAppBootstrapWindowsService\Ccs3ClientAppBootstrapWindowsService
git pull
dotnet publish
REM navigate to the published folder and make Ccs3ClientAppBootstrapWindowsService.zip file from the executable and its configuration
cd bin\Release\net9.0-windows\publish\win-x64\
tar -a -c -f Ccs3ClientAppBootstrapWindowsService.zip Ccs3ClientAppBootstrapWindowsService.exe appsettings.json

REM navigate back to the starting folder
cd ..\..\..\..\..\..\..

REM navigate to the folder where the Ccs3ClientAppBootstrapWindowsServiceInstaller.proj file is located, git pull and dotnet publish
cd Ccs3ClientAppBootstrapWindowsServiceInstaller\Ccs3ClientAppBootstrapWindowsServiceInstaller
git pull
dotnet publish
REM navigate to the published folder and copy Ccs3ClientAppBootstrapWindowsService.zip from the previous "dotnet publish" and create Ccs3ClientAppBootstrapWindowsServiceInstaller.zip from Ccs3ClientAppBootstrapWindowsService.zip and Ccs3ClientAppBootstrapWindowsServiceInstaller.exe
cd bin\Release\net9.0-windows\publish\win-x64\
copy ..\..\..\..\..\..\..\Ccs3ClientAppBootstrapWindowsService\Ccs3ClientAppBootstrapWindowsService\bin\Release\net9.0-windows\publish\win-x64\Ccs3ClientAppBootstrapWindowsService.zip .
tar -a -c -f Ccs3ClientAppBootstrapWindowsServiceInstaller.zip Ccs3ClientAppBootstrapWindowsServiceInstaller.exe Ccs3ClientAppBootstrapWindowsService.zip

REM navigate back to the starting folder
cd ..\..\..\..\..\..\..\

echo off
echo --------------------------------
echo --------------------------------
echo --------------------------------
echo:
echo The Ccs3ClientAppBootstrapWindowsServiceInstaller.zip file has been created at:
echo:
echo -----
echo:
echo Ccs3ClientAppBootstrapWindowsServiceInstaller\Ccs3ClientAppBootstrapWindowsServiceInstaller\bin\Release\net9.0-windows\publish\win-x64\Ccs3ClientAppBootstrapWindowsServiceInstaller.zip
echo:
echo -----
echo Extract it at client PC and execute the Ccs3ClientAppBootstrapWindowsServiceInstaller.exe as administrator
echo --------------------------------
echo --------------------------------
echo --------------------------------
echo on