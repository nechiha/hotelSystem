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
        public int SelectedRoomId { get; set; }
        public double RoomPrice { get; set; }

        public class SelectedOption
        {
            public int optionId { get; set; }
            public string optionName { get; set; }
            public double price { get; set; }
        }

        private System.Collections.ObjectModel.ObservableCollection<SelectedOption> _selectedOptions
            = new System.Collections.ObjectModel.ObservableCollection<SelectedOption>();



        private void Phone_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void Phone_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = ((string)e.DataObject.GetData(typeof(string))).Replace(" ", "");
                if (!text.All(char.IsDigit)) e.CancelCommand();
            }
            else e.CancelCommand();
        }




        public bookingWindow()
        {
            InitializeComponent();
            LoadOptions();

            arriveDatePick.SelectedDateTimeChanged += (s, e) => { UpdateTotalPrice(); ValidateInput(); };
            departureDatePick.SelectedDateTimeChanged += (s, e) => { UpdateTotalPrice(); ValidateInput(); };

            this.Loaded += BookingWindow_Loaded;

            selectedOptionsList.ItemsSource = _selectedOptions;

        }

        private void LoadOptions()
        {
            using (var context = new hotelSystemEntities1())
            {
                var options = context.options
                    .Where(o => o.optionId != 1)
                    .ToList();
                insertOptionCB.ItemsSource = options;
                insertOptionCB.DisplayMemberPath = "optionName";
                insertOptionCB.SelectedValuePath = "optionId";
            }
        }

        private void addOptionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (insertOptionCB.SelectedItem == null)
            {
                MessageBox.Show("Выберите услугу перед добавлением", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = insertOptionCB.SelectedItem as options;

            if (_selectedOptions.Any(o => o.optionId == selected.optionId))
            {
                MessageBox.Show("Эта услуга уже добавлена", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _selectedOptions.Add(new SelectedOption
            {
                optionId = selected.optionId,
                optionName = selected.optionName,
                price = selected.price
            });

            insertOptionCB.SelectedIndex = -1;
            UpdateTotalPrice();
        }

        private void removeOptionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag != null)
            {
                int optionId = (int)fe.Tag;
                var toRemove = _selectedOptions.FirstOrDefault(o => o.optionId == optionId);
                if (toRemove != null)
                {
                    _selectedOptions.Remove(toRemove);
                    UpdateTotalPrice();
                }
            }
        }

        private void BookingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            int capacity = 1;
            using (var context = new hotelSystemEntities1())
            {
                var room = context.rooms.Find(SelectedRoomId);
                if (room != null)
                    capacity = room.typeId > 4 ? room.typeId - 4 : room.typeId; // 1..4
            }

            peopleAmountCB.Items.Clear();
            for (int i = 1; i <= capacity; i++)
                peopleAmountCB.Items.Add(i);
            peopleAmountCB.SelectedIndex = 0;

            ValidateInput();
        }

        private void ValidateInput(object sender = null, EventArgs e = null)
        {
            bool surnameOk = !string.IsNullOrWhiteSpace(insertSurname.Text);
            bool nameOk = !string.IsNullOrWhiteSpace(insertName.Text);
            bool phoneOk = insertPhone.Text.Trim().Length >= 5;
            bool datesOk = arriveDatePick.SelectedDateTime.HasValue &&
                           departureDatePick.SelectedDateTime.HasValue &&
                           (departureDatePick.SelectedDateTime.Value.Date
                                - arriveDatePick.SelectedDateTime.Value.Date).Days > 0;
            bool peopleOk = peopleAmountCB.SelectedItem != null;

            MarkField(insertSurname, surnameOk);
            MarkField(insertName, nameOk);
            MarkField(insertPhone, phoneOk);

            toBookBtn.IsEnabled = surnameOk && nameOk && phoneOk && datesOk && peopleOk;
        }

        private void MarkField(Control field, bool isOk)
        {
            field.BorderBrush = isOk
                ? new SolidColorBrush(Color.FromRgb(0xE4, 0x90, 0xA5)) // обычный розовый
                : new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)); // красный при ошибке
            field.BorderThickness = new Thickness(1.5);
        }


        /// <summary>
        /// Обновляет отображение общей стоимости
        /// </summary>
        private void UpdateTotalPrice()
        {
            if (!arriveDatePick.SelectedDateTime.HasValue || !departureDatePick.SelectedDateTime.HasValue)
            {
                if (totalPriceText != null) totalPriceText.Text = "0.00 ₽";
                return;
            }

            int days = (departureDatePick.SelectedDateTime.Value.Date
                        - arriveDatePick.SelectedDateTime.Value.Date).Days;
            if (days <= 0)
            {
                if (totalPriceText != null) totalPriceText.Text = "0.00 ₽";
                return;
            }

            double roomTotal = RoomPrice * days;
            double optionsTotal = _selectedOptions.Sum(o => o.price);

            if (totalPriceText != null)
                totalPriceText.Text = $"{roomTotal + optionsTotal:0.00} ₽";
        }

        /// <summary>
        /// Кнопка "Забронировать" — сохраняет всё в БД
        /// </summary>
        private void toBookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(insertSurname.Text) ||
                string.IsNullOrWhiteSpace(insertName.Text) ||
                string.IsNullOrWhiteSpace(insertPhone.Text) ||
                !arriveDatePick.SelectedDateTime.HasValue ||
                !departureDatePick.SelectedDateTime.HasValue ||
                peopleAmountCB.SelectedItem == null ||
                SelectedRoomId == 0)
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string phone = insertPhone.Text.Trim();
            if (phone.Length < 5)
            {
                MessageBox.Show("Введите корректный номер телефона", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime arrive = arriveDatePick.SelectedDateTime.Value;
            DateTime departure = departureDatePick.SelectedDateTime.Value;

            int days = (departure.Date - arrive.Date).Days;
            if (days <= 0)
            {
                MessageBox.Show("Дата выезда должна быть позже даты заезда", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int peopleAmount = (int)peopleAmountCB.SelectedItem;

            using (var context = new hotelSystemEntities1())
            {
                try
                {
                    var roomEntity = context.rooms.Find(SelectedRoomId);
                    if (roomEntity != null)
                    {
                        int capacity = roomEntity.typeId > 4 ? roomEntity.typeId - 4 : roomEntity.typeId;
                        if (peopleAmount > capacity)
                        {
                            MessageBox.Show($"В этот номер можно заселить не более {capacity} чел.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    bool overlap = context.bookingRooms.Any(br =>
                        br.roomId == SelectedRoomId &&
                        br.bookings.bookingStatusId == 1 &&
                        br.arrivingDate < departure &&
                        br.departureDate > arrive);

                    if (overlap)
                    {
                        MessageBox.Show("Этот номер уже забронирован на выбранные даты. Выберите другие даты.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string patronymic = string.IsNullOrWhiteSpace(insertPatronomyc.Text)
                        ? null : insertPatronomyc.Text.Trim();

                    var client = context.clients.FirstOrDefault(c =>
                        c.clientSurname == insertSurname.Text &&
                        c.clientName == insertName.Text &&
                        c.clientPatronomyc == patronymic &&
                        c.clientPhone == phone);

                    if (client == null)
                    {
                        client = new clients
                        {
                            clientSurname = insertSurname.Text.Trim(),
                            clientName = insertName.Text.Trim(),
                            clientPatronomyc = patronymic,
                            clientPhone = phone
                        };
                        context.clients.Add(client);
                        context.SaveChanges();
                    }

                    var booking = new bookings
                    {
                        clientId = client.clientId,
                        amountOfPeople = peopleAmount,
                        bookingStatusId = 1
                    };
                    context.bookings.Add(booking);
                    context.SaveChanges();

                    context.bookingRooms.Add(new bookingRooms
                    {
                        bookingId = booking.bookingId,
                        roomId = SelectedRoomId,
                        arrivingDate = arrive,
                        departureDate = departure
                    });

                    double optionsTotal = 0;
                    foreach (var opt in _selectedOptions)
                    {
                        optionsTotal += opt.price;
                        context.bookingOptions.Add(new bookingOptions
                        {
                            bookingId = booking.bookingId,
                            optionId = opt.optionId
                        });
                    }

                    double totalPrice = RoomPrice * days + optionsTotal;
                    context.bills.Add(new bills
                    {
                        bookingId = booking.bookingId,
                        total = totalPrice
                    });

                    context.SaveChanges();

                    MessageBox.Show($"Бронирование успешно создано!\nСчёт на сумму: {totalPrice:0.00} ₽",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void escapeFromBookingBtn_Click(object sender, RoutedEventArgs e) => this.Close();

        /// <summary>
        /// Вспомогательный метод для поиска элемента по имени в визуальном дереве
        /// </summary>       

        private void arriveDatePick_SelectedDateTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {

        }

        private void departureDatePick_SelectedDateTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {

        }
    }
}
