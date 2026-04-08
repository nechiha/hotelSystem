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
using System.Windows.Shapes;
using HandyControl.Tools;
using MessageBox = HandyControl.Controls.MessageBox;

namespace курсовая
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class generalWindow : Window
    {
        public generalWindow()
        {
            InitializeComponent();
        }

        private void mainBtn_Click(object sender, RoutedEventArgs e)
        {
            generalFrame.Navigate(new mainPage());
        }

        private void workloadBtn_Click(object sender, RoutedEventArgs e)
        {
            generalFrame.Navigate(new workloadPage());
        }

        private void bookingHistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            generalFrame.Navigate(new bookingHistoryPage());
        }

        private void roomsManagementBtn_Click(object sender, RoutedEventArgs e)
        {
            generalFrame.Navigate(new roomsManagementPage());
        }

        private void rateBtn_Click(object sender, RoutedEventArgs e)
        {
            generalFrame.Navigate(new ratePage());
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите выйти?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question );
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}
