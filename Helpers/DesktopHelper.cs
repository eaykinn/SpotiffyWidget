using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class DesktopHelper
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindowEx(
        IntPtr parentHandle,
        IntPtr childAfter,
        string className,
        string windowTitle
    );

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint Msg,
        IntPtr wParam,
        IntPtr lParam,
        SendMessageTimeoutFlags fuFlags,
        uint uTimeout,
        out IntPtr lpdwResult
    );

    [DllImport("user32.dll")]
    static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    const uint WM_SPAWN_WORKERW = 0x052C;

    [Flags]
    enum SendMessageTimeoutFlags : uint
    {
        SMTO_NORMAL = 0x0,
        SMTO_BLOCK = 0x1,
        SMTO_ABORTIFHUNG = 0x2,
        SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
    }

    public static void SetAsDesktopChild(Window window)
    {
        IntPtr progman = FindWindow("Progman", null);
        SendMessageTimeout(
            progman,
            WM_SPAWN_WORKERW,
            IntPtr.Zero,
            IntPtr.Zero,
            SendMessageTimeoutFlags.SMTO_NORMAL,
            1000,
            out _
        );

        IntPtr workerw = IntPtr.Zero;
        IntPtr defView = IntPtr.Zero;
        do
        {
            workerw = FindWindowEx(IntPtr.Zero, workerw, "WorkerW", null);
            defView = FindWindowEx(workerw, IntPtr.Zero, "SHELLDLL_DefView", null);
        } while (defView == IntPtr.Zero && workerw != IntPtr.Zero);

        IntPtr hWnd = new WindowInteropHelper(window).Handle;
        SetParent(hWnd, workerw);
    }
}
