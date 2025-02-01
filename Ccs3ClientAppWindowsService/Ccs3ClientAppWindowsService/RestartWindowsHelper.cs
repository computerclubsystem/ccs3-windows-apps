using System.Diagnostics;
using System;
using System.Runtime.InteropServices;

namespace Ccs3ClientAppWindowsService;

public class RestartWindowsHelper {
    [DllImport("Advapi32.dll", SetLastError = true)]
    static extern bool InitiateSystemShutdownExA(string? lpMachineName, string? lpMessage, int dwTimeout, bool bForceAppsClosed, bool bRebootAfterShutdown, uint dwReason);

    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    internal static extern IntPtr GetCurrentProcess();

    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr
    phtok);

    [DllImport("advapi32.dll", SetLastError = true)]
    internal static extern bool LookupPrivilegeValue(string host, string name,
    ref long pluid);

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TokPriv1Luid {
        public int Count;
        public long Luid;
        public int Attr;
    }

    internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
    internal const int TOKEN_QUERY = 0x00000008;
    internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
    internal const string SE_TIME_ZONE_NAMETEXT = "SeTimeZonePrivilege";

    public bool Restart() {
#if DEBUG
        return true;
#endif
        const uint SHTDN_REASON_MAJOR_APPLICATION = 0x00040000;
        const uint SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001;
        const uint SHTDN_REASON_FLAG_PLANNED = 0x80000000;
        const uint reasonFlags = SHTDN_REASON_MAJOR_APPLICATION | SHTDN_REASON_MINOR_MAINTENANCE | SHTDN_REASON_FLAG_PLANNED;
        EnableShutdownPrivilege();
        return InitiateSystemShutdownExA(null, null, 0, true, true, reasonFlags);
    }

    private void EnableShutdownPrivilege() {
        TokPriv1Luid tp;
        IntPtr hproc = GetCurrentProcess();
        IntPtr htok = IntPtr.Zero;
        OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
        tp.Count = 1;
        tp.Luid = 0;
        tp.Attr = SE_PRIVILEGE_ENABLED;
        string shutdownPrivilegeName = "SeShutdownPrivilege";
        LookupPrivilegeValue(null, shutdownPrivilegeName, ref tp.Luid);
        AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
    }
}
