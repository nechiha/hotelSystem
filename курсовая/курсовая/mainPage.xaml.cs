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
    /// <summary>
    /// Логика взаимодействия для mainPage.xaml
    /// </summary>
    public partial class mainPage : Page
        
    {
        hotelSystemEntities1 hotelSystemEntities1 = new hotelSystemEntities1();
        public mainPage()
        {
            InitializeComponent();
            LoadAvailableRooms();
        }

        private void LoadAvailableRooms()
        {
            using (var context = new hotelSystemEntities())
            {
                // Загружаем свободные номера + цена из последнего тарифа
                var rooms = (from r in context.rooms
                             join rt in context.roomTypes on r.typeId equals rt.typeId
                             join rs in context.roomStatuses on r.roomStatusId equals rs.roomStatusId
                             // Левый джойн с тарифами (берём первый попавшийся тариф для типа)
                             join rateRoom in context.rateForRoomTypes on r.typeId equals rateRoom.typeId into rateGroup
                             from rateRoom in rateGroup.DefaultIfEmpty()
                             where r.roomStatusId == 1 // 🔥 Только свободные!
                             select new
                             {
                                 r.roomId,
                                 r.roomNumber,
                                 r.roomImage,
                                 r.roomDescription,
                                 rt.typeName,
                                 price = rateRoom.price ?? 0, // Если нет тарифа — 0
                                 rateName = rateRoom.rates.rateName ?? "Стандартный тариф"
                             }).ToList();

                RoomsListBox.ItemsSource = rooms;
            }
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is var selectedRoom)
            {
                var roomReviewWindow = new roomReviewWindow(selectedRoom);
                roomReviewWindow.ShowDialog();
            }
        }
    }
}