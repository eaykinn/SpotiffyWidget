using System.Threading;

namespace SpotiffyWidget.Helpers
{
    public static class CancellationService
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        public static CancellationToken Token => _cts.Token;

        public static void Reset()
        {
            // Yeni bir CTS oluşturup atomik olarak değiştir, sonra eskiyi iptal et+dispose et.
            var newCts = new CancellationTokenSource();
            var old = System.Threading.Interlocked.Exchange(ref _cts, newCts);
            if (old != null)
            {
                try
                {
                    old.Cancel();
                }
                catch { }
                old.Dispose();
            }
        }

        public static void Cancel()
        {
            var cts = _cts;
            if (cts == null)
                return;
            try
            {
                cts.Cancel();
            }
            catch { }
        }
    }
}
