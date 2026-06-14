using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace курсовая
{
    public class RoomImageConverter : IValueConverter
    {
        private const string FallbackImage = "Image/placeholder.jpg";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = value?.ToString();

            if (string.IsNullOrWhiteSpace(imagePath))
                imagePath = FallbackImage;

            try
            {
                return LoadImage(imagePath);
            }
            catch
            {
                try
                {
                    return LoadImage(FallbackImage);
                }
                catch
                {
                    return null;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private BitmapImage LoadImage(string path)
        {
            Uri uri;

            if (Uri.TryCreate(path, UriKind.Absolute, out Uri parsedUri) && parsedUri.IsFile)
            {
                uri = parsedUri;
            }
            else if (Path.IsPathRooted(path) && File.Exists(path))
            {
                uri = new Uri(path, UriKind.Absolute);
            }
            else
            {
                string relativePath = path.Replace("/", "\\");
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

                if (File.Exists(fullPath))
                {
                    uri = new Uri(fullPath, UriKind.Absolute);
                }
                else
                {
                    string packPath = path.StartsWith("/")
                        ? $"pack://application:,,,{path}"
                        : $"pack://application:,,,/{path}";

                    uri = new Uri(packPath, UriKind.Absolute);
                }
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = uri;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
    }
}