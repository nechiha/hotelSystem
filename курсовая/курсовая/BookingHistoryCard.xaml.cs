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
using MessageBox = HandyControl.Controls.MessageBox;

namespace курсовая
{
    public partial class BookingHistoryCard : UserControl
    {
        private dynamic _bookingData;

        public BookingHistoryCard()
        {
            InitializeComponent();
        }

        public void SetBookingData(dynamic bookingData)
        {
            _bookingData = bookingData;
            this.DataContext = bookingData;

            // Кнопка отмены видна, только если бронь можно отменить
            bool canCancel = _bookingData.canCancel;
            deleteBookingBtn.Visibility = canCancel ? Visibility.Visible : Visibility.Collapsed;
        }

        private void deleteBookingBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите отменить бронирование?",
                "Подтверждение отмены",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            int bookingId = _bookingData.bookingId;

            using (var context = new hotelSystemEntities1())
            {
                try
                {
                    var booking = context.bookings.Find(bookingId);
                    if (booking != null)
                    {
                        booking.bookingStatusId = 2;
                        context.SaveChanges();
                    }

                    MessageBox.Show("Бронирование отменено", "Готово",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    Window.GetWindow(this)?.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при отмене: " + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}