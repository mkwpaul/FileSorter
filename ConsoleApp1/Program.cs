// See https://aka.ms/new-console-template for more information
using BetterMessageBox;
using System.Diagnostics;
using System.Windows;

Console.WriteLine("Hello, World!");


var screens = ScreenInterop.AllScreens;

while (true)
{
    PrintMousePosition();

    foreach (var screen in screens)
    {
        PrintScreen(screen);
    }
    var input = Console.ReadLine();
    if (!string.IsNullOrEmpty(input))
        break;
}


var windowHandle = Process.GetCurrentProcess().Handle;

while (true)
{
    Console.WriteLine("Prints MousePosition, Mouse-Monitor and Window Monitor");
    Console.ReadLine();

    PrintMousePosition();

    var screen = ScreenInterop.GetScreenFromMouse();

    Console.WriteLine();
    Console.WriteLine("Print Mouse Screen");
    PrintScreen(screen);


    var windowScreen = ScreenInterop.GetScreenWindowHandle(windowHandle);

    Console.WriteLine();
    Console.WriteLine("Print Window Screen");
    PrintScreen(windowScreen);

}

void PrintMousePosition()
{
    var point = ScreenInterop.GetMousePosition();
    
    Console.WriteLine();
    Console.WriteLine($"point, X = {point.X}, Y = {point.Y}");
}

void PrintScreen(Screen screen)
{
    Console.WriteLine();
    Console.WriteLine(screen.DeviceName);
    Console.WriteLine(screen.Bounds);
    Console.WriteLine(screen.WorkingArea);
    Console.WriteLine(screen.IsPrimary);
    Console.WriteLine();
}


public enum StartupLocation
{
    MouseScreen,
    ParentWindowScreen,
    MousePosition,
}

public static class ScreenMath
{
    public static void SetWindowStartup(Window window, StartupLocation location, IntPtr? windowHandle = null)
    {
        switch (location)
        {
            case StartupLocation.MouseScreen:
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                break;
            case StartupLocation.ParentWindowScreen:
                SetStartupToParentWindow(window, windowHandle!.Value);
                break;
            case StartupLocation.MousePosition:
                SetWindowStartupToMouse(window);
                break;
        }
    }

    public static void SetStartupToParentWindow(Window window, IntPtr parentWindowHandle)
    {
        var screen = ScreenInterop.GetScreenWindowHandle(parentWindowHandle);
        SetWindowStartup(window, screen.WorkingArea.GetCenter(), screen);
    }

    public static void SetWindowStartupToMouse(Window window, Screen? screen = null)
    {
        var point = ScreenInterop.GetMousePosition();
        SetWindowStartup(window, point, screen);
    }

    public static void SetWindowStartup(Window window, Point center, Screen? screen = null)
    {
        screen ??= ScreenInterop.GetScreenFromPoint(center);
        var targetRect = FitToScreen(screen, window, center);
        SetRetange(window, targetRect);
    }

    public static Rect FitToScreen(Screen screen, Window window, Point center)
    {
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        var windowRect = GetRectange(window);
        return FitRectInOther(screen.WorkingArea, windowRect, center);
    }

    public static Rect FitRectInOther(Rect parent, Rect move, Point center)
    {
        if (parent.Width < move.Width)
            move.Width = parent.Width;

        if (parent.Height < move.Height)
            move.Height = parent.Height;

        move.SetCenter(center);

        double horizontalOffset = 0;
        double verticalOffset = 0;
        if (move.Left < parent.Left)
            horizontalOffset = parent.Left - move.Left;
        if (move.Right > parent.Right)
            horizontalOffset = move.Right - parent.Right;
        if (move.Top < parent.Top)
            verticalOffset = parent.Top - move.Top;
        if (move.Bottom > parent.Bottom)
            verticalOffset = move.Bottom - parent.Bottom;

        move.Offset(horizontalOffset, verticalOffset);
        return move;
    }

    public static Rect GetRectange(Window window)
    {
        return new Rect(window.Left, window.Top, window.Width, window.Height);
    }

    public static void SetRetange(Window window, Rect value)
    {
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = value.Left;
        window.Top = value.Top;
        window.Width = value.Width;
        window.Height = value.Height;
    }

    public static Point GetCenter(this Rect rect)
    {
        var x = rect.Left + (rect.Width / 2);
        var y = rect.Bottom + (rect.Height / 2);
        return new Point(x, y);
    }

    public static void SetCenter(this Rect rect, Point center)
    {
        var left = center.X - (rect.Width / 2);
        var top = center.Y - (rect.Height / 2);
        rect.Location = new Point(left, top);
    }
}

