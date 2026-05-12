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
using System.Windows.Navigation;
using System.Windows.Shapes;
using курсовая.Properties;

namespace курсовая
{
    public partial class BookingHistoryCard : UserControl
    {
        private dynamic _bookingData;

        public BookingHistoryCard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Устанавливает данные бронирования для отображения
        /// </summary>
        public void SetBookingData(dynamic bookingData)
        {
            _bookingData = bookingData;
            this.DataContext = bookingData; // 🔥 Это активирует все {Binding ...} в XAML

            // Показываем/скрываем кнопку удаления
            // Показываем только если: статус = подтверждено И дата выезда ещё не прошла
            bool canCancel = _bookingData.BookingStatusId == 1 &&
                           _bookingData.DepartureDate >= DateTime.Today;

            deleteBookingBtn.Visibility = canCancel ? Visibility.Visible : Visibility.Collapsed;
        }

        private void deleteBookingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_bookingData == null) return;

            var result = MessageBox.Show(
                $"Отменить бронирование номера {_bookingData.RoomNumber}?\n\nГость: {_bookingData.GuestName}",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Логика отмены в БД
                // using (var context = new hotelSystemEntities1())
                // {
                //     var booking = context.bookings.Find(_bookingData.BookingId);
                //     if (booking != null)
                //     {
                //         booking.bookingStatusId = 2; // Отменено
                //         context.SaveChanges();
                //     }
                // }

                MessageBox.Show("Бронирование отменено", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем список или закрываем окно
                Window.GetWindow(this)?.Close();
            }
        }
    }
}
