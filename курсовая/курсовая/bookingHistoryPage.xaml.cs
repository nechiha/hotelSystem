using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using курсовая.Properties;

namespace курсовая
{
    public partial class bookingHistoryPage : Page
    {
        private ICollectionView _bookingsView;

        public bookingHistoryPage()
        {
            InitializeComponent();
            LoadBookingHistory();
        }

        private void LoadBookingHistory()
        {
            using (var context = new hotelSystemEntities1())
            {
                // 🔥 ШАГ 1: Загружаем данные из БД (без форматирования!)
                var rawData = context.bookingRooms
                    .Include(br => br.bookings)
                    .Include(br => br.bookings.clients)
                    .Include(br => br.bookings.bookingStatuses)
                    .Include(br => br.rooms)
                    .Include(br => br.rooms.roomTypes)
                    .Select(br => new
                    {
                        br.bookingId,
                        br.rooms.roomId,
                        br.rooms.roomNumber,
                        br.rooms.roomImage,
                        br.rooms.roomTypes.typeName,
                        br.bookings.clients.clientSurname,
                        br.bookings.clients.clientName,
                        br.bookings.clients.clientPatronomyc,
                        br.arrivingDate,
                        br.departureDate,
                        br.bookings.bookingStatusId,
                        br.bookings.bookingStatuses.bookingStatusName
                    })
                    .ToList(); // 🔥 Выполняем запрос в БД

                // 🔥 ШАГ 2: Формируем итоговый список с ИМЕНАМИ свойств как в твоем XAML
                var bookings = rawData.Select(item => new
                {
                    RoomNumber = item.roomNumber,
                    RoomImage = item.roomImage,
                    // 🔥 GuestName — именно так ждёт твой XAML
                    GuestName = $"{item.clientSurname} {item.clientName} {item.clientPatronomyc}".Trim(),
                    // 🔥 ArrivingDate / DepartureDate — именно так в привязках
                    ArrivingDate = item.arrivingDate,
                    DepartureDate = item.departureDate,
                    BookingStatusName = item.bookingStatusName,
                    BookingStatusId = item.bookingStatusId,
                    // Дополнительно (если понадобится)
                    RoomId = item.roomId,
                    BookingId = item.bookingId
                }).ToList();

                _bookingsView = CollectionViewSource.GetDefaultView(bookings);
                _bookingsView.Filter = FilterBookings;
                RoomsListBox.ItemsSource = _bookingsView;
            }
        }

        private bool FilterBookings(object item)
        {
            dynamic booking = item;

            string searchText = (searchBookingTB?.Text ?? "").Trim().ToLower();
            string filterTag = (filtersCB?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            // Поиск по номеру или ФИО
            if (!string.IsNullOrEmpty(searchText))
            {
                bool matches = booking.RoomNumber.ToString().Contains(searchText) ||
                    booking.GuestName.ToLower().Contains(searchText);
                if (!matches) return false;
            }

            // Фильтры
            if (filterTag == "Status_Current" && (booking.BookingStatusId != 1 || booking.DepartureDate.Date < DateTime.Today))
                return false;
            if (filterTag == "Status_Past" && booking.DepartureDate.Date >= DateTime.Today)
                return false;
            if (filterTag == "Status_Canceled" && booking.BookingStatusId != 2)
                return false;

            return true;
        }

        private void searchBookingTB_TextChanged(object sender, TextChangedEventArgs e) => _bookingsView?.Refresh();
        private void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => _bookingsView?.Refresh();

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            searchBookingTB.Text = "";
            filtersCB.SelectedIndex = 0;
            _bookingsView?.Refresh();
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
            {
                dynamic booking = element.DataContext;
                var window = new roomReviewWindow(booking, roomReviewWindow.InfoCardMode.BookingHistory);
                window.ShowDialog();
            }
        }
    }
}