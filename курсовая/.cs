using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace курсовая
{
    public class RoomImageConverter : IValueConverter
    {
        private const string FallbackImage = "Image/placeholder.png";

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

            // 1. Если в БД вдруг лежит полный путь, например D:\...\photo.jpg
            if (Path.IsPathRooted(path) && File.Exists(path))
            {
                uri = new Uri(path, UriKind.Absolute);
            }
            else
            {
                // 2. Если путь относительный, например RoomImages/room_101.jpg
                string relativePath = path.Replace("/", "\\");
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

                if (File.Exists(fullPath))
                {
                    uri = new Uri(fullPath, UriKind.Absolute);
                }
                else
                {
                    // 3. Если картинка лежит в проекте как ресурс, например Image/ExampleImage.jpg
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