using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VideoDownloader2.ViewModels;

namespace VideoDownloader2.Views;

public sealed partial class AddItemView : Page
{
    public AddItemView()
    {
        InitializeComponent();
    }

    public AddItemViewModel ViewModel => (AddItemViewModel)DataContext;

    public static Visibility HdrFlagVisiblity(string dynamicRange) =>
        dynamicRange == "HDR10" ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility HdrTextVisibility(string dynamicRange) =>
        dynamicRange is not "HDR10" and not "SDR" and not "" and not null ? Visibility.Visible : Visibility.Collapsed;

    public static string? PrettyResolutionFramerateText(int? width, int? height, float? frameRate) =>
        (width, height) switch
        {
            (null, null) => null,
            (7680, 4320) => $"8K{frameRate:0}",
            (5120, 2880) => $"5K{frameRate:0}",
            (3840, 2160) => $"4K{frameRate:0}",
            (2560, 1440) => $"1440p{frameRate:0}",
            (1920, 1080) => $"1080p{frameRate:0}",
            (1280, 720) => $"720p{frameRate:0}",
            (854, 480) => $"480p{frameRate:0}",
            (640, 360) => $"360p{frameRate:0}",
            (426, 240) => $"240p{frameRate:0}",
            (256, 144) => $"144p{frameRate:0}",
            _ => $"{width}x{height} {frameRate:0}fps"
        };
}
