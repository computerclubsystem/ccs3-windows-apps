using System.Runtime.InteropServices;

namespace Ccs3ClientAppWindowsService;

public class RestartWindowsHelper {
    [DllImport("Advapi32.dll", SetLastError = true)]
    static extern bool InitiateSystemShutdownExA(
      string? lpMachineName,
      string? lpMessage,
      int dwTimeout,
      bool bForceAppsClosed,
      bool bRebootAfterShutdown,
      uint dwReason
    );

    public bool Restart() {
#if DEBUG
        return true;
#endif
        const uint SHTDN_REASON_MAJOR_APPLICATION = 0x00040000;
        const uint SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001;
        const uint SHTDN_REASON_FLAG_PLANNED = 0x80000000;
        const uint reasonFlags = SHTDN_REASON_MAJOR_APPLICATION | SHTDN_REASON_MINOR_MAINTENANCE | SHTDN_REASON_FLAG_PLANNED;

        return InitiateSystemShutdownExA(null, null, 0, true, true, reasonFlags);
    }
}
