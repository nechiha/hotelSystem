using LiveCharts;
using LiveCharts.Wpf;
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
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class workloadPage : Page
    {
        public workloadPage()
        {
            InitializeComponent();

            periodStartPicker.SelectedDate = DateTime.Today;
            periodEndPicker.SelectedDate = DateTime.Today.AddDays(6);

            LoadWorkload();
        }

        private void Period_DateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadWorkload();
        }

        private void LoadWorkload()
        {
            if (periodStartPicker?.SelectedDate == null || periodEndPicker?.SelectedDate == null)
                return;

            DateTime periodStart = periodStartPicker.SelectedDate.Value.Date;
            DateTime periodEnd = periodEndPicker.SelectedDate.Value.Date;

            if (periodEnd < periodStart)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала",
                    "Некорректный период", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime periodEndInclusive = periodEnd.AddDays(1).AddTicks(-1);

            using (var context = new hotelSystemEntities1())
            {
                var busy = (from br in context.bookingRooms
                            join r in context.rooms on br.roomId equals r.roomId
                            where br.bookings.bookingStatusId == 1
                                  && br.arrivingDate <= periodEndInclusive
                                  && br.departureDate >= periodStart
                            select new { br.roomId, r.typeId })
                            .Distinct()
                            .ToList();

                int regular = busy.Count(x => x.typeId <= 4);
                int vip = busy.Count(x => x.typeId >= 5);
                int totalBusy = regular + vip;

                int totalRooms = context.rooms.Count();
                int free = totalRooms - totalBusy;
                if (free < 0) free = 0;

                // Плитки
                regularValue.Text = regular.ToString();
                vipValue.Text = vip.ToString();
                totalBusyValue.Text = totalBusy.ToString();
                totalRoomsValue.Text = totalRooms.ToString();

                // Цветовая индикация загрузки (по проценту занятости)
                double load = totalRooms > 0 ? (double)totalBusy / totalRooms : 0;
                if (load < 0.5)
                    totalBusyValue.Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // зелёный
                else if (load < 0.8)
                    totalBusyValue.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)); // оранжевый
                else
                    totalBusyValue.Foreground = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)); // красный

                UpdateChart(totalBusy, free);
            }
        }

        private void UpdateChart(int busy, int free)
        {
            workloadPieChart.Series = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Занято",
                    Values = new ChartValues<int> { busy },
                    DataLabels = true,
                    Fill = new SolidColorBrush(Color.FromRgb(0xE4, 0x90, 0xA5)),
                    LabelPoint = p => $"{busy}"
                },
                new PieSeries
                {
                    Title = "Свободно",
                    Values = new ChartValues<int> { free },
                    DataLabels = true,
                    Fill = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                    LabelPoint = p => $"{free}"
                }
            };
        }
    }
}
