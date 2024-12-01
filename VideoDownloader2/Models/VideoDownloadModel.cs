using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace VideoDownloader2.Models;

public partial class VideoDownloadModel(Uri sourceUri, Uri destinationUri, FormatData formatData,
    IEnumerable<string> subtitles, string name, TimeSpan duration)
    : ObservableObject
{
    public Uri SourceUri { get; } = sourceUri;
    public Uri DestinationUri { get; } = destinationUri;
    public FormatData FormatData { get; } = formatData;
    public string Name { get; } = name;
    public TimeSpan Duration { get; } = duration;
    public ImmutableArray<string> SelectedSubtitles { get; } = [.. subtitles];

    [ObservableProperty]
    float completion;

    [ObservableProperty]
    float speedMBs;

    [ObservableProperty]
    TimeSpan eta;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Completed))]
    DownloadState state;

    public bool Completed =>
        State is DownloadState.Error or DownloadState.Success;
}
