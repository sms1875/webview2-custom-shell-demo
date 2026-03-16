using System;
using System.Runtime.InteropServices;

namespace CustomShellWebView2Demo
{
    internal static class NativeMethods
    {
        internal const int WM_NCHITTEST = 0x0084;
        internal const int WM_NCLBUTTONDOWN = 0x00A1;
        internal const int HTCLIENT = 1;
        internal const int HTCAPTION = 2;
        internal const int HTLEFT = 10;
        internal const int HTRIGHT = 11;
        internal const int HTTOP = 12;
        internal const int HTTOPLEFT = 13;
        internal const int HTTOPRIGHT = 14;
        internal const int HTBOTTOM = 15;
        internal const int HTBOTTOMLEFT = 16;
        internal const int HTBOTTOMRIGHT = 17;
        internal const int CS_DROPSHADOW = 0x00020000;
        internal const int MONITOR_DEFAULTTONEAREST = 2;
        internal const int WM_GETMINMAXINFO = 0x0024;

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        [DllImport("gdi32.dll", ExactSpelling = true)]
        internal static extern IntPtr CreateRoundRectRgn(
            int leftRect,
            int topRect,
            int rightRect,
            int bottomRect,
            int widthEllipse,
            int heightEllipse);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool redraw);

        [DllImport("user32.dll")]
        internal static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
    }
}
