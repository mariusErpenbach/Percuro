using System.Threading.Tasks;
using Percuro.Services;

namespace Percuro.Services;

public static class StartupService
{
    private static bool _started = false;
    public static void StartBackgroundServices()
    {
        if (_started) return;
        _started = true;
        // Starte Login-API im Hintergrund
        Task.Run(() => LoginApiHost.Start());
        // Hier können später weitere Hintergrunddienste gestartet werden
    }
}
