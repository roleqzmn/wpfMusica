using Microsoft.Win32;
using System.Windows;
using WpfApp2.ViewModels;

namespace WpfApp2
{
    public partial class AddWindow : Window
    {
    private PlaylistEditViewModel? _currentViewModel;

        public AddWindow()
        {
            InitializeComponent();
            DataContextChanged += AddWindow_DataContextChanged;
        }

        private void AddWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_currentViewModel is not null)
            {
            _currentViewModel.CloseRequested -= OnRequestClose;
            }

        _currentViewModel = e.NewValue as PlaylistEditViewModel;
            if (_currentViewModel is not null)
            {
            _currentViewModel.CloseRequested += OnRequestClose;
            }
        }

        private void OnRequestClose(bool? dialogResult)
        {
            DialogResult = dialogResult;
        }
    }
}
