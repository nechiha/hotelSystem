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
    public partial class roomReviewWindow : Window
    {
        private readonly dynamic _roomData;
        private readonly InfoCardMode _currentMode;

        public enum InfoCardMode
        {
            RoomOverview,
            BookingHistory
        }

        // 🔥 ОДИН конструктор на все случаи
        public roomReviewWindow(dynamic roomData, InfoCardMode mode = InfoCardMode.RoomOverview)
        {
            InitializeComponent();
            _roomData = roomData;
            _currentMode = mode;
            LoadRoomData();
            LoadUserControl();
        }

        private void LoadRoomData()
        {
            this.Title = _currentMode == InfoCardMode.RoomOverview
                ? $"Номер {_roomData.roomNumber}"      // 🔥 маленькая r
                : $"Бронирование: номер {_roomData.roomNumber}";

            roomNumberTB.Text = $"№ {_roomData.roomNumber}";  // 🔥 маленькая r
            roomTypeText.Text = _roomData.typeName?.ToString() ?? "Тип не указан";  // 🔥 маленькая t

            LoadRoomImage();
            toBookBtn.Visibility = _currentMode == InfoCardMode.RoomOverview
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void LoadRoomImage()
        {
            if (string.IsNullOrEmpty(_roomData.roomImage))  // 🔥 маленькая r
            {
                roomImage.Source = new BitmapImage(new Uri("/Icon/placeholder.png", UriKind.Relative));
                return;
            }
            try
            {
                string path = _roomData.roomImage;  // 🔥 маленькая r
                if (!path.StartsWith("/") && !path.StartsWith("pack://"))
                    path = "/" + path;
                roomImage.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                roomImage.Source = new BitmapImage(new Uri("/Icon/placeholder.png", UriKind.Relative));
            }
        }

        private void LoadUserControl()
        {
            if (_currentMode == InfoCardMode.RoomOverview)
            {
                var roomCard = new RoomReviewCard();
                roomCard.SetRoomData(_roomData);
                DynamicContentControl.Content = roomCard;
            }
            else // BookingHistory
            {
                var historyCard = new BookingHistoryCard();
                historyCard.SetBookingData(_roomData);  // 🔥 Передаём тот же _roomData
                DynamicContentControl.Content = historyCard;
            }
        }

        private void escapeFromReviewBtn_Click(object sender, RoutedEventArgs e) => this.Close();
        private void toBookBtn_Click(object sender, RoutedEventArgs e)
        {
            //  Берём данные из _roomData, который уже есть в этом окне
            int roomId = _roomData.roomId;      // или RoomId, если у тебя с большой буквы
            double price = _roomData.price;     // или Price, если с большой

            var bookingWindow = new bookingWindow
            {
                SelectedRoomId = roomId,
                RoomPrice = price
            };
            bookingWindow.ShowDialog();
        }
    }
}