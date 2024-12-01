using Microsoft.UI.Xaml;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VideoDownloader2.Helpers;

public static class ViewBindingHelpers
{
    public static Visibility ToVisibility(this bool value) =>
        value ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility ToInvertedVisibility(this bool value) =>
        value ? Visibility.Collapsed : Visibility.Visible;

    public static Visibility ToVisibility<T>(this IEnumerable<T> source) =>
        source.Any() ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility ToVisibility<T>(this T number) where T : INumber<T> =>
        number != T.AdditiveIdentity ? Visibility.Visible : Visibility.Collapsed;

    public static string? PrettyFileSize(long? sizeBytes) => sizeBytes switch
    {
        null => null,
        < 1024 => $"{sizeBytes}B",
        < 1024L * 1024 => $"{sizeBytes / 1024}KB",
        < 1024L * 1024 * 1024 => $"{sizeBytes / (1024L * 1024L)}MB",
        < 1024L * 1024 * 1024 * 1024 => $"{sizeBytes / (1024L * 1024L * 1024L)}GB",
        _ => $"{sizeBytes / (1024L * 1024L * 1024L * 1024L)}TB"
    };
}
