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

        public void SetBookingData(dynamic bookingData)
        {
            _bookingData = bookingData;
            this.DataContext = bookingData;

            // 🔥 УБРАЛ .bookings — обращайся напрямую!
            bool canCancel = _bookingData.bookingStatusId == 1 &&
                           _bookingData.departureDate >= DateTime.Today;

            deleteBookingBtn.Visibility = canCancel ? Visibility.Visible : Visibility.Collapsed;
        }

        private void deleteBookingBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Логика отмены
            MessageBox.Show("Бронирование отменено (теоритически)");
            Window.GetWindow(this).Close();
        }
    }
}