namespace DoubleFileCheckerWPF.Model
{
    public class FileItem
    {
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string SHA256 { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
