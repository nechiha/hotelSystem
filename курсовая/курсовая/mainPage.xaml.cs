using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using курсовая.Properties;

namespace курсовая
{
    public partial class mainPage : Page
    {
        private ICollectionView _roomsView;  // ✅ Представление для фильтрации
        private List<dynamic> _allRoomsList; // Оригинальные данные

        public mainPage()
        {
            InitializeComponent();
            LoadAvailableRooms();
        }

        private void LoadAvailableRooms()
        {
            using (var context = new hotelSystemEntities1())
            {
                // 🔥 Автоматически освобождаем номера, где дата выезда прошла
                var expiredBookings = context.bookingRooms
                    .Where(br => br.departureDate < DateTime.Today &&
                                 br.bookings.bookingStatusId == 1)
                    .ToList();

                foreach (var booking in expiredBookings)
                {
                    var room = context.rooms.Find(booking.roomId);
                    if (room != null && room.roomStatusId == 2)
                    {
                        room.roomStatusId = 1; // Освобождаем
                    }
                }
                context.SaveChanges();
                var rooms = (from r in context.rooms
                             join rt in context.roomTypes on r.typeId equals rt.typeId
                             join rs in context.roomStatuses on r.roomStatusId equals rs.roomStatusId
                             join rateRoom in context.rateForRoomTypes on r.typeId equals rateRoom.typeId into rateGroup
                             from rateRoom in rateGroup.DefaultIfEmpty()
                             where r.roomStatusId == 1 ||
                           (r.roomStatusId == 2 &&
                            !context.bookingRooms.Any(br => br.roomId == r.roomId &&
                                                           br.departureDate >= DateTime.Today &&
                                                           br.bookings.bookingStatusId == 1))
                             select new
                             {
                                 r.roomId,
                                 r.roomNumber,
                                 r.roomFloor,
                                 r.roomImage,
                                 r.roomDescription,
                                 rt.typeName,
                                 rt.typeId,
                                 price = rateRoom != null ? rateRoom.price : 0,
                                 rateName = rateRoom != null && rateRoom.rates != null
                                     ? rateRoom.rates.rateName
                                     : "Стандартный тариф"
                             }).ToList();

                _allRoomsList = rooms.Cast<dynamic>().ToList();

                // Создаём представление для фильтрации
                _roomsView = CollectionViewSource.GetDefaultView(rooms);
                _roomsView.Filter = FilterRooms;

                RoomsListBox.ItemsSource = _roomsView;
            }
        }

        /// <summary>
        /// Метод фильтрации (вызывается автоматически при обновлении view)
        /// </summary>
        private bool FilterRooms(object item)
        {
            dynamic room = item;

            string searchText = mainSearchTB.Text?.Trim().ToLower() ?? "";
            string filterTag = (filtersCB.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            // 1. Поиск по тексту
            if (!string.IsNullOrEmpty(searchText))
            {
                bool matchesSearch = room.roomNumber.ToString().Contains(searchText) ||
                    (room.roomDescription?.ToLower().Contains(searchText) ?? false) ||
                    (room.typeName?.ToLower().Contains(searchText) ?? false);

                if (!matchesSearch) return false;
            }

            // 2. Фильтр по типу номера
            if (filterTag?.StartsWith("Type_") == true)
            {
                int typeId = Convert.ToInt32(filterTag.Replace("Type_", ""));
                if (room.typeId != typeId) return false;
            }

            // 3. Фильтр по этажу
            if (filterTag?.StartsWith("Floor_") == true)
            {
                int floor = Convert.ToInt32(filterTag.Replace("Floor_", ""));
                if (room.roomFloor != floor) return false;
            }

            // 4. Фильтр по цене
            if (filterTag?.StartsWith("Price_") == true)
            {
                var parts = filterTag.Replace("Price_", "").Split('_');
                double minPrice = double.Parse(parts[0]);
                double maxPrice = double.Parse(parts[1]);

                if (room.price < minPrice || room.price > maxPrice) return false;
            }

            return true;
        }

        private void mainSearchTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            _roomsView?.Refresh();  // ✅ Обновляем фильтр
        }

        private void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _roomsView?.Refresh();  // ✅ Обновляем фильтр
        }

        private void resetFiltersBtn_Click(object sender, RoutedEventArgs e)
        {
            mainSearchTB.Text = "";
            filtersCB.SelectedIndex = 0;
            _roomsView?.Refresh();  // ✅ Обновляем фильтр
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
            {
                dynamic selectedRoom = element.DataContext;

                // 🔥 ОТКРЫВАЕМ roomReviewWindow, а НЕ bookingWindow!
                var roomReviewWindow = new roomReviewWindow(selectedRoom);
                roomReviewWindow.ShowDialog();
            }
        }
    }
}