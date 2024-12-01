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
}
