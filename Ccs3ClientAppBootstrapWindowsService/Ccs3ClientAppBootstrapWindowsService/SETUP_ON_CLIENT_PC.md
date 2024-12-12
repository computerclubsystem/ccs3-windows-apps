-The CCS3 Client App Windows services are using network connections - if firewall is used it should be configured to allow applications in the CCS3 folder to connect to external machines and also to be able to listen for connections (Ccs3ClientAppWindowsService is listening so the client app can connect to it - this communication happens only between applications running on the computer)
- Download https://192.168.6.9:65449/Ccs3ClientAppBootstrapWindowService.zip and extract it
- Start "cmd.exe" as Administrator and navigate to `"C:\Program Files"`:
```bash
cd "C:\Program Files"
```
- Create `CCS3`` folder
```bash
md CCS3
```
- Navigate to `CCS3`
```bash
cd CCS3
```
- Create `BootstrapWindowsService` folder
```bash
md BootstrapWindowsService
```
- Navigate to `BootstrapWindowsService`
```bash
cd BootstrapWindowsService
```
- Copy extracted files in the current folder
```bash
copy "<the folder where Ccs3ClientAppBootstrapWindowService.zip is extracted>\*.*" .
```
- Delete the Ccs3ClientAppBootstrapWindowsService files and folders
- Register the service
```bash
sc create Ccs3ClientAppBootstrapWindowsService DisplayName= "CCS3 Client App Bootstrap Windows Service" start= auto obj= LocalSystem binpath= "C:\Program Files\CCS3\ClientAppBootstrapWindowsService\Ccs3ClientAppBootstrapWindowsService.exe"
```
- Add system environment variable `CCS3_STATIC_FILES_SERVICE_BASE_URL` with value `https://<host name or ip address of static files server>:<port>`
- Add system environment variable `CCS3_PC_CONNECTOR_SERVICE_BASE_URL` with value `https://<host name or ip address of PC connector server>:<port>`
- Add system environment variable `CCS3_CLIENT_APP_WINDOWS_SERVICE_LOCAL_BASE_URL` with value `https://127.0.0.1:<port>` or `https://localhost:<port>` - the port should be available on the client PC - for example use value 30000 or greater. Also trusted certificate with specified `127.0.0.1` or `localhost` CN must exist in the local computer certificate storage to enable CCS3 Client App Windows Service to function correctly
- Start "mmc.exe" as administrator
- Add the "Services" snap in and find the service `CCS3 Client App Bootstrap Windows Service`
- Configure the `Ccs3ClientAppBootstrapWindowsService` to restart always with 0 for "Restart service after"
- Start the service and verify whether there is `C:\Program Files\CCS3\ClientAppBootstrapWindowsService\downloads` folder with downloaded and extracted files
- Stop the service `Ccs3ClientAppBootstrapWindowsService`
- Delete the `downloads` folder
- Another service will be created - `Ccs3ClientAppWindowsService` - stop it and remove it with `sc delete Ccs3ClientAppWindowsService` then delete its folder `C:\Program Files\CCS3\Ccs3ClientAppWindowsService`
- Commit the changes