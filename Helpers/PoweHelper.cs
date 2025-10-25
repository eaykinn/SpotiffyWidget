using System.Runtime.InteropServices;

namespace SpotiffyWidget.Helpers;



public static class PowerHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern uint SetThreadExecutionState(uint esFlags);

    const uint ES_CONTINUOUS        = 0x80000000;
    const uint ES_SYSTEM_REQUIRED   = 0x00000001;
    const uint ES_DISPLAY_REQUIRED  = 0x00000002;

    /// <summary>
    /// Bilgisayarın uyku moduna geçmesini engeller.
    /// </summary>
    public static void PreventSleep()
    {
        SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
    }

    /// <summary>
    /// Uyku moduna tekrar izin verir (normal davranış).
    /// </summary>
    public static void AllowSleep()
    {
        SetThreadExecutionState(ES_CONTINUOUS);
    }
}