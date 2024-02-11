using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketClientTest
{
    public class DesktopManager
    {
        private DesktopManagerState _state;

        public DesktopManager()
        {
            _state = new DesktopManagerState
            {
                DesktopAccessRights = WindowsInterop.DesktopAccessRights
            };
        }
        public IntPtr CreateDesktop(string name)
        {
            return WindowsInterop.CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, _state.DesktopAccessRights, IntPtr.Zero);
        }

        public bool CloseDesktopPointer(IntPtr desktopPtr)
        {
            return WindowsInterop.CloseDesktop(desktopPtr);
        }

        public IntPtr GetInputDesktopPointer()
        {
            return WindowsInterop.OpenInputDesktop(0, true, _state.DesktopAccessRights);
        }

        public bool SwitchToDesktop(IntPtr desktopPtr)
        {
            return WindowsInterop.SwitchDesktop(desktopPtr);
        }

        public IntPtr GetDefaultDesktopPointer()
        {
            return WindowsInterop.OpenDesktop("Default", 0, true, _state.DesktopAccessRights);
        }

        public int GetLastError()
        {
            return Marshal.GetLastWin32Error();
        }

        private class DesktopManagerState
        {
            public WindowsInterop.ACCESS_MASK DesktopAccessRights { get; set; }
        }
    }
}
