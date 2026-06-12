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
using System.Text.RegularExpressions;


namespace курсовая
{
    /// <summary>
    /// Логика взаимодействия для editRateWindow.xaml
    /// </summary>
    public partial class editRateWindow : Window
    {
        private readonly int _rateId;
        private bool _breakfastIncluded = false;
        private bool _vipBreakfastIncluded = false;

        public editRateWindow(int rateId)
        {
            InitializeComponent();
            _rateId = rateId;
            LoadRateData();
        }

        // Загрузка данных выбранного тарифа в поля
        private void LoadRateData()
        {
            using (var context = new hotelSystemEntities1())
            {
                var rate = context.rates.FirstOrDefault(r => r.rateId == _rateId);
                if (rate == null)
                {
                    MessageBox.Show("Тариф не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Название
                insertRateNameTB.Text = rate.rateName;

                // Тумблеры завтрака
                _breakfastIncluded = rate.isBreakfastIncluded;
                _vipBreakfastIncluded = rate.isVIPBreakfastIncluded;
                isBreakfastIncludedTglBtn.IsChecked = rate.isBreakfastIncluded;
                isVIPBreakfastIncludedTglBtn.IsChecked = rate.isVIPBreakfastIncluded;

                // Цены по типам номеров
                var prices = context.rateForRoomTypes
                    .Where(rfr => rfr.rateId == _rateId)
                    .ToDictionary(rfr => rfr.typeId, rfr => rfr.price);

                singleRoomPriceTB.Text = GetPriceText(prices, 1);
                doubleRoomPriceTB.Text = GetPriceText(prices, 2);
                tripleRoomPriceTb.Text = GetPriceText(prices, 3);
                quadrupleRoomPriceTB.Text = GetPriceText(prices, 4);
                singleVIPRoomPriceTB.Text = GetPriceText(prices, 5);
                doubleVIPRoomPriceTB.Text = GetPriceText(prices, 6);
                tripleVIPRoomPriceTb.Text = GetPriceText(prices, 7);
                quadrupleVIPRoomPriceTB.Text = GetPriceText(prices, 8);
            }
        }

        // Возвращает текст цены; если цена 0 или отсутствует — пустую строку
        private string GetPriceText(Dictionary<int, double> prices, int typeId)
        {
            if (prices.TryGetValue(typeId, out double price) && price > 0)
                return price.ToString();
            return "";
        }

        private void isBreakfastIncludedTglBtn_Checked(object sender, RoutedEventArgs e) => _breakfastIncluded = true;
        private void isBreakfastIncludedTglBtn_Unchecked(object sender, RoutedEventArgs e) => _breakfastIncluded = false;
        private void isVIPBreakfastIncludedTglBtn_Checked(object sender, RoutedEventArgs e) => _vipBreakfastIncluded = true;
        private void isVIPBreakfastIncludedTglBtn_Unchecked(object sender, RoutedEventArgs e) => _vipBreakfastIncluded = false;

        // Разрешаем вводить только цифры
        private void PriceTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // true, если в вводимом тексте есть хоть один не-цифровой символ -> запрещаем
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // Блокируем вставку нечислового текста (Ctrl+V)
        private void PriceTB_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                // убираем пробелы и проверяем, что остались только цифры
                string cleaned = text.Replace(" ", "").Replace("\u00A0", "");
                if (!cleaned.All(char.IsDigit))
                    e.CancelCommand(); // отменяем вставку
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void saveEditedRateBtn_Click(object sender, RoutedEventArgs e)
        {
            string rateName = insertRateNameTB.Text?.Trim();
            if (string.IsNullOrWhiteSpace(rateName))
            {
                MessageBox.Show("Введите название тарифа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new hotelSystemEntities1())
            {
                try
                {
                    var rate = context.rates.FirstOrDefault(r => r.rateId == _rateId);
                    if (rate == null)
                    {
                        MessageBox.Show("Тариф не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Типы, по которым реально есть номера
                    var existingTypeIds = context.rooms
                        .Select(r => r.typeId)
                        .Distinct()
                        .ToList();

                    var priceInputs = new List<(int typeId, string text, string name)>
                    {
                        (1, singleRoomPriceTB.Text,       "1-местный"),
                        (2, doubleRoomPriceTB.Text,       "2-местный"),
                        (3, tripleRoomPriceTb.Text,       "3-местный"),
                        (4, quadrupleRoomPriceTB.Text,    "4-местный"),
                        (5, singleVIPRoomPriceTB.Text,    "1-местный VIP"),
                        (6, doubleVIPRoomPriceTB.Text,    "2-местный VIP"),
                        (7, tripleVIPRoomPriceTb.Text,    "3-местный VIP"),
                        (8, quadrupleVIPRoomPriceTB.Text, "4-местный VIP"),
                    };

                    var pricesToSave = new Dictionary<int, double>();
                    foreach (var input in priceInputs)
                    {
                        bool roomsExist = existingTypeIds.Contains(input.typeId);
                        if (!TryGetPrice(input.text, roomsExist, input.name, out double price))
                            return;
                        pricesToSave[input.typeId] = price;
                    }

                    // Обновляем сам тариф
                    rate.rateName = rateName;
                    rate.isBreakfastIncluded = _breakfastIncluded;
                    rate.isVIPBreakfastIncluded = _vipBreakfastIncluded;

                    // Обновляем/создаём строки цен
                    var existingPrices = context.rateForRoomTypes
                        .Where(rfr => rfr.rateId == _rateId)
                        .ToList();

                    foreach (var pair in pricesToSave)
                    {
                        var row = existingPrices.FirstOrDefault(p => p.typeId == pair.Key);
                        if (row != null)
                        {
                            row.price = pair.Value; // обновляем существующую
                        }
                        else
                        {
                            context.rateForRoomTypes.Add(new rateForRoomTypes
                            {
                                typeId = pair.Key,
                                rateId = _rateId,
                                price = pair.Value
                            });
                        }
                    }

                    context.SaveChanges();

                    MessageBox.Show("Тариф успешно изменён", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Та же проверка, что и в createRateWindow
        private bool TryGetPrice(string text, bool required, string fieldName, out double price)
        {
            price = 0;
            text = text?.Trim().Replace(" ", "").Replace("\u00A0", "");

            if (string.IsNullOrWhiteSpace(text))
            {
                if (required)
                {
                    MessageBox.Show(
                        $"В отеле есть номера типа \"{fieldName}\". Укажите для них цену.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                return true;
            }

            if (!double.TryParse(text, out price))
            {
                MessageBox.Show($"Некорректная цена для номера: {fieldName}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (required && price <= 0)
            {
                MessageBox.Show(
                    $"Цена для номера \"{fieldName}\" должна быть больше нуля.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (price < 0)
            {
                MessageBox.Show($"Цена не может быть отрицательной: {fieldName}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
