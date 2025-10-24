using System;
using Microsoft.Win32;

namespace SpotiffyWidget.Helpers
{
    public static class SystemEventHelper
    {
        // Bu event'ler üzerinden dışarıya bilgi geçebilirsin
        public static event Action<string>? OnSystemEvent;

        private static bool _isListening = false;

        /// <summary>
        /// Olayları dinlemeye başlar.
        /// </summary>
        public static void StartListening()
        {
            if (_isListening)
                return;

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            _isListening = true;
        }

        /// <summary>
        /// Dinlemeyi durdurur (önemli: bellek sızıntısı olmaması için).
        /// </summary>
        public static void StopListening()
        {
            if (!_isListening)
                return;

            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            _isListening = false;
        }

        private static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    OnSystemEvent?.Invoke("SessionLock");
                    break;

                case SessionSwitchReason.SessionUnlock:
                    OnSystemEvent?.Invoke("SessionUnlock");
                    break;
            }
        }

        private static void SystemEvents_PowerModeChanged(
            object sender,
            PowerModeChangedEventArgs e
        )
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    OnSystemEvent?.Invoke("Sleep");
                    break;

                case PowerModes.Resume:
                    OnSystemEvent?.Invoke("Wake");
                    break;
            }
        }
    }
}
