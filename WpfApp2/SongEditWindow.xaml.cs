using System.Windows;
using WpfApp2.ViewModels;

namespace WpfApp2;

public partial class SongEditWindow : Window
{
    private SongEditViewModel? _currentViewModel;

    public SongEditWindow()
    {
        InitializeComponent();
        DataContextChanged += SongEditWindow_DataContextChanged;
    }

    private void SongEditWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_currentViewModel is not null)
        {
            _currentViewModel.CloseRequested -= OnCloseRequested;
        }

        _currentViewModel = e.NewValue as SongEditViewModel;
        if (_currentViewModel is not null)
        {
            _currentViewModel.CloseRequested += OnCloseRequested;
        }
    }

    private void OnCloseRequested(bool? dialogResult)
    {
        DialogResult = dialogResult;
    }
}
