using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace курсовая
{
    /// <summary>
    /// Логика взаимодействия для roomReviewWindow.xaml
    /// </summary>
    public partial class roomReviewWindow : Window
    {
        private readonly dynamic _roomData;
        private readonly InfoCardMode _currentMode;

        public enum InfoCardMode
        {
            RoomOverview,      // Просмотр номера (перед бронированием)
            BookingHistory     // История бронирования (будущее)
        }

        /// <summary>
        /// Основной конструктор — для просмотра номера
        /// </summary>
        public roomReviewWindow(dynamic roomData, InfoCardMode mode = InfoCardMode.RoomOverview)
        {
            InitializeComponent();

            _roomData = roomData;
            _currentMode = mode;

            LoadRoomData();
            LoadUserControl();
        }

        /// <summary>
        /// Конструктор для истории бронирования (будущий)
        /// </summary>
        public roomReviewWindow(dynamic roomData, dynamic bookingData)
        {
            InitializeComponent();

            _roomData = roomData;
            _currentMode = InfoCardMode.BookingHistory;

            LoadRoomData();
            LoadUserControl(bookingData);
        }

        /// <summary>
        /// Заполняет поля окна данными из БД
        /// </summary>
        private void LoadRoomData()
        {
            // Заголовок окна
            this.Title = _currentMode == InfoCardMode.RoomOverview
                ? $"Номер {_roomData.roomNumber}"
                : $"Бронирование: номер {_roomData.roomNumber}";

            // 🔥 Используем ТОЛЬКО те x:Name, что есть в твоем XAML:
            roomNumberTB.Text = $"№ {_roomData.roomNumber}";
            roomTypeText.Text = _roomData.typeName?.ToString() ?? "Тип не указан";

            // Загружаем изображение в roomImage (твой x:Name)
            LoadRoomImage();

            // Показываем кнопку "Забронировать" только в режиме RoomOverview
            toBookBtn.Visibility = _currentMode == InfoCardMode.RoomOverview
                ? Visibility.Visible
                : Visibility.Hidden; // 🔥 Hidden, как в твоем XAML
        }

        /// <summary>
        /// Загружает изображение с обработкой ошибок
        /// </summary>
        private void LoadRoomImage()
        {
            if (string.IsNullOrEmpty(_roomData.roomImage))
            {
                roomImage.Source = new BitmapImage(
                    new Uri("/Icon/placeholder.png", UriKind.Relative));
                return;
            }

            try
            {
                string path = _roomData.roomImage;
                if (!path.StartsWith("/") && !path.StartsWith("pack://"))
                {
                    path = "/" + path;
                }

                roomImage.Source = new BitmapImage(
                    new Uri(path, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                roomImage.Source = new BitmapImage(
                    new Uri("/Icon/placeholder.png", UriKind.Relative));
            }
        }

        /// <summary>
        /// Загружает UserControl в DynamicContentControl
        /// </summary>
        private void LoadUserControl(dynamic bookingData = null)
        {
            switch (_currentMode)
            {
                case InfoCardMode.RoomOverview:
                    var roomCard = new RoomReviewCard();
                    roomCard.SetRoomData(_roomData);
                    DynamicContentControl.Content = roomCard;
                    break;

                case InfoCardMode.BookingHistory:
                    // 🔥 Загружаем BookingHistoryCard и передаём данные
                    var historyCard = new BookingHistoryCard();
                    historyCard.SetBookingData(_roomData); // _roomData уже содержит всё: GuestName, Dates, Status и т.д.
                    DynamicContentControl.Content = historyCard;
                    break;
            }
        }

        private void escapeFromReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void toBookBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Передай roomId в bookingWindow, если нужно
            var bookingWindow = new bookingWindow();
            bookingWindow.Show();
        }
    }
}
