using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class BlurHelper
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attr,
        ref int attrValue,
        int attrSize
    );

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_MICA_EFFECT = 1029;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

    public static void EnableMicaEffect(Window window, int isDarkMode, int backdropType)
    {
        var windowHelper = new WindowInteropHelper(window);
        IntPtr hwnd = windowHelper.Handle;

        // Windows 11 kontrolü (isteğe bağlı)
        if (Environment.OSVersion.Version.Build < 22000)
        {
            // Windows 11 değilse, Acrylic blur kullan
            EnableAcrylicBlur(window);
            return;
        }

        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref isDarkMode, sizeof(int));

        // Mica effect'i etkinleştir
        //int micaEffect = 1; // 1 = Mica, 0 = kapalı
        //DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref micaEffect, sizeof(int));

        // Alternatif yöntem (Windows 11 22H2+)
        // 1 = Mica, 2 = Mica Alt, 3 = Acrylic, 4 = Tabbed
        DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
    }

    // Windows 10 için fallback
    public static void EnableAcrylicBlur(Window window)
    {
        var windowHelper = new WindowInteropHelper(window);

        var accent = new AccentPolicy
        {
            AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
            GradientColor = 0xCC1E1E1E,
        };

        var accentStructSize = Marshal.SizeOf(accent);
        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = accentStructSize,
            Data = accentPtr,
        };

        SetWindowCompositionAttribute(windowHelper.Handle, ref data);
        Marshal.FreeHGlobal(accentPtr);
    }

    // Önceki struct ve enum'lar
    [DllImport("user32.dll")]
    internal static extern int SetWindowCompositionAttribute(
        IntPtr hwnd,
        ref WindowCompositionAttributeData data
    );

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19,
    }

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public uint GradientColor;
        public int AnimationId;
    }
}
