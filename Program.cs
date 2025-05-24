using Avalonia;
using System;
using Avalonia.Diagnostics;
using Percuro.Services;

namespace Percuro;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
   [STAThread]
    public static void Main(string[] args)
    {
        // Starte alle Hintergrunddienste professionell
        StartupService.StartBackgroundServices();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
  
            
}
