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
using курсовая.Properties;

namespace курсовая
{
    public partial class bookingWindow : Window
    {
        // 🔥 Храним добавленные опции (ComboBox + выбранный optionId)
        private List<(ComboBox comboBox, int optionId)> _addedOptions = new List<(ComboBox comboBox, int optionId)>();
        // 🔥 Данные комнаты (передаются из mainPage)
        public int SelectedRoomId { get; set; }
        public double RoomPrice { get; set; }

        public bookingWindow()
        {
            InitializeComponent();
            LoadComboBoxes();

            // 🔥 Подписываемся на события изменения дат
            arriveDatePick.SelectedDateTimeChanged += (s, e) => UpdateTotalPrice();
            departureDatePick.SelectedDateTimeChanged += (s, e) => UpdateTotalPrice();
        }


        /// <summary>
        /// Загружает данные в комбобоксы
        /// </summary>
        private void LoadComboBoxes()
        {
            using (var context = new hotelSystemEntities1())
            {
                // Кол-во персон
                for (int i = 1; i <= 10; i++)
                    peopleAmountCB.Items.Add(i);
                peopleAmountCB.SelectedIndex = 0;

                // Опции
                var options = context.options.ToList();
                insertOptionCB.ItemsSource = options;
                insertOptionCB.DisplayMemberPath = "optionName";
                insertOptionCB.SelectedValuePath = "optionId";
            }
        }

