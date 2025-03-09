# Build
- Verify if you have installed .NET 9.0 SDK by executiong the following command in command prompt:
```bash
dotnet --version
```
- You should get something like `9.0.102`
- If you don't already have it, you need to install .NET 9.0 SDK - https://dotnet.microsoft.com/en-us/download
- Open command prompt and navigate to the directory where the `Ccs3HttpToUdpProxyWindowsService.csproj` file is
- Execute the following command:
```bash
dotnet publish
```
- If you get errors like "dotnet is not recognized as an internal or external command", you need to install .NET 9.0 SDK - https://dotnet.microsoft.com/en-us/download
- The result can be found in `bin\Release\net9.0\win-x64\publish`

# Install as Windows service
- Create new folder in `C:\Program Files\CCS3\HttpToUdpProxyWindowsService` and paste all the files from `bin\Release\net9.0\win-x64\publish`
- Open command prompt as administrator and navigate to `C:\Program Files\CCS3\HttpToUdpProxyWindowsService`
- Execute the following command:
```bash
sc create "Ccs3HttpToUdpProxyWindowsService" DisplayName= "CCS3 Http To Udp Proxy Windows Service" start= auto obj= LocalSystem binpath= "C:\Program Files\CCS3\HttpToUdpProxyWindowsService\Ccs3HttpToUdpProxyWindowsService.exe"
```
- Go to Windows Services, right click on the service and select "Properties"
- Open "Recovery" tab and set the three drop-downs to "Restart the Service". Optionally you can set value 0 for "Restart service after ... (minutes)" to restart the service immediatelly if it crashes 
- Start the service