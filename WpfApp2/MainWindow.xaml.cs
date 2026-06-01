using System.Text;
using System.Windows;
using WpfApp2.Services;
using WpfApp2.ViewModels;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
            : this(null)
        {
        }

        public MainWindow(string? initialLibraryPath = null)
        {
            InitializeComponent();
        DataContext = new MainWindowViewModel(
            new PlaylistDialogService(),
            new JsonLibraryService(),
            new SongImportService(),
            initialLibraryPath);
        }
    }
}