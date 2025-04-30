using DoubleFileCheckerWPF.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DoubleFileCheckerWPF
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<FileGroupViewModel> GroupedImages { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            myDataGrid.LoadingRow += MyDataGrid_LoadingRow;
            myDataGrid.UnloadingRow += MyDataGrid_UnloadingRow;
        }
        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(myDataGrid);
            if (scrollViewer != null)
            {
                int delta = 0;
                if (e.Delta > 0)
                {
                    delta = 1;
                }
                else if (e.Delta < 0)
                {
                    delta = -1;
                }
                double newOffset = scrollViewer.VerticalOffset - (delta);
                newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ExtentHeight));
                scrollViewer.ScrollToVerticalOffset(newOffset);
                e.Handled = true;
            }
        }

        private void DataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            // Setze den Cursor nur bei Bedarf zurück
            if (Mouse.OverrideCursor != Cursors.Arrow)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        // Hilfsmethode zur Suche des ScrollViewers
        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T result)
                    return result;
                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }
        private void MyDataGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (e.Row.DataContext is FileGroupViewModel vm)
                vm.LoadImages();
        }

        private void MyDataGrid_UnloadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (e.Row.DataContext is FileGroupViewModel vm)
                vm.UnloadImages();
        }
        private async void OpenFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var files = await Task.Run(() => ProcessDirectory(dialog.FolderName, RecursiveEnabled));
                    var grouped = files.GroupBy(f => f.SHA256)
                                      .Where(g => g.Count() > (DuplicatesEnabled ? 1 : 0))
                                      .Select(g => new FileGroupViewModel(g.Key, [.. g.Select(f => f.FullPath)]))
                                      .ToList();

                    GroupedImages.Clear();
                    grouped.ForEach(GroupedImages.Add);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler: {ex.Message}");
                }
            }
        }

        private List<FileItem> ProcessDirectory(string folder, bool recursive = false)
        {
            var files = new List<FileItem>();

            List<string> filterextensions = [];
            if (NurBilderEnabled)
            {
                filterextensions.AddRange([".jpg", ".bmp", ".png"]);
            }

            foreach (var file in Directory.EnumerateFiles(folder, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                string fileext = Path.GetExtension(file);
                
                if (filterextensions.Count > 0)
                {
                    if (!filterextensions.Contains(fileext))
                    {
                        continue;
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    ActualFile.Text = file;
                }, DispatcherPriority.Background);

                using var stream = File.OpenRead(file);
                var hash = BitConverter.ToString(SHA256.HashData(stream)).Replace("-", "");


                files.Add(new FileItem
                {
                    FileName = Path.GetFileName(file),
                    FullPath = file,
                    SHA256 = hash,
                    Size = stream.Length
                });
            }
            return files;
        }
        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var toDelete = GroupedImages
                .SelectMany(g => g._selectionStates)
                .Where(state => state.Value == true)
                .ToList();

            if (!toDelete.Any())
            {
                MessageBox.Show("Keine Bilder ausgewählt");
                return;
            }

            var result = MessageBox.Show(
                $"{toDelete.Count} Bilder wirklich löschen?",
                "Löschen bestätigen",
                MessageBoxButton.OKCancel);

            List<FileGroupViewModel> RemoveGroups = [];
            if (result == MessageBoxResult.OK)
            {
                foreach (var kvp in toDelete)
                {
                    try
                    {
                        File.Delete(kvp.Key);
                        // Aus allen Gruppen entfernen
                        foreach (var group in GroupedImages)
                        {
                            ImageViewModel? img = group.Images.Where(x => x.Path == kvp.Key).FirstOrDefault();
                            if (img != null)
                            {
                                group.Images.Remove(img);
                            }
                            group._paths.Remove(kvp.Key);

                            if (group._paths.Count == 0)
                            {
                                RemoveGroups.Add(group);
                            }
                        }

                        //textBox.AppendText($"Gelöscht: {img.Path}\n");
                    }
                    catch (Exception ex)
                    {
                        //textBox.AppendText($"Fehler: {ex.Message}\n");
                    }
                }
                foreach (var removed in RemoveGroups)
                {
                    GroupedImages.Remove(removed);
                }
            }
        }

        public bool RecursiveEnabled { get; set; }
        public bool DuplicatesEnabled { get; set; }
        public bool NurBilderEnabled { get; set; }

        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}