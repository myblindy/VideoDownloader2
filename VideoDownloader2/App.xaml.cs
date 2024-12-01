using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using VideoDownloader2.Services;
using VideoDownloader2.ViewModels;
using YoutubeDLSharp;

namespace VideoDownloader2;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }
    public static IntPtr MainWindowHandle { get; private set; }
    public static DispatcherQueue DispatcherQueue { get; } = DispatcherQueue.GetForCurrentThread();

    public static YoutubeDL YouTubeDL { get; } = new()
    {
        FFmpegPath = "runtime/ffmpeg.exe",
        OverwriteFiles = true,
        YoutubeDLPath = "runtime/yt-dlp.exe"
    };

    static readonly IHost host = Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices(ctx => ctx
                .AddSingleton<SettingsService>()

                .AddSingleton<MainViewModel>()
                .AddSingleton<AddItemViewModel>())
            .Build();

    public static T? GetService<T>() => host.Services.GetService<T>();
    public static T GetRequiredService<T>() where T : notnull =>
        host.Services.GetRequiredService<T>();

    public App()
    {
        InitializeComponent();
        RequestedTheme = ApplicationTheme.Dark;
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
        MainWindow.Activate();
    }
}
