using System.Runtime.InteropServices;

namespace Ccs3ClientAppWindowsService;

public static class ClientAppProcessController {
    #region Win32 Constants

    private const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;
    private const int CREATE_NO_WINDOW = 0x08000000;

    private const int CREATE_NEW_CONSOLE = 0x00000010;

    private const uint INVALID_SESSION_ID = 0xFFFFFFFF;
    private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
    private const int STARTF_USESHOWWINDOW = 0x00000001;

    #endregion

    #region DllImports

    [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    //[LibraryImport("advapi32.dll", EntryPoint ="CreateProcessAsUser", SetLastError = true)]
    private static extern bool CreateProcessAsUser(
        IntPtr hToken,
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandle,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
    //[LibraryImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
    static extern bool DuplicateTokenEx(
        IntPtr ExistingTokenHandle,
        uint dwDesiredAccess,
        IntPtr lpThreadAttributes,
        int TokenType,
        int ImpersonationLevel,
        ref IntPtr DuplicateTokenHandle);

    [DllImport("userenv.dll", SetLastError = true)]
    //[LibraryImport("userenv.dll", SetLastError = true)]
    static extern bool CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

    [DllImport("userenv.dll", SetLastError = true)]
    //[LibraryImport("userenv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

    [DllImport("kernel32.dll", SetLastError = true)]
    //[LibraryImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hSnapshot);

    [DllImport("kernel32.dll")]
    //[LibraryImport("kernel32.dll")]
    static extern uint WTSGetActiveConsoleSessionId();

    [DllImport("Wtsapi32.dll")]
    //[LibraryImport("Wtsapi32.dll")]
    static extern uint WTSQueryUserToken(uint SessionId, ref IntPtr phToken);

    [DllImport("wtsapi32.dll", SetLastError = true)]
    //[LibraryImport("wtsapi32.dll", SetLastError = true)]
    static extern int WTSEnumerateSessions(
        IntPtr hServer,
        int Reserved,
        int Version,
        ref IntPtr ppSessionInfo,
        ref int pCount);

    [DllImport("Wtsapi32.dll")]
    //[LibraryImport("Wtsapi32.dll")]
    static extern void WTSFreeMemory(IntPtr ppSessionInfo);

    #endregion

    #region Win32 Structs

    private enum SW {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_NORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_MAXIMIZE = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_SHOWDEFAULT = 10,
        SW_MAX = 10
    }

    public enum WTS_CONNECTSTATE_CLASS {
        WTSActive,
        WTSConnected,
        WTSConnectQuery,
        WTSShadow,
        WTSDisconnected,
        WTSIdle,
        WTSListen,
        WTSReset,
        WTSDown,
        WTSInit
    }

    private enum SECURITY_IMPERSONATION_LEVEL {
        SecurityAnonymous = 0,
        SecurityIdentification = 1,
        SecurityImpersonation = 2,
        SecurityDelegation = 3,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct STARTUPINFO {
        public int cb;
        public String lpReserved;
        public String lpDesktop;
        public String lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    private enum TOKEN_TYPE {
        TokenPrimary = 1,
        TokenImpersonation = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WTS_SESSION_INFO {
        public readonly UInt32 SessionID;

        [MarshalAs(UnmanagedType.LPStr)]
        public readonly String pWinStationName;

        public readonly WTS_CONNECTSTATE_CLASS State;
    }

    #endregion

    public static List<WTS_SESSION_INFO> GetSessions() {
        List<WTS_SESSION_INFO> result = new();

        //var bResult = false;
        var hImpersonationToken = IntPtr.Zero;
        var activeSessionId = INVALID_SESSION_ID;
        var pSessionInfo = IntPtr.Zero;
        var sessionCount = 0;

        // Get a handle to the user access token for the current active session.
        if (WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref pSessionInfo, ref sessionCount) != 0) {
            var arrayElementSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            var current = pSessionInfo;

            for (var i = 0; i < sessionCount; i++) {
                WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)current, typeof(WTS_SESSION_INFO))!;
                result.Add(si);
                current += arrayElementSize;

                if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive) {
                    activeSessionId = si.SessionID;
                }
            }

            WTSFreeMemory(pSessionInfo);
        }

        return result;
    }

    // Gets the user token from the currently active session
    private static bool GetSessionUserToken(ref IntPtr phUserToken) {
        var bResult = false;
        var hImpersonationToken = IntPtr.Zero;
        var activeSessionId = INVALID_SESSION_ID;
        var pSessionInfo = IntPtr.Zero;
        var sessionCount = 0;

        // Get a handle to the user access token for the current active session.
        if (WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref pSessionInfo, ref sessionCount) != 0) {
            var arrayElementSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            var current = pSessionInfo;

            for (var i = 0; i < sessionCount; i++) {
                WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)current, typeof(WTS_SESSION_INFO))!;
                current += arrayElementSize;
                if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive) {
                    activeSessionId = si.SessionID;
                }
            }

            WTSFreeMemory(pSessionInfo);
        }

        // If enumerating did not work, fall back to the old method
        if (activeSessionId == INVALID_SESSION_ID) {
            activeSessionId = WTSGetActiveConsoleSessionId();
        }
        var err0 = Marshal.GetLastSystemError();

        var tokenResult = WTSQueryUserToken(activeSessionId, ref hImpersonationToken);
        if (tokenResult != 0) {
            // Convert the impersonation token to a primary token
            bResult = DuplicateTokenEx(hImpersonationToken, 0, IntPtr.Zero,
                (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, (int)TOKEN_TYPE.TokenPrimary,
                ref phUserToken);

            CloseHandle(hImpersonationToken);
        }

        return bResult;
    }

    public static StartProcessAsCurrentUserResult StartProcessAsCurrentUser(string appPath, string? cmdLine = null, string? workDir = null, bool visible = true, ILogger? logger = null) {
        StartProcessAsCurrentUserResult result = new();
        var hUserToken = IntPtr.Zero;
        var startInfo = new STARTUPINFO();
        var procInfo = new PROCESS_INFORMATION();
        result.ProcInfo = procInfo;
        var pEnv = IntPtr.Zero;

        startInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));

        try {
            if (!GetSessionUserToken(ref hUserToken)) {
                result.Success = false;
                result.LastErrors = GetLastErrors();
                logger?.LogError("GetSessionUserToken failed");
                return result;
            }

            uint dwCreationFlags = CREATE_UNICODE_ENVIRONMENT | (uint)(visible ? CREATE_NEW_CONSOLE : CREATE_NO_WINDOW);
            startInfo.dwFlags = STARTF_USESHOWWINDOW;
            startInfo.wShowWindow = (short)(visible ? SW.SW_SHOW : SW.SW_HIDE);
            startInfo.lpDesktop = "winsta0\\default";

            if (!CreateEnvironmentBlock(ref pEnv, hUserToken, false)) {
                result.Success = false;
                result.LastErrors = GetLastErrors();
                logger?.LogError("CreateEnvironmentBlock failed");
                return result;
            }

            if (workDir != null) {
                Directory.SetCurrentDirectory(workDir);
            }

            if (!CreateProcessAsUser(hUserToken,
                appPath, // Application Name
                cmdLine, // Command Line
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                dwCreationFlags,
                pEnv,
                workDir, // Working directory
                ref startInfo,
                out procInfo)
            ) {
                result.Success = false;
                result.LastErrors = GetLastErrors();
                logger?.LogError("CreateProcessAsUser failed");
                return result;
            }
            result.LastErrors = GetLastErrors();
        } finally {
            CloseHandle(hUserToken);
            if (pEnv != IntPtr.Zero) {
                DestroyEnvironmentBlock(pEnv);
            }
            //CloseHandle(procInfo.hThread);
            //CloseHandle(procInfo.hProcess);
        }


        result.ProcInfo = procInfo;
        result.Success = result.ProcInfo.hProcess != 0;
        logger?.LogTrace("StartProcessAsCurrentUserResult - hProcess: {0}, hProcess != 0: {1}", result.ProcInfo.hProcess, result.ProcInfo.hProcess != 0);
        return result;
    }

    public static void CloseProcInfoHandles(PROCESS_INFORMATION procInfo) {
        CloseHandle(procInfo.hThread);
        CloseHandle(procInfo.hProcess);
    }

    private static Tuple<string, int>[] GetLastErrors() {
        Tuple<string, int>[] errors = new[] {
            new Tuple<string,int>("GetLastWin32Error", Marshal.GetLastWin32Error()),
            new Tuple<string,int>("GetLastSystemError", Marshal.GetLastSystemError()),
            new Tuple<string,int>("GetLastPInvokeError", Marshal.GetLastPInvokeError()),
            new Tuple<string,int>("GetHRForLastWin32Error", Marshal.GetHRForLastWin32Error()),
        };
        return errors;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct PROCESS_INFORMATION {
    public IntPtr hProcess;
    public IntPtr hThread;
    public uint dwProcessId;
    public uint dwThreadId;
}

public class StartProcessAsCurrentUserResult {
    public bool Success { get; set; }
    public Tuple<string, int>[] LastErrors { get; set; }
    public PROCESS_INFORMATION ProcInfo { get; set; }
}