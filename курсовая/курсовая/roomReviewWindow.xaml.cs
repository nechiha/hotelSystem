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
        private InfoCardMode _currentMode;
        private int _roomId;
        private int? _bookingId; // nullable, потому что в первом режиме его нет

        public enum InfoCardMode
        {
            RoomOverview,
            BookingHistory
        }

        public roomReviewWindow(/*InfoCardMode mode, int roomId, int? bookingId = null*/)
        {
            InitializeComponent();

            //_currentMode = mode;
            //_roomId = roomId;
            //_bookingId = bookingId;

            // Настраиваем интерфейс в зависимости от режима
            ConfigureUIForMode();

            // Загружаем нужный UserControl
            LoadUserControl();

            // TODO: В будущем загрузите данные из БД
            // LoadDataFromDatabase();
        }

        /// <summary>
        /// Настройка интерфейса в зависимости от режима
        /// </summary>
        private void ConfigureUIForMode()
        {
            switch (_currentMode)
            {
                case InfoCardMode.RoomOverview:
                    // Показываем кнопку "Забронировать"
                    toBookBtn.Visibility = Visibility.Visible;
                    break;

                case InfoCardMode.BookingHistory:
                    // Скрываем кнопку "Забронировать"
                    toBookBtn.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Загрузка нужного UserControl в зависимости от режима
        /// </summary>
        private void LoadUserControl()
        {
            switch (_currentMode)
            {
                case InfoCardMode.RoomOverview:
                    var roomCard = new RoomReviewCard();
                    // TODO: передать данные комнаты
                    // roomCard.SetRoomData(roomData);
                    DynamicContentControl.Content = roomCard;
                    break;

                case InfoCardMode.BookingHistory:
                    var bookingCard = new BookingHistoryCard();
                    // TODO: передать данные бронирования
                    // bookingCard.SetBookingData(roomData, bookingData);
                    DynamicContentControl.Content = bookingCard;
                    break;
            }
        }

        /// <summary>
        /// Загрузка данных из БД (будет реализовано позже)
        /// </summary>
        private void LoadDataFromDatabase()
        {
            // Здесь будет загрузка из MS SQL через ADO.NET
            // switch (_currentMode)
            // {
            //     case InfoCardMode.RoomOverview:
            //         var room = DatabaseHelper.GetRoomById(_roomId);
            //         RoomNumberText.Text = room.RoomNumber;
            //         RoomTypeText.Text = room.RoomType;
            //         ((RoomReviewCard)DynamicContentControl.Content).SetRoomData(room);
            //         break;
            //         
            //     case InfoCardMode.BookingHistory:
            //         var room = DatabaseHelper.GetRoomById(_roomId);
            //         var booking = DatabaseHelper.GetBookingById(_bookingId.Value);
            //         RoomNumberText.Text = room.RoomNumber;
            //         RoomTypeText.Text = room.RoomType;
            //         ((BookingHistoryCard)DynamicContentControl.Content).SetBookingData(room, booking);
            //         break;
            // }
        }

        // Кнопка "Назад"
        private void escapeFromReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Кнопка "Забронировать" (только для режима RoomOverview)
        private void toBookBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Здесь будет открытие окна бронирования
            MessageBox.Show("Открыть окно бронирования", "Бронирование",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
