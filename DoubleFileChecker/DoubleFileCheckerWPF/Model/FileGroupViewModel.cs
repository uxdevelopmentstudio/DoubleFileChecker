using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DoubleFileCheckerWPF.Model
{
    public class FileGroupViewModel : INotifyPropertyChanged
    {
        public string GroupName { get; }
        public ObservableCollection<ImageViewModel> Images { get; } = new();
        public readonly List<string> _paths;
        private bool _isLoaded;

        public readonly Dictionary<string, bool> _selectionStates = new();

        public FileGroupViewModel(string name, List<string> paths)
        {
            GroupName = name;
            _paths = [.. paths.OrderBy(f => f.Length)];

            // Initiale Selektion für Gruppen >1
            if (_paths.Count > 1)
            {
                foreach (var path in _paths[1..])
                {
                    _selectionStates[path] = true;
                }
            }
        }
        public async void LoadImages()
        {
            if (_isLoaded) return;
            _isLoaded = true;

            await Task.Run(() =>
            {
                foreach (var path in _paths)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var vm = new ImageViewModel(path)
                        {
                            // Wiederherstellung des gespeicherten Zustands
                            IsSelected = _selectionStates.TryGetValue(path, out var state)
                                        ? state
                                        : false
                        };

                        vm.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(ImageViewModel.IsSelected))
                            {
                                _selectionStates[path] = ((ImageViewModel)s!).IsSelected;
                            }
                        };

                        Images.Add(vm);
                    });
                }
            });
        }

        public void UnloadImages()
        {
            if (!_isLoaded) return;
            _isLoaded = false;

            foreach (var img in Images)
                img.Unload();

            Images.Clear();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
