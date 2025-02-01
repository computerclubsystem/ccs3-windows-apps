using System.Runtime.InteropServices;

namespace Ccs3ClientAppWindowsService;

public class RestartWindowsHelper {
    [DllImport("user32.dll", SetLastError = true)]
    static extern int ExitWindowsEx(uint uFlags, uint dwReason);

    public void Restart() {
#if DEBUG
        return;
#endif
        const ushort EWX_REBOOT = 0x00000002;
        const ushort EWX_FORCE = 0x00000004;
        const uint exitFlags = EWX_REBOOT | EWX_FORCE;

        const uint SHTDN_REASON_MAJOR_APPLICATION = 0x00040000;
        const uint SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001;
        const uint SHTDN_REASON_FLAG_PLANNED = 0x80000000;
        const uint reasonFlags = SHTDN_REASON_MAJOR_APPLICATION | SHTDN_REASON_MINOR_MAINTENANCE | SHTDN_REASON_FLAG_PLANNED;
        
        ExitWindowsEx(exitFlags, reasonFlags);
    }
}
