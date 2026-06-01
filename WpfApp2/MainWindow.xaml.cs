using System.Text;
using System.Collections.ObjectModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<PlaylistItem> _playlists = new();
        private readonly SongItem[] _songs =
        {
            new(1, "Kyoto", "Nebulite", "Night Drive", "Synthwave", new TimeSpan(0, 2, 24), "/Resources/vinyl.ico"),
            new(2, "In Flight", "Alegend", "Deep Sky", "Electronic", new TimeSpan(0, 2, 56), "/Resources/vinyl.ico"),
            new(3, "City Lights", "Nova Pulse", "Urban Echoes", "Lo-fi", new TimeSpan(0, 3, 19), "/Resources/vinyl.ico"),
            new(4, "Far Away", "The Waves", "Blue Horizon", "Pop", new TimeSpan(0, 3, 42), "/Resources/vinyl.ico"),
            new(5, "Morning Coffee", "Lofi District", "Easy Beats", "Chillhop", new TimeSpan(0, 2, 38), "/Resources/vinyl.ico")
        };

        public MainWindow()
        {
            InitializeComponent();
            PlaylistsList.ItemsSource = _playlists;
            SongsDataGrid.ItemsSource = _songs;
            SongsDataGrid.SelectedIndex = 0;
            UpdateCurrentSongBar(_songs[0]);
        }

        private void OpenAddWindow(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow { Owner = this };
            if (addWindow.ShowDialog() == true)
            {
                _playlists.Add(new PlaylistItem(addWindow._title, addWindow._path));
            }
        }

        private void SongsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongsDataGrid.SelectedItem is SongItem song)
            {
                UpdateCurrentSongBar(song);
            }
        }

        private void UpdateCurrentSongBar(SongItem song)
        {
            CurrentSongTitleText.Text = song.Title;
            CurrentSongArtistText.Text = song.Artist;
            CurrentTimeText.Text = "0:00";
            TotalTimeText.Text = $"{song.Duration:m\\:ss}";

            CurrentSongCover.Source = string.IsNullOrWhiteSpace(song.CoverPath)
                ? null
                : new BitmapImage(new Uri(song.CoverPath, UriKind.RelativeOrAbsolute));
        }

        private sealed record PlaylistItem(string Name, string CoverPath);
        private sealed record SongItem(int Id, string Title, string Artist, string Album, string Genre, TimeSpan Duration, string CoverPath);
    }
}