using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using VideoDownloader2.Models;
using YoutubeDLSharp;

namespace VideoDownloader2.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<VideoDownloadModel> Downloads { get; } = [];

    public void AddNewDownload(VideoDownloadModel downloadModel)
    {
        Downloads.Insert(0, downloadModel);

        var progress = new Progress<DownloadProgress>(dp =>
        {
            // ignore warnings
            if (dp.State is DownloadState.Error && dp.Data.StartsWith("WARNING"))
                return;

            var (progress, state, speedString, etaString) = (dp.Progress, dp.State, dp.DownloadSpeed, dp.ETA);
            var speedMBs = speedString is null || Regex.Match(speedString, @"^(\d+(?:\.\d+))([KMG])iB/s") is not { Success: true } match ? 0
                : float.Parse(match.Groups[1].Value) * match.Groups[2].Value switch
                {
                    "K" => 1.0f / 1024,
                    "M" => 1,
                    "G" => 1024,
                    _ => 0
                };
            if (etaString?.Count(c => c == ':') == 1)
                etaString = $"0:{etaString}";
            if (etaString is null || !TimeSpan.TryParse(etaString, out var eta))
                eta = TimeSpan.Zero;

            App.DispatcherQueue.TryEnqueue(() =>
                (downloadModel.Completion, downloadModel.SpeedMBs, downloadModel.Eta, downloadModel.State) =
                    (progress, speedMBs, eta, state));
        });

        App.YouTubeDL.RunVideoDownload(downloadModel.SourceUri.ToString(),
            downloadModel.FormatData.AudioBitrate > 0 ? downloadModel.FormatData.FormatId : $"{downloadModel.FormatData.FormatId}+bestaudio",
            progress: progress,
            overrideOptions: new()
            {
                Output = downloadModel.DestinationUri.OriginalString,
                EmbedSubs = true,
                SubLangs = string.Join(',', downloadModel.SelectedSubtitles),
            });
    }
}
