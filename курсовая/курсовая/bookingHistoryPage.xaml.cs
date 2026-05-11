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

namespace курсовая
{
    public partial class bookingHistoryPage : Page
    {
        public bookingHistoryPage()
        {
            InitializeComponent();
            RoomsListBox.ItemsSource = new[] { 1, 2, 3, 4, 5 };
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is var selectedRoom)
            {
                // 🔥 Исправлено: переменная называется "window", а не "roomReviewWindow"
                var window = new roomReviewWindow(
                    selectedRoom,
                    roomReviewWindow.InfoCardMode.RoomOverview);
                window.ShowDialog();
            }
        }
    }
}