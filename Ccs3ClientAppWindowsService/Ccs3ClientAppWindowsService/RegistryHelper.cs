using Microsoft.Win32;

namespace Ccs3ClientAppWindowsService;

public class RegistryHelper {
    public void ChangeTaskManagerAvailability(bool available) {
        using var localMachineRegKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
        using var taskMgrKey = localMachineRegKey.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\taskmgr.exe", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
        // ":" is a dummy value - it just needs to be something that is pointing to non-existent file
        // to disable starting the Task Manager
        taskMgrKey.SetValue("Debugger", available ? string.Empty : ":");
    }
}
