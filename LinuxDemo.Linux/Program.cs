using Microsoft.Maui.Hosting;
using Platform.Maui.Linux.Gtk4.Platform;

namespace LinuxDemo.Linux;

/// <summary>
/// GTK entry point for the Linux head of the MAUI app. The shared App / MainPage
/// classes come from the LinuxDemo project — Linux is just another head, like
/// Android or iOS.
/// </summary>
public sealed class Program : GtkMauiApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public static void Main(string[] args)
    {
        var app = new Program();
        app.Run(args);
    }
}
