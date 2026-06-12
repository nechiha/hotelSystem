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
            LoadPeriods();
        }

        private void LoadPeriods()
        {
            workloadPeriod.Items.Clear();
            workloadPeriod.Items.Add("Сегодня");
            workloadPeriod.Items.Add("Текущая неделя");
            workloadPeriod.Items.Add("Текущий месяц");
            workloadPeriod.SelectionChanged += (s, e) => LoadWorkload();
            workloadPeriod.SelectedIndex = 0; // вызовет LoadWorkload
        }

        private void LoadWorkload()
        {
            // 1. Определяем границы периода
            DateTime periodStart, periodEnd;
            string period = workloadPeriod.SelectedItem?.ToString() ?? "Сегодня";

            DateTime today = DateTime.Today;
            switch (period)
            {
                case "Текущая неделя":
                    // понедельник текущей недели
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    periodStart = today.AddDays(-diff);
                    periodEnd = periodStart.AddDays(6);
                    break;

                case "Текущий месяц":
                    periodStart = new DateTime(today.Year, today.Month, 1);
                    periodEnd = periodStart.AddMonths(1).AddDays(-1);
                    break;

                default: // Сегодня
                    periodStart = today;
                    periodEnd = today;
                    break;
            }

            // конец периода берём до конца дня
            DateTime periodEndInclusive = periodEnd.Date.AddDays(1).AddTicks(-1);

            using (var context = new hotelSystemEntities1())
            {
                // 2. Берём активные брони, пересекающиеся с периодом
                //    Пересечение: заезд <= конец периода И выезд >= начало периода
                var busy = (from br in context.bookingRooms
                            join r in context.rooms on br.roomId equals r.roomId
                            where br.bookings.bookingStatusId == 1
                                  && br.arrivingDate <= periodEndInclusive
                                  && br.departureDate >= periodStart
                            select new { br.roomId, r.typeId })
                            .Distinct()   // один номер считаем один раз
                            .ToList();

                // 3. Считаем обычные (typeId <= 4) и VIP (typeId >= 5)
                int regular = busy.Count(x => x.typeId <= 4);
                int vip = busy.Count(x => x.typeId >= 5);

                // 4. Выводим одной строкой в DataGrid
                workloadDataGrid.ItemsSource = new[]
                {
                    new { regularRooms = regular, vipRooms = vip }
                };
            }
        }
    }
}   
