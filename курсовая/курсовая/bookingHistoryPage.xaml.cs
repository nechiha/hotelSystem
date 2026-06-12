using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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
                DateTime today = DateTime.Today;

                // 1. Основные данные броней (без опций)
                var rawData = context.bookingRooms
                    .Include(br => br.bookings)
                    .Include(br => br.bookings.clients)
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
                        br.bookings.clients.clientPhone,
                        br.arrivingDate,
                        br.departureDate,
                        br.bookings.bookingStatusId
                    })
                    .ToList(); // выполняем запрос -> дальше работаем в памяти

                // 2. Все опции одним запросом: bookingId -> список названий услуг
                var optionsByBooking = context.bookingOptions
                    .Select(bo => new { bo.bookingId, bo.options.optionName })
                    .ToList()
                    .GroupBy(x => x.bookingId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.optionName).ToList());

                // 3. Формируем итоговый список (в памяти)
                var bookings = rawData.Select(item =>
                {
                    string displayStatus;
                    Brush statusColor;

                    if (item.bookingStatusId == 2)
                    {
                        displayStatus = "Отменено";
                        statusColor = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35));
                    }
                    else if (item.departureDate.Date < today)
                    {
                        displayStatus = "Завершено";
                        statusColor = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));
                    }
                    else if (item.arrivingDate.Date > today)
                    {
                        displayStatus = "Предстоящее";
                        statusColor = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
                    }
                    else
                    {
                        displayStatus = "Текущее";
                        statusColor = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                    }

                    // опции для этой брони
                    string extraOptions = optionsByBooking.TryGetValue(item.bookingId, out var opts) && opts.Any()
                        ? string.Join(", ", opts)
                        : "—";

                    // Фактически проведённые дни
                    int daysSpent;
                    if (item.bookingStatusId == 2 || item.arrivingDate.Date > today)
                    {
                        // отменено или ещё не заехал
                        daysSpent = 0;
                    }
                    else if (item.departureDate.Date < today)
                    {
                        // завершено — полная длительность
                        daysSpent = (item.departureDate.Date - item.arrivingDate.Date).Days;
                    }
                    else
                    {
                        // текущее — сколько уже прошло с момента заезда
                        daysSpent = (today - item.arrivingDate.Date).Days;
                    }

                    return new
                    {
                        roomNumber = item.roomNumber,
                        roomImage = item.roomImage,
                        typeName = item.typeName,
                        guestName = $"{item.clientSurname} {item.clientName} {item.clientPatronomyc}".Trim(),
                        guestPhone = item.clientPhone,
                        arrivingDate = item.arrivingDate,
                        departureDate = item.departureDate,
                        bookingStatusId = item.bookingStatusId,
                        displayStatus = displayStatus,
                        statusColor = statusColor,
                        extraOptions = extraOptions,
                        daysSpent = daysSpent,
                        roomId = item.roomId,
                        bookingId = item.bookingId,
                        canCancel = item.bookingStatusId == 1 && item.arrivingDate.Date > today
                    };
                })
                .OrderByDescending(b => b.arrivingDate)
                .ToList();

                _bookingsView = CollectionViewSource.GetDefaultView(bookings);
                _bookingsView.Filter = FilterBookings;
                RoomsListBox.ItemsSource = _bookingsView;
            }
        }

        private bool FilterBookings(object item)
        {
            dynamic booking = item;
            DateTime today = DateTime.Today;

            string searchText = (searchBookingTB?.Text ?? "").Trim().ToLower();
            string filterTag = (filtersCB?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            // Поиск по номеру или ФИО
            if (!string.IsNullOrEmpty(searchText))
            {
                bool matches = booking.roomNumber.ToString().Contains(searchText) ||
                               ((string)booking.guestName).ToLower().Contains(searchText);
                if (!matches) return false;
            }

            // Фильтр по вычисляемому статусу
            switch (filterTag)
            {
                case "Upcoming":
                    return booking.bookingStatusId == 1 && booking.arrivingDate.Date > today;
                case "Current":
                    return booking.bookingStatusId == 1 &&
                           booking.arrivingDate.Date <= today &&
                           booking.departureDate.Date >= today;
                case "Completed":
                    return booking.bookingStatusId == 1 && booking.departureDate.Date < today;
                case "Canceled":
                    return booking.bookingStatusId == 2;
                default:
                    return true; // All
            }
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
                LoadBookingHistory(); // обновляем после возможной отмены
            }
        }
    }
}