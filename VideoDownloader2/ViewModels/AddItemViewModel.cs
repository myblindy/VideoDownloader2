using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nito.AsyncEx;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoDownloader2.Models;
using VideoDownloader2.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using YoutubeDLSharp.Metadata;

namespace VideoDownloader2.ViewModels;

public partial class AddItemViewModel(SettingsService settingsService) : ObservableObject
{
    [ObservableProperty]
    bool querying;

    [ObservableProperty]
    string? name;

    [ObservableProperty]
    TimeSpan duration;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadPressedCommand))]
    string? destinationPath;

    [ObservableProperty]
    FormatData? selectedFormat;

    public ObservableCollection<FormatData> Formats { get; } = [];

    public ObservableCollection<SelectableSubtitleModel> Subtitles { get; } = [];

    readonly AsyncAutoResetEvent
        downloadPressedResetEvent = new(),
        cancelPressedResetEvent = new();

    public async Task<VideoDownloadModel?> GetDownloadDetailsAsync()
    {
        Uri? sourceUri = null;

        try
        {
            Querying = true;

            var clipboardContent = Clipboard.GetContent();
            if (clipboardContent.Contains(StandardDataFormats.Uri))
                sourceUri = await clipboardContent.GetUriAsync();
            else if (clipboardContent.Contains(StandardDataFormats.Text))
            {
                var text = await clipboardContent.GetTextAsync();
                if (Uri.TryCreate(text, UriKind.Absolute, out var uri))
                    sourceUri = uri;
            }

            if (sourceUri is null)
                return null;

            var ytData = await App.YouTubeDL.RunVideoDataFetch(sourceUri.ToString());
            if (!ytData.Success || ytData.Data.Duration is null)
                return null;

            Name = ytData.Data.Title;
            Duration = TimeSpan.FromSeconds(ytData.Data.Duration.Value);

            var formatsRoot = ytData.Data.Formats.AsEnumerable();
            if (formatsRoot.Any() && formatsRoot.Any(f => f.FrameRate is not null))
                formatsRoot = formatsRoot.Where(f => f.FrameRate is not null);

            Formats.Clear();
            foreach (var format in formatsRoot
                .OrderByDescending(f => f.FrameRate)
                    .ThenBy(f => f.DynamicRange)
                    .ThenByDescending(f => f.Width * f.Height)
                    .ThenByDescending(f => f.ContainerFormat))
            {
                Formats.Add(format);
            }

            // use approximate file size if necessary
            foreach (var f in Formats)
                f.FileSize ??= f.ApproximateFileSize
                    ?? (long)((ytData.Data.Duration ?? 0) * (f.Bitrate ?? f.VideoBitrate ?? 0) * 1024 / 8);

            SelectedFormat = Formats.FirstOrDefault();

            Subtitles.Clear();
            foreach (var subtitle in ytData.Data.Subtitles.Select(x => x.Key).Order())
                Subtitles.Add(new()
                {
                    Name = subtitle,
                    Selected = EngSubtitleNameRegex.IsMatch(subtitle)
                });

            var fn = Regex.Replace(Name, $@"({string.Join('|', Path.GetInvalidFileNameChars().Select(c => Regex.Escape(c.ToString())))})+", " ")
                + "." + ytData.Data.Extension;
            DestinationPath = string.IsNullOrWhiteSpace(settingsService.LastDestingationFolder) ? fn : Path.Combine(settingsService.LastDestingationFolder, fn);
        }
        finally
        {
            Querying = false;
        }

        var downloadPressedTask = downloadPressedResetEvent.WaitAsync();
        var cancelPressedTask = cancelPressedResetEvent.WaitAsync();
        if (await Task.WhenAny(downloadPressedTask, cancelPressedTask) == cancelPressedTask)
            return null;

        settingsService.LastDestingationFolder = Path.GetDirectoryName(DestinationPath);
        return new(sourceUri, new Uri(DestinationPath!, UriKind.RelativeOrAbsolute), SelectedFormat!,
            Subtitles.Where(w => w.Selected).Select(w => w.Name!), Name, Duration);
    }

    [RelayCommand(CanExecute = nameof(DownloadPressedCanExecute))]
    void DownloadPressed() => downloadPressedResetEvent.Set();

    bool DownloadPressedCanExecute() =>
        !string.IsNullOrWhiteSpace(DestinationPath) && SelectedFormat is not null
            && Uri.TryCreate(DestinationPath, UriKind.RelativeOrAbsolute, out _);

    [RelayCommand]
    void CancelPressed() => cancelPressedResetEvent.Set();

    [RelayCommand]
    async Task BrowseDestinationPath()
    {
        var picker = new FileSavePicker
        {
            FileTypeChoices =
            {
                { "Video files", [".mp4", ".mkv", ".mov", ".webm"] }
            },
            SuggestedFileName = DestinationPath
        };
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.MainWindowHandle);
        if (await picker.PickSaveFileAsync() is not { } file)
            return;

        DestinationPath = file.Path;
    }

    [GeneratedRegex(@"^eng?(?:\b|[_])")]
    private static partial Regex EngSubtitleNameRegex { get; }
}

public partial class SelectableSubtitleModel : ObservableObject
{
    [ObservableProperty]
    string? name;

    [ObservableProperty]
    bool selected;
}
