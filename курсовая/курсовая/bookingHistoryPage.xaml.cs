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
                    roomNumber = item.roomNumber,        // 🔥 маленькая r
                    roomImage = item.roomImage,          // 🔥 маленькая r
                    typeName = item.typeName,            // 🔥 маленькая t
                    guestName = $"{item.clientSurname} {item.clientName} {item.clientPatronomyc}".Trim(),
                    arrivingDate = item.arrivingDate,
                    departureDate = item.departureDate,
                    bookingStatusName = item.bookingStatusName,
                    bookingStatusId = item.bookingStatusId,
                    roomId = item.roomId,
                    bookingId = item.bookingId,
                    daysSpent = (item.departureDate - item.arrivingDate).Days,
                    extraOptions = string.Join(", ",
                                                from bo in context.bookingOptions
                                                join o in context.options on bo.optionId equals o.optionId
                                                where bo.bookingId == item.bookingId
                                                select o.optionName)
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
                bool matches = booking.roomNumber.ToString().Contains(searchText) ||
                    booking.guestName.ToLower().Contains(searchText);
                if (!matches) return false;
            }

            // Фильтры
            if (filterTag == "Status_Current" && (booking.bookingStatusId != 1 || booking.departureDate.Date < DateTime.Today))
                return false;
            if (filterTag == "Status_Past" && booking.departureDate.Date >= DateTime.Today)
                return false;
            if (filterTag == "Status_Canceled" && booking.bookingStatusId != 2)
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