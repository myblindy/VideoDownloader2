using Microsoft.UI.Xaml.Controls;
using VideoDownloader2.ViewModels;

namespace VideoDownloader2.Views;

public sealed partial class MainView : Page
{
    public MainView()
    {
        InitializeComponent();
    }

    public MainViewModel ViewModel => (MainViewModel)DataContext;
}
