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
    /// <summary>
    /// Логика взаимодействия для roomsManagementPage.xaml
    /// </summary>
    public partial class roomsManagementPage : Page
    {
        public roomsManagementPage()
        {
            InitializeComponent();
        }

        private void searchManagementRoomsTB_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void addRoomBtn_Click(object sender, RoutedEventArgs e)
        {
            var addRoomWindow = new addRoomWindow();
            addRoomWindow.Show();
        }
    }
}
