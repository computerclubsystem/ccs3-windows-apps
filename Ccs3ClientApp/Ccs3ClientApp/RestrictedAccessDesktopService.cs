using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ccs3ClientApp;

internal class RestrictedAccessDesktopService {
    private RestrictedAccessDesktopServiceState _state;

    public RestrictedAccessDesktopService() {
        _state = new RestrictedAccessDesktopServiceState {
            DesktopAccessRights = WindowsInterop.DesktopAccessRights,
            DesktopReadAccessRights = WindowsInterop.DesktopEnumerateAccessRights,
            DesktopSwitchAccessRights = WindowsInterop.DesktopSwitchAccessRights,
        };
    }

    public IntPtr CreateRestrictedAccessDesktop(string desktopName) {
        IntPtr existingPointer = OpenDesktop(desktopName);
        if (existingPointer == IntPtr.Zero) {
            _state.RestrictedAccessDesktopPointer = CreateDesktop(desktopName);
        } else {
            _state.RestrictedAccessDesktopPointer = existingPointer;
        }
        return _state.RestrictedAccessDesktopPointer;
    }

    public IntPtr GetRestrictedAccessDesktopPointer() {
        return _state.RestrictedAccessDesktopPointer;
    }

    public IntPtr OpenDesktop(string desktopName) {
        return WindowsInterop.OpenDesktop(desktopName, 0, true, _state.DesktopAccessRights);
    }

    public void SetCurrentThreadDesktop(IntPtr desktopPtr) {
        WindowsInterop.SetThreadDesktop(desktopPtr);
    }

    public bool HasDesktop(string name) {
        IntPtr ptr = WindowsInterop.OpenDesktop(name, 0, true, _state.DesktopReadAccessRights);
        if (ptr != IntPtr.Zero) {
            CloseDesktopPointer(ptr);
            return true;
        } else {
            return false;
        }
    }

    public bool SwitchToRestrictedAccessDesktop() {
        if (_state.RestrictedAccessDesktopPointer == IntPtr.Zero) {
            return false;
        }
        return SwitchToDesktop(_state.RestrictedAccessDesktopPointer);
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
        return WindowsInterop.OpenDesktop("Default", 0, true, _state.DesktopSwitchAccessRights);
    }

    public bool CloseDesktop(IntPtr desktopPointer) {
        return WindowsInterop.CloseDesktop(desktopPointer);
    }

    public int GetLastError() {
        return Marshal.GetLastWin32Error();
    }

    private class RestrictedAccessDesktopServiceState {
        public WindowsInterop.ACCESS_MASK DesktopAccessRights { get; set; }
        public WindowsInterop.ACCESS_MASK DesktopReadAccessRights { get; set; }
        public WindowsInterop.ACCESS_MASK DesktopSwitchAccessRights { get; set; }
        public IntPtr RestrictedAccessDesktopPointer;
        public IntPtr DefaultDesktopPointer;
    }
}
