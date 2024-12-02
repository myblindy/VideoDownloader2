using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using VideoDownloader2.ViewModels;
using VideoDownloader2.Views;

namespace VideoDownloader2;

[ObservableObject]
public sealed partial class MainWindow : Window
{
    readonly MainViewModel mainViewModel = App.GetRequiredService<MainViewModel>();
    readonly MainView mainView;

    readonly AddItemViewModel addItemViewModel = App.GetRequiredService<AddItemViewModel>();
    readonly AddItemView addItemView;

    [ObservableProperty]
    bool updating = true;

    public MainWindow()
    {
        addItemView = new() { DataContext = addItemViewModel, Visibility = Visibility.Collapsed };
        mainView = new() { DataContext = mainViewModel };

        InitializeComponent();

        AppWindow.SetIcon("Assets/WindowIcon.ico");

        ExtendsContentIntoTitleBar = true;

        async Task initializeAsync()
        {
            await App.YouTubeDL.RunUpdate();
            Updating = false;

            PageViewHost.Children.Add(mainView);
            PageViewHost.Children.Add(addItemView);
        }
        _ = initializeAsync();
    }

    [RelayCommand]
    async Task Paste()
    {
        try
        {
            mainView.Visibility = Visibility.Collapsed;
            addItemView.Visibility = Visibility.Visible;
            if (await addItemViewModel.GetDownloadDetailsAsync() is { } details)
                mainViewModel.AddNewDownload(details);
        }
        finally
        {
            mainView.Visibility = Visibility.Visible;
            addItemView.Visibility = Visibility.Collapsed;
        }
    }
}
