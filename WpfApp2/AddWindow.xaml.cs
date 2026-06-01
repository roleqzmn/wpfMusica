using Microsoft.Win32;
using System.Windows;
using WpfApp2.ViewModels;

namespace WpfApp2
{
    public partial class AddWindow : Window
    {
        private AddPlaylistViewModel? _currentViewModel;

        public AddWindow()
        {
            InitializeComponent();
            DataContextChanged += AddWindow_DataContextChanged;
        }

        private void AddWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_currentViewModel is not null)
            {
                _currentViewModel.RequestClose -= OnRequestClose;
            }

            _currentViewModel = e.NewValue as AddPlaylistViewModel;
            if (_currentViewModel is not null)
            {
                _currentViewModel.RequestClose += OnRequestClose;
            }
        }

        private void OnRequestClose(bool? dialogResult)
        {
            DialogResult = dialogResult;
        }
    }
}
