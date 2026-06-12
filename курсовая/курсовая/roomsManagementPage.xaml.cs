using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace курсовая
{
    /// <summary>
    /// Логика взаимодействия для roomsManagementPage.xaml
    /// </summary>
    public partial class roomsManagementPage : Page
    {
        private ICollectionView _roomsView;

        public roomsManagementPage()
        {
            InitializeComponent();
            LoadFilters();
            LoadRooms();
        }

        private void LoadFilters()
        {
            managmentFilterCB.Items.Clear();
            managmentFilterCB.Items.Add("Все номера");
            managmentFilterCB.Items.Add("Только обычные");
            managmentFilterCB.Items.Add("Только VIP");
            managmentFilterCB.Items.Add("Свободные");
            managmentFilterCB.Items.Add("Занятые");
            managmentFilterCB.SelectedIndex = 0;
            managmentFilterCB.SelectionChanged += (s, e) => _roomsView?.Refresh();
        }

        private void LoadRooms()
        {
            using (var context = new hotelSystemEntities1())
            {
                var rooms = (from r in context.rooms
                             join rt in context.roomTypes on r.typeId equals rt.typeId
                             join rs in context.roomStatuses on r.roomStatusId equals rs.roomStatusId
                             select new
                             {
                                 r.roomNumber,
                                 r.roomFloor,
                                 r.typeId,
                                 typeName = rt.typeName,
                                 statusName = rs.roomStatusName,
                                 r.roomStatusId
                             })
                             .OrderBy(r => r.roomNumber)
                             .ToList()
                             // формируем поля для отображения
                             .Select(r => new
                             {
                                 r.roomNumber,
                                 r.roomFloor,
                                 capacity = r.typeId > 4 ? r.typeId - 4 : r.typeId,
                                 category = r.typeId > 4 ? "VIP" : "Обычный",
                                 r.roomStatusId
                             })
                             .ToList();

                _roomsView = CollectionViewSource.GetDefaultView(rooms);
                _roomsView.Filter = FilterRooms;
                workloadDataGrid.ItemsSource = _roomsView;
            }
        }

        private bool FilterRooms(object item)
        {
            dynamic room = item;

            // Поиск по номеру комнаты
            string search = searchManagementRoomsTB.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                if (!room.roomNumber.ToString().Contains(search))
                    return false;
            }

            // Фильтр
            string filter = managmentFilterCB.SelectedItem?.ToString() ?? "Все номера";
            switch (filter)
            {
                case "Только обычные": return room.category == "Обычный";
                case "Только VIP": return room.category == "VIP";
                case "Свободные": return room.roomStatusId == 1;
                case "Занятые": return room.roomStatusId == 2;
                default: return true; // Все номера
            }
        }

        private void searchManagementRoomsTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            _roomsView?.Refresh();
        }

        private void addRoomBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new addRoomWindow();
            window.ShowDialog();
            LoadRooms(); // обновляем список после добавления
        }
    }
}
