using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp2
{
    public partial class AddWindow : Window
    {
        public string _title { get; private set; }
        public string _path { get; private set; }

        public AddWindow()
        {
            _title = String.Empty;
            _path = String.Empty;
            InitializeComponent();
        }

        private void BrowseAndAdd(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "image files (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg";
            if (openFileDialog.ShowDialog() == true)
            {
                _path = openFileDialog.FileName;
                Preview.Source = new BitmapImage(new Uri(_path));
            }
        }

        private void AddPlaylist(object sender, RoutedEventArgs e)
        {
            _title = PlaylistNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(_title))
            {
                return;
            }

            DialogResult = true;
        }

        private void Cancel(object sender, RoutedEventArgs e) { Close(); }
    }
}
