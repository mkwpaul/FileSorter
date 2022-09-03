using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BetterMessageBox;


public class Screen : IEquatable<Screen>
{
    private const int PRIMARY_MONITOR = unchecked((int)0xBAADF00D);
    private const int MONITORINFOF_PRIMARY = 0x00000001;

    private readonly IntPtr hmonitor;

    internal Screen(IntPtr monitor) : this(monitor, IntPtr.Zero)
    {
    }

    internal Screen(IntPtr monitor, IntPtr hdc)
    {
        if (!ScreenInterop.HasMultiMonitorSupport || monitor == (IntPtr)PRIMARY_MONITOR)
        {
            Bounds = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            IsPrimary = true;
            DeviceName = "DISPLAY";
        }
        else
        {
            var info = new MONITORINFOEX();

            GetMonitorInfo(new HandleRef(null, monitor), info);

            Bounds = new Rect(
                info.rcMonitor.left, info.rcMonitor.top,
                info.rcMonitor.right - info.rcMonitor.left,
                info.rcMonitor.bottom - info.rcMonitor.top);

            IsPrimary = (info.dwFlags & MONITORINFOF_PRIMARY) != 0;
            DeviceName = new string(info.szDevice).TrimEnd((char)0);

        }
        hmonitor = monitor;
    }

    /// <summary>
    /// Gets the bounds of the display.
    /// </summary>
    /// <returns>A <see cref="T:System.Windows.Rect" />, representing the bounds of the display.</returns>
    public Rect Bounds { get; private set; }

    /// <summary>
    /// Gets the device name associated with a display.
    /// </summary>
    /// <returns>The device name associated with a display.</returns>
    public string DeviceName { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a particular display is the primary device.
    /// </summary>
    /// <returns>true if this display is primary; otherwise, false.</returns>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Gets the working area of the display. The working area is the desktop area of the display, excluding taskbars, docked windows, and docked tool bars.
    /// </summary>
    /// <returns>A <see cref="T:System.Windows.Rect" />, representing the working area of the display.</returns>
    public Rect WorkingArea
    {
        get
        {
            if (!ScreenInterop.HasMultiMonitorSupport || hmonitor == (IntPtr)PRIMARY_MONITOR)
                return SystemParameters.WorkArea;

            var info = new MONITORINFOEX();
            GetMonitorInfo(new HandleRef(null, hmonitor), info);
            return new Rect(
                info.rcWork.left, info.rcWork.top,
                info.rcWork.right - info.rcWork.left,
                info.rcWork.bottom - info.rcWork.top);
        }
    }


    public override bool Equals(object? obj)
    {
        return obj is Screen monitor && hmonitor == monitor.hmonitor;
    }

    public override int GetHashCode()
    {
        return (int)hmonitor;
    }

    public bool Equals(Screen? other)
    {
        return Equals(other);
    }

    public static bool operator ==(Screen a, Screen b)
    {
        return a?.hmonitor == b?.hmonitor;
    }

    public static bool operator !=(Screen a, Screen b)
    {
        return a?.hmonitor != b?.hmonitor;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX info);
}



/// <summary>
/// Represents a display device or multiple display devices on a single system.
/// See https://github.com/micdenny/WpfScreenHelper/
/// </summary>
internal class ScreenInterop
{
    
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern bool EnumDisplayMonitors(HandleRef hdc, COMRECT rcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern IntPtr MonitorFromWindow(HandleRef handle, int flags);

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool SystemParametersInfo(int nAction, int nParam, ref RECT rc, int nUpdate);

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern IntPtr MonitorFromPoint(POINTSTRUCT pt, int flags);

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    private static extern bool GetCursorPos([In, Out] POINT pt);

    private const int SM_CMONITORS = 80;
    private const int PRIMARY_MONITOR = unchecked((int)0xBAADF00D);
    private const int MONITOR_DEFAULTTONEAREST = 0x00000002;
    public static bool HasMultiMonitorSupport { get; }

    static ScreenInterop()
    {
        HasMultiMonitorSupport = GetSystemMetrics(SM_CMONITORS) != 0;
    }

    public static IEnumerable<Screen> AllScreens
    {
        get
        {
            if (HasMultiMonitorSupport)
            {
                var closure = new MonitorEnumCallback();
                //var proc = new MonitorEnumProc(closure.Callback);

                EnumDisplayMonitors(default, null, closure.Callback, IntPtr.Zero);

                if (closure.Screens.Count > 0)
                    return closure.Screens.Select(x => x);
            }

            return new[] { new Screen((IntPtr)PRIMARY_MONITOR) };
        }
    }

    public static Screen? PrimaryScreen
    {
        get
        {
            if (HasMultiMonitorSupport)
            {
                return AllScreens.FirstOrDefault(t => t.IsPrimary);
            }
            return new Screen((IntPtr)PRIMARY_MONITOR);
        }
    }

    public static Point GetMousePosition()
    {
        var point = new POINT();
        bool success = GetCursorPos(point);

        if (success)
            return new Point(point.x, point.y);

        return default;
    }


    /// <summary>
    /// Retrieves a Screen for the display that contains the largest portion of the specified control.
    /// </summary>
    /// <param name="hwnd">The window handle for which to retrieve the Screen.</param>
    /// <returns>A Screen for the display that contains the largest region of the object. In multiple display environments where no display contains any portion of the specified window, the display closest to the object is returned.</returns>
    public static Screen GetScreenWindowHandle(IntPtr hwnd)
    {
        if (HasMultiMonitorSupport)
        {
            return new Screen(MonitorFromWindow(new HandleRef(null, hwnd), 2));
        }
        return new Screen((IntPtr)PRIMARY_MONITOR);
    }

    /// <summary>
    /// Retrieves a Screen for the display that contains the specified point.
    /// </summary>
    /// <param name="point">A <see cref="T:System.Windows.Point" /> that specifies the location for which to retrieve a Screen.</param>
    /// <returns>A Screen for the display that contains the point. In multiple display environments where no display contains the point, the display closest to the specified point is returned.</returns>
    public static Screen GetScreenFromPoint(Point point)
    {
        if (!HasMultiMonitorSupport)
            return new Screen((IntPtr)PRIMARY_MONITOR);

        var pt = new POINTSTRUCT((int)point.X, (int)point.Y);
        return new Screen(MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST));
    }



