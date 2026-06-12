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
    /// Логика взаимодействия для createRateWindow.xaml
    /// </summary>
    public partial class createRateWindow : Window
    {
        private bool _breakfastIncluded = false;
        private bool _vipBreakfastIncluded = false;

        public createRateWindow()
        {
            InitializeComponent();
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

        private void saveNewRateBtn_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка названия
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
                    // 2. Узнаём, для каких типов номеров реально есть номера в отеле
                    var existingTypeIds = context.rooms
                        .Select(r => r.typeId)
                        .Distinct()
                        .ToList();

                    // 3. Сопоставляем поля ввода с typeId и понятным названием
                    var priceInputs = new List<(int typeId, string text, string name)>
                    {
                        (1, singleRoomPriceTB.Text,        "1-местный"),
                        (2, doubleRoomPriceTB.Text,        "2-местный"),
                        (3, tripleRoomPriceTb.Text,        "3-местный"),
                        (4, quadrupleRoomPriceTB.Text,     "4-местный"),
                        (5, singleVIPRoomPriceTB.Text,     "1-местный VIP"),
                        (6, doubleVIPRoomPriceTB.Text,     "2-местный VIP"),
                        (7, tripleVIPRoomPriceTb.Text,     "3-местный VIP"),
                        (8, quadrupleVIPRoomPriceTB.Text,  "4-местный VIP"),
                    };

                    // 4. Проверяем и собираем цены
                    var pricesToSave = new Dictionary<int, double>();
                    foreach (var input in priceInputs)
                    {
                        bool roomsExist = existingTypeIds.Contains(input.typeId);

                        if (!TryGetPrice(input.text, roomsExist, input.name, out double price))
                            return; // ошибка уже показана

                        pricesToSave[input.typeId] = price;
                    }

                    // 5. Создаём сам тариф
                    var rate = new rates
                    {
                        rateName = rateName,
                        creatingDate = DateTime.Now,
                        isBreakfastIncluded = _breakfastIncluded,
                        isVIPBreakfastIncluded = _vipBreakfastIncluded,
                        isActive = false
                    };
                    context.rates.Add(rate);
                    context.SaveChanges(); // получаем rateId

                    // 6. Создаём строки цен для каждого типа (1..8)
                    foreach (var pair in pricesToSave)
                    {
                        context.rateForRoomTypes.Add(new rateForRoomTypes
                        {
                            typeId = pair.Key,
                            rateId = rate.rateId,
                            price = pair.Value
                        });
                    }
                    context.SaveChanges();

                    MessageBox.Show("Тариф успешно создан", "Успех",
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

        // required = true -> поле обязательно и цена должна быть > 0
        // required = false -> поле можно оставить пустым (вернёт 0)
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
                return true; // номеров такого типа нет -> цена 0
            }

            if (!double.TryParse(text, out price))
            {
                MessageBox.Show($"Некорректная цена для номера: {fieldName}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Если поле заполнено, но номера такого типа есть — цена должна быть > 0
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

        private void singleRoomPriceTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void singleRoomPriceTB_Pasting(object sender, DataObjectPastingEventArgs e)
        {

        }

        private void doubleRoomPriceTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void doubleRoomPriceTB_Pasting(object sender, DataObjectPastingEventArgs e)
        {

        }

        private void tripleRoomPriceTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void tripleRoomPriceTb_Pasting(object sender, DataObjectPastingEventArgs e)
        {

        }

        private void quadrupleRoomPriceTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void quadrupleRoomPriceTB_Pasting(object sender, DataObjectPastingEventArgs e)
        {

        }

        private void singleVIPRoomPriceTB_Pasting(object sender, DataObjectPastingEventArgs e)
        {

        }

        private void singleVIPRoomPriceTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void doubleVIPRoomPriceTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void doubleVIPRoomPriceTB_Pasting(object sender, DataObjectPastingEventArgs e)
        {

        }

        private void tripleVIPRoomPriceTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void tripleVIPRoomPriceTb_Pasting(object sender, DataObjectPastingEventArgs e)
        {

        }

        private void quadrupleVIPRoomPriceTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void quadrupleVIPRoomPriceTB_Pasting(object sender, DataObjectPastingEventArgs e)
        {

        }
    }
}
