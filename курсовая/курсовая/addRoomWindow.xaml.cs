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

namespace курсовая
{
    /// <summary>
    /// Логика взаимодействия для addRoomWindow.xaml
    /// </summary>
    public partial class addRoomWindow : Window
    {
        public addRoomWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            // Этажи 1..5
            for (int i = 1; i <= 5; i++)
                chooseFloorCB.Items.Add(i);

            // Вместимость 1..4
            for (int i = 1; i <= 4; i++)
                chooseCapacityCB.Items.Add(i);

            // Категория
            chooseCategoryCB.Items.Add("Обычный");
            chooseCategoryCB.Items.Add("VIP");
        }

        private void saveRoomBtn_Click_1(object sender, RoutedEventArgs e)
        {
            // 1. Проверка заполнения
            if (chooseFloorCB.SelectedItem == null)
            {
                MessageBox.Show("Выберите этаж", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (chooseCapacityCB.SelectedItem == null)
            {
                MessageBox.Show("Выберите вместимость", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (chooseCategoryCB.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(insertRoomTB.Text))
            {
                MessageBox.Show("Введите номер комнаты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Парсим номер комнаты
            if (!int.TryParse(insertRoomTB.Text.Trim(), out int roomNumber) || roomNumber <= 0)
            {
                MessageBox.Show("Номер комнаты должен быть положительным числом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int floor = (int)chooseFloorCB.SelectedItem;
            int capacity = (int)chooseCapacityCB.SelectedItem;
            bool isVip = chooseCategoryCB.SelectedItem.ToString() == "VIP";

            // 3. Вычисляем typeId: 1-4 обычные, 5-8 VIP
            int typeId = capacity + (isVip ? 4 : 0);

            using (var context = new hotelSystemEntities1())
            {
                try
                {
                    // 4. Проверка уникальности номера комнаты
                    bool exists = context.rooms.Any(r => r.roomNumber == roomNumber);
                    if (exists)
                    {
                        MessageBox.Show($"Номер комнаты {roomNumber} уже существует. Введите другой.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // 5. Создаём номер
                    var room = new rooms
                    {
                        roomNumber = roomNumber,
                        roomFloor = floor,
                        typeId = typeId,
                        roomDescription = string.IsNullOrWhiteSpace(insertDescriptionTB.Text)
                            ? null : insertDescriptionTB.Text.Trim(),
                        roomImage = imageSelector.Uri?.ToString(), // путь к изображению (если выбрано)
                        roomStatusId = 1 // свободен
                    };

                    context.rooms.Add(room);
                    context.SaveChanges();

                    MessageBox.Show("Номер успешно добавлен", "Успех",
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

        // Только цифры в номере комнаты
        private void RoomNumberTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void RoomNumberTB_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = ((string)e.DataObject.GetData(typeof(string))).Replace(" ", "");
                if (!text.All(char.IsDigit))
                    e.CancelCommand();
            }
            else e.CancelCommand();
        }
    }
}
