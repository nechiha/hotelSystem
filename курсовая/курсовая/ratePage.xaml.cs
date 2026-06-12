using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для ratePage.xaml
    /// </summary>
    public partial class ratePage : Page
    {
        // Модель строки тарифа для списка
        public class RateItem : INotifyPropertyChanged
        {
            public int rateId { get; set; }
            public string rateName { get; set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        OnPropertyChanged(nameof(IsSelected));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<RateItem> _rates;
        private bool _isLoading = false; // чтобы не писать в БД при первичной загрузке

        public ratePage()
        {
            InitializeComponent();
            LoadRates();
        }

        private void LoadRates()
        {
            _isLoading = true;

            using (var context = new hotelSystemEntities1())
            {
                var list = context.rates
                    .OrderBy(r => r.rateName)
                    .Select(r => new { r.rateId, r.rateName, r.isActive })
                    .ToList();

                _rates = new ObservableCollection<RateItem>(
                    list.Select(r => new RateItem
                    {
                        rateId = r.rateId,
                        rateName = r.rateName,
                        IsSelected = r.isActive
                    }));
            }

            // Подписываемся на изменение выбора у каждого элемента
            foreach (var item in _rates)
                item.PropertyChanged += RateItem_PropertyChanged;

            RatesListBox.ItemsSource = _rates;
            _isLoading = false;
        }

        // Срабатывает, когда пользователь выбирает радиокнопку
        private void RateItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isLoading) return;
            if (e.PropertyName != nameof(RateItem.IsSelected)) return;

            var item = sender as RateItem;
            if (item == null) return;

            if (item.IsSelected)
            {
                // выбрали новый активный — снимаем выбор с остальных в коллекции
                SetActiveRate(item.rateId);
            }
            else
            {
                // 🔥 Запрещаем оставить систему без активного тарифа:
                // если сняли выбор и активных больше нет — возвращаем обратно
                if (!_rates.Any(r => r.IsSelected))
                {
                    _isLoading = true;
                    item.IsSelected = true; // откатываем
                    _isLoading = false;
                }
            }
        }

        // Делает активным один тариф, остальные — неактивными
        private void SetActiveRate(int rateId)
        {
            using (var context = new hotelSystemEntities1())
            {
                try
                {
                    foreach (var rate in context.rates)
                        rate.isActive = (rate.rateId == rateId);

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при смене активного тарифа: " + ex.Message,
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Клик по названию тарифа тоже выбирает его активным
        private void currentRateBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is RateItem item)
            {
                item.IsSelected = true; // снимет выбор с остальных через GroupName радиокнопки
            }
        }

        // Кнопка "Создать новый"
        private void createNewRateBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new createRateWindow();
            window.ShowDialog();
            LoadRates(); // обновляем список после закрытия
        }

        // Кнопка "Изменить" у строки тарифа
        private void editRateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is RateItem item)
            {
                var window = new editRateWindow(item.rateId);
                window.ShowDialog();
                LoadRates(); // обновляем список после закрытия
            }
        }
    }
}
