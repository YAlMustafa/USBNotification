using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace USBNotification
{
    public static class USBNotification
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DEV_BROADCAST_PORT
        {
            public int dbcp_size;
            public int dbcp_devicetype;
            public int dbcp_reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string dbcp_name;
        }

        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVTYP_PORT = 0x00000003;

        public static event USBDeviceConnectionChangedHandler? DeviceConnectionChanged;

        public static void SubmitWindow(Window window)
        {
            if (window.IsLoaded)
                HwndSource.FromHwnd(new WindowInteropHelper(window).Handle).AddHook(WndProc);
            else
                window.Loaded += new RoutedEventHandler((sender, e) => HwndSource.FromHwnd(new WindowInteropHelper(window).Handle).AddHook(WndProc));
        }

        private static nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            if (msg == WM_DEVICECHANGE)
            {
                switch ((int)wParam)
                {
                    case DBT_DEVICEARRIVAL:
                        HandleDeviceChange(lParam, USBDeviceActions.PluggedIn);
                        break;
                    case DBT_DEVICEREMOVECOMPLETE:
                        HandleDeviceChange(lParam, USBDeviceActions.PluggedOut);
                        break;
                }
            }
            return nint.Zero;
        }

        private static void HandleDeviceChange(nint lParam, USBDeviceActions action)
        {
            DEV_BROADCAST_HDR? hdr = (DEV_BROADCAST_HDR?)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR));
            if (hdr is null) return;

            if (hdr.Value.dbch_devicetype == DBT_DEVTYP_PORT)
            {
                DEV_BROADCAST_PORT? port = (DEV_BROADCAST_PORT?)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_PORT));
                if (port is not null)
                {
                    string port_name = port.Value.dbcp_name;
                    DeviceConnectionChanged?.Invoke(new object(), new USBDeviceActionArgs(port_name, action));
                }
            }
        }
    }
}
