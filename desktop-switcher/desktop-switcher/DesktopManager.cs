using System.Runtime.InteropServices;

namespace SwitchToDefaultDesktop;

public class DesktopManager {
    private DesktopManagerState _state;

    public DesktopManager() {
        _state = new DesktopManagerState {
            DesktopAccessRights = WindowsInterop.DesktopAccessRights
        };
    }
    public bool CreateSecureDesktop(string name) {
        _state.SecureDesktopPointer = CreateDesktop(name);
        return _state.SecureDesktopPointer != IntPtr.Zero;
    }

    public bool SwitchToSecureDesktop() {
        if (_state.SecureDesktopPointer == IntPtr.Zero) {
            return false;
        }
        return SwitchToDesktop(_state.SecureDesktopPointer);
    }

    public bool SwitchToDefaultDesktop() {
        if (_state.DefaultDesktopPointer == IntPtr.Zero) {
            _state.DefaultDesktopPointer = GetDefaultDesktopPointer();
        }
        return SwitchToDesktop(_state.DefaultDesktopPointer);
    }

    public IntPtr CreateDesktop(string name) {
        return WindowsInterop.CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, _state.DesktopAccessRights, IntPtr.Zero);
    }

    public bool CloseDesktopPointer(IntPtr desktopPtr) {
        return WindowsInterop.CloseDesktop(desktopPtr);
    }

    public IntPtr GetInputDesktopPointer() {
        return WindowsInterop.OpenInputDesktop(0, true, _state.DesktopAccessRights);
    }

    public bool SwitchToDesktop(IntPtr desktopPtr) {
        return WindowsInterop.SwitchDesktop(desktopPtr);
    }

    public IntPtr GetDefaultDesktopPointer() {
        return WindowsInterop.OpenDesktop("Default", 0, true, _state.DesktopAccessRights);
    }

    public int GetLastError() {
        return Marshal.GetLastWin32Error();
    }

    private class DesktopManagerState {
        public WindowsInterop.ACCESS_MASK DesktopAccessRights { get; set; }
        public IntPtr SecureDesktopPointer;
        public IntPtr DefaultDesktopPointer;
    }
}
