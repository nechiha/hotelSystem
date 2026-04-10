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
    /// Логика взаимодействия для ratePage.xaml
    /// </summary>
    public partial class ratePage : Page
    {
        public ratePage()
        {
            InitializeComponent();
            RatesListBox.ItemsSource = new[] { 1, 2, 3, 4, 5 }; // заглушка

        }

        private void createNewRateBtn_Click(object sender, RoutedEventArgs e)
        {
            var createRateWindow = new createRateWindow();
            createRateWindow.ShowDialog();
        }

        private void editRateBtn_Click(object sender, RoutedEventArgs e)
        {
            var editRateWindow = new editRateWindow();
            editRateWindow.ShowDialog();
        }

        private void currentRateBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {   
            var editRateWindow = new editRateWindow();    
            editRateWindow.ShowDialog();
        }
    }
}