        /// <summary>
        /// Кнопка "+" — добавляет новый ComboBox для опции
        /// </summary>
        private void addOptionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (insertOptionCB.SelectedItem == null)
            {
                MessageBox.Show("Выберите опцию перед добавлением", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedOption = insertOptionCB.SelectedItem as options;

            // 🔥 Пропускаем "Нет" (optionId = 1)
            if (selectedOption.optionId == 1)
            {
                MessageBox.Show("Опция 'Нет' не добавляется", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var newCombo = new ComboBox
            {
                Width = 300,
                Height = 35,
                Margin = new Thickness(0, 5, 0, 0),
                FontSize = 15,
                ItemsSource = insertOptionCB.ItemsSource,
                DisplayMemberPath = "optionName",
                SelectedValuePath = "optionId",
                Tag = "dynamicOption"
            };
            newCombo.SelectedValue = selectedOption.optionId;

            var optionsPanel = FindVisualChild<StackPanel>(this, "optionsPanel");
            if (optionsPanel != null)
                optionsPanel.Children.Add(newCombo);

            _addedOptions.Add((newCombo, selectedOption.optionId));

            var margin = addOptionBtn.Margin;
            addOptionBtn.Margin = new Thickness(margin.Left, margin.Top + 40, margin.Right, margin.Bottom);

            // 🔥 Обновляем цену
            UpdateTotalPrice();

            insertOptionCB.SelectedIndex = -1;
        }

        /// <summary>
        /// Обновляет отображение общей стоимости
        /// </summary>
        private void UpdateTotalPrice()
        {
            if (!arriveDatePick.SelectedDateTime.HasValue || !departureDatePick.SelectedDateTime.HasValue)
            {
                if (totalPriceText != null)
                    totalPriceText.Text = "0.00 ₽";
                return;
            }

            int days = (departureDatePick.SelectedDateTime.Value.Date - arriveDatePick.SelectedDateTime.Value.Date).Days;
            if (days <= 0)
            {
                if (totalPriceText != null)
                    totalPriceText.Text = "0.00 ₽";
                return;
            }

            double roomTotal = RoomPrice * days;
            double optionsTotal = 0;

            // Считаем стоимость добавленных опций (ПРОПУСКАЕМ optionId = 1)
            foreach (var item in _addedOptions)
            {
                if (item.optionId == 1) // Пропускаем "Нет"
                    continue;

                using (var context = new hotelSystemEntities1())
                {
                    var option = context.options.Find(item.optionId);
                    if (option != null)
                        optionsTotal += option.price;
                }
            }

            // 🔥 Обновляем цену
            if (totalPriceText != null)
                totalPriceText.Text = $"{roomTotal + optionsTotal:0.00} ₽";
        }

        /// <summary>
        /// Кнопка "Забронировать" — сохраняет всё в БД
        /// </summary>
        private void toBookBtn_Click(object sender, RoutedEventArgs e)
        {
            // 🔥 Валидация
            if (string.IsNullOrWhiteSpace(insertSurname.Text) ||
                string.IsNullOrWhiteSpace(insertName.Text) ||
                !arriveDatePick.SelectedDateTime.HasValue ||
                !departureDatePick.SelectedDateTime.HasValue ||
                SelectedRoomId == 0)
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int days = (departureDatePick.SelectedDateTime.Value.Date - arriveDatePick.SelectedDateTime.Value.Date).Days;
            if (days <= 0)
            {
                MessageBox.Show("Дата выезда должна быть позже даты заезда", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new hotelSystemEntities1())
            {
                try
                {
                    // 🔥 1. Ищем или создаём клиента
                    var client = context.clients
                        .FirstOrDefault(c => c.clientSurname == insertSurname.Text &&
                                           c.clientName == insertName.Text &&
                                           c.clientPatronomyc == insertPatronomyc.Text);

                    if (client == null)
                    {
                        client = new clients
                        {
                            clientSurname = insertSurname.Text,
                            clientName = insertName.Text,
                            clientPatronomyc = insertPatronomyc.Text
                        };
                        context.clients.Add(client);
                        context.SaveChanges();
                    }

                    // 🔥 2. Создаём бронь
                    var booking = new bookings
                    {
                        clientId = client.clientId,
                        amountOfPeople = peopleAmountCB.SelectedIndex + 1,
                        bookingStatusId = 1, // Подтверждено
                    };

                    context.bookings.Add(booking);
                    context.SaveChanges();

                    // 🔥 3. Создаём связь номер-бронь
                    var bookingRoom = new bookingRooms
                    {
                        bookingId = booking.bookingId,
                        roomId = SelectedRoomId,
                        arrivingDate = arriveDatePick.SelectedDateTime.Value,
                        departureDate = departureDatePick.SelectedDateTime.Value
                    };
                    context.bookingRooms.Add(bookingRoom);

                    // 🔥 4. Добавляем опции (ПРОПУСКАЕМ optionId = 1)
                    double optionsTotal = 0;
                    foreach (var item in _addedOptions)
                    {
                        // 🔥 Пропускаем "Нет" (optionId = 1)
                        if (item.optionId == 1)
                            continue;

                        var option = context.options.Find(item.optionId);
                        if (option != null)
                        {
                            optionsTotal += option.price;

                            var bookingOption = new bookingOptions
                            {
                                bookingId = booking.bookingId,
                                optionId = item.optionId
                            };
                            context.bookingOptions.Add(bookingOption);
                        }
                    }

                    // 🔥 5. Считаем общую стоимость
                    double roomTotal = RoomPrice * days;
                    double totalPrice = roomTotal + optionsTotal;

                    // 🔥 6. СОЗДАЁМ СЧЁТ В ТАБЛИЦЕ bills
                    var bill = new bills
                    {
                        bookingId = booking.bookingId,
                        total = totalPrice
                    };
                    context.bills.Add(bill);

                    // 🔥 7. Обновляем статус комнаты
                    var room = context.rooms.Find(SelectedRoomId);
                    if (room != null)
                    {
                        room.roomStatusId = 2; // занята
                    }

                    context.SaveChanges();

                    MessageBox.Show($"Бронирование успешно создано!\nСчёт на сумму: {totalPrice:0.00} ₽", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void escapeFromBookingBtn_Click(object sender, RoutedEventArgs e) => this.Close();

        /// <summary>
        /// Вспомогательный метод для поиска элемента по имени в визуальном дереве
        /// </summary>
        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T frameworkElement && frameworkElement.Name == childName)
                    return frameworkElement;

                var childOfChild = FindVisualChild<T>(child, childName);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void arriveDatePick_SelectedDateTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {

        }

        private void departureDatePick_SelectedDateTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {

        }
    }
}
