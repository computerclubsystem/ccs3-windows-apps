using System.Runtime.InteropServices;

namespace SwitchToDefaultDesktop;

public class WindowsInterop {
    [Flags]
    public enum ACCESS_MASK : uint {
        DELETE = 0x00010000,
        READ_CONTROL = 0x00020000,
        WRITE_DAC = 0x00040000,
        WRITE_OWNER = 0x00080000,
        SYNCHRONIZE = 0x00100000,

        STANDARD_RIGHTS_REQUIRED = 0x000F0000,

        STANDARD_RIGHTS_READ = 0x00020000,
        STANDARD_RIGHTS_WRITE = 0x00020000,
        STANDARD_RIGHTS_EXECUTE = 0x00020000,

        STANDARD_RIGHTS_ALL = 0x001F0000,

        SPECIFIC_RIGHTS_ALL = 0x0000FFFF,

        ACCESS_SYSTEM_SECURITY = 0x01000000,

        MAXIMUM_ALLOWED = 0x02000000,

        GENERIC_READ = 0x80000000,
        GENERIC_WRITE = 0x40000000,
        GENERIC_EXECUTE = 0x20000000,
        GENERIC_ALL = 0x10000000,

        DESKTOP_READOBJECTS = 0x00000001,
        DESKTOP_CREATEWINDOW = 0x00000002,
        DESKTOP_CREATEMENU = 0x00000004,
        DESKTOP_HOOKCONTROL = 0x00000008,
        DESKTOP_JOURNALRECORD = 0x00000010,
        DESKTOP_JOURNALPLAYBACK = 0x00000020,
        DESKTOP_ENUMERATE = 0x00000040,
        DESKTOP_WRITEOBJECTS = 0x00000080,
        DESKTOP_SWITCHDESKTOP = 0x00000100,

        WINSTA_ENUMDESKTOPS = 0x00000001,
        WINSTA_READATTRIBUTES = 0x00000002,
        WINSTA_ACCESSCLIPBOARD = 0x00000004,
        WINSTA_CREATEDESKTOP = 0x00000008,
        WINSTA_WRITEATTRIBUTES = 0x00000010,
        WINSTA_ACCESSGLOBALATOMS = 0x00000020,
        WINSTA_EXITWINDOWS = 0x00000040,
        WINSTA_ENUMERATE = 0x00000100,
        WINSTA_READSCREEN = 0x00000200,

        WINSTA_ALL_ACCESS = 0x0000037F
    }

    public const ACCESS_MASK DesktopAccessRights =
        ACCESS_MASK.DESKTOP_JOURNALRECORD
        | ACCESS_MASK.DESKTOP_JOURNALPLAYBACK
        | ACCESS_MASK.DESKTOP_CREATEWINDOW
        | ACCESS_MASK.DESKTOP_ENUMERATE
        | ACCESS_MASK.DESKTOP_WRITEOBJECTS
        | ACCESS_MASK.DESKTOP_SWITCHDESKTOP
        | ACCESS_MASK.DESKTOP_CREATEMENU
        | ACCESS_MASK.DESKTOP_HOOKCONTROL
        | ACCESS_MASK.DESKTOP_READOBJECTS;


    [DllImport("user32.dll")]
    public static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, ACCESS_MASK dwDesiredAccess, IntPtr lpsa);

    [DllImport("user32.dll")]
    public static extern bool CloseDesktop(IntPtr hDesktop);

    [DllImport("user32.dll")]
    public static extern IntPtr OpenDesktop(string lpszDesktop, int dwFlags, bool fInherit, ACCESS_MASK dwDesiredAccess);

    [DllImport("user32.dll")]
    public static extern IntPtr OpenInputDesktop(int dwFlags, bool fInherit, ACCESS_MASK dwDesiredAccess);

    [DllImport("user32.dll")]
    public static extern bool SwitchDesktop(IntPtr hDesktop);
}