    public static Screen GetScreenFromMouse()
    {
        if (!HasMultiMonitorSupport)
            return new Screen((IntPtr)PRIMARY_MONITOR);

        var point = new POINT();
        bool success = GetCursorPos(point);

        var pt = new POINTSTRUCT(point.x, point.y);
        return new Screen(MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST));
    }
}

public class MonitorEnumCallback
{
    public List<Screen> Screens { get; private set; }

    public MonitorEnumCallback()
    {
        Screens = new();
    }

    public bool Callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lparam)
    {
        Screens.Add(new Screen(monitor, hdc));
        return true;
    }
}

public delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam);


[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;

    public RECT(int left, int top, int right, int bottom)
    {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
    }

    public RECT(Rect r)
    {
        left = (int)r.Left;
        top = (int)r.Top;
        right = (int)r.Right;
        bottom = (int)r.Bottom;
    }

    public static RECT FromXYWH(int x, int y, int width, int height)
    {
        return new RECT(x, y, x + width, y + height);
    }

    public Size Size
    {
        get { return new Size(right - left, bottom - top); }
    }
}

// use this in cases where the Native API takes a POINT not a POINT*
// classes marshal by ref.
[StructLayout(LayoutKind.Sequential)]
internal struct POINTSTRUCT
{
    public int x;
    public int y;
    public POINTSTRUCT(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal class POINT
{
    public int x;
    public int y;

    public POINT()
    {
    }

    public POINT(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return "{x=" + x + ", y=" + y + "}";
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
internal class MONITORINFOEX
{
    internal int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
    internal RECT rcMonitor = new RECT();
    internal RECT rcWork = new RECT();
    internal int dwFlags = 0;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] internal char[] szDevice = new char[32];
}

[StructLayout(LayoutKind.Sequential)]
internal class COMRECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;

    public COMRECT()
    {
    }

    public COMRECT(Rect r)
    {
        left = (int)r.X;
        top = (int)r.Y;
        right = (int)r.Right;
        bottom = (int)r.Bottom;
    }

    public COMRECT(int left, int top, int right, int bottom)
    {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
    }

    public static COMRECT FromXYWH(int x, int y, int width, int height)
    {
        return new COMRECT(x, y, x + width, y + height);
    }

    public override string ToString()
    {
        return "Left = " + left + " Top " + top + " Right = " + right + " Bottom = " + bottom;
    }
}