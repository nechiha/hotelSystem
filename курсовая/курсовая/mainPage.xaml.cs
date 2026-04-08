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
    /// Логика взаимодействия для mainPage.xaml
    /// </summary>
    public partial class mainPage : Page
    {
        public mainPage()
        {
            InitializeComponent();
            RoomsListBox.ItemsSource = new[] { 1, 2, 3, 4, 5 }; // заглушка
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var roomReviewWindow = new roomReviewWindow();
            roomReviewWindow.Show();
        }
    }
}
