using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DoubleFileCheckerWPF.Model
{
    public class ImageViewModel(string path) : INotifyPropertyChanged
    {
        public string Path { get; } = path;
        public string FileName => System.IO.Path.GetFileName(Path);
        public string FileSize => new FileInfo(Path).Length.ToString("N0") + " bytes";

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        private BitmapImage? _image;
        private bool _isLoading;

        public BitmapImage Image
        {
            get => _image ??= LoadImage();
            private set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private BitmapImage LoadImage()
        {
            IsLoading = true;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(Path);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.DecodePixelWidth = 400;
                bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void Unload()
        {
            _image = null;
            GC.SuppressFinalize(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
