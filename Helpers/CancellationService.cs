using System.Threading;

namespace SpotiffyWidget.Helpers
{
    public static class CancellationService
    {
        private static CancellationTokenSource _cts = new();

        /// <summary>
        /// Şu anda aktif olan token'ı döner
        /// </summary>
        public static CancellationToken Token => _cts.Token;

        /// <summary>
        /// Yeni bir işlem başlatmadan önce çağrılmalı.
        /// Önceki token'ı dispose eder ve yenisini oluşturur.
        /// </summary>
        public static void Reset()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Şu anda çalışan işlemi iptal eder.
        /// </summary>
        public static void Cancel()
        {
            if (!_cts.IsCancellationRequested)
                _cts.Cancel();
        }
    }
}
