using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class DesktopHelper
{
    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    public static void SetAsDesktopChild(Window window)
    {
        IntPtr hWnd = new WindowInteropHelper(window).Handle;
        IntPtr hProgman = FindWindow("Progman", null);
        SetParent(hWnd, hProgman);
    }
}
