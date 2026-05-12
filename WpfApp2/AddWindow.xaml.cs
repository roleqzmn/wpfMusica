using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp2
{
    public partial class AddWindow : Window
    {
        string _title;
        string _path;
        public AddWindow()
        {
            _title = String.Empty;
            _path = String.Empty;
            InitializeComponent();
        }
        private void BrowseAndAdd(object sender,  RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.OpenFiles();

        }
        private void AddPlaylist(object sender, RoutedEventArgs e) { Close(); }
        private void Cancel(object sender, RoutedEventArgs e) { Close(); }
    }
}
