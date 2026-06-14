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
using System.IO;
using Path = System.IO.Path;

namespace курсовая
{
        public partial class addRoomWindow : Window
        {
            public addRoomWindow()
            {
                InitializeComponent();
                LoadComboBoxes();
            }

            private void LoadComboBoxes()
            {
                for (int i = 1; i <= 5; i++)
                    chooseFloorCB.Items.Add(i);

                for (int i = 1; i <= 4; i++)
                    chooseCapacityCB.Items.Add(i);

                chooseCategoryCB.Items.Add("Обычный");
                chooseCategoryCB.Items.Add("VIP");
            }

            private void saveRoomBtn_Click_1(object sender, RoutedEventArgs e)
            {
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

                if (!int.TryParse(insertRoomTB.Text.Trim(), out int roomNumber) || roomNumber <= 0)
                {
                    MessageBox.Show("Номер комнаты должен быть положительным числом", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int floor = (int)chooseFloorCB.SelectedItem;
                int capacity = (int)chooseCapacityCB.SelectedItem;
                bool isVip = chooseCategoryCB.SelectedItem.ToString() == "VIP";

                int typeId = capacity + (isVip ? 4 : 0);

                using (var context = new hotelSystemEntities1())
                {
                    try
                    {
                        bool exists = context.rooms.Any(r => r.roomNumber == roomNumber);

                        if (exists)
                        {
                            MessageBox.Show($"Номер комнаты {roomNumber} уже существует. Введите другой.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        string savedImagePath = SaveRoomImage(roomNumber);

                        var room = new rooms
                        {
                            roomNumber = roomNumber,
                            roomFloor = floor,
                            typeId = typeId,
                            roomDescription = string.IsNullOrWhiteSpace(insertDescriptionTB.Text)
                                ? null
                                : insertDescriptionTB.Text.Trim(),

                            roomImage = savedImagePath,

                            roomStatusId = 1 
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

            /// <summary>
            /// Копирует выбранное через ImageSelector фото в папку RoomImages
            /// и возвращает относительный путь для сохранения в БД.
            /// </summary>
            private string SaveRoomImage(int roomNumber)
            {
                if (imageSelector.Uri == null)
                    return null;

                try
                {
                    Uri selectedUri = imageSelector.Uri;

                    string sourcePath;

                    // Обычно ImageSelector возвращает file:///D:/...
                    if (selectedUri.IsFile)
                    {
                        sourcePath = selectedUri.LocalPath;
                    }
                    else
                    {
                        sourcePath = selectedUri.ToString();
                    }

                    if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                        return null;

                    string imagesFolder = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "RoomImages"
                    );

                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    string extension = Path.GetExtension(sourcePath);

                    if (string.IsNullOrWhiteSpace(extension))
                        extension = ".jpg";

                    string fileName = $"room_{roomNumber}_{Guid.NewGuid()}{extension}";
                    string destinationPath = Path.Combine(imagesFolder, fileName);

                    File.Copy(sourcePath, destinationPath, true);

                    // Именно это сохраняется в БД
                    return $"RoomImages/{fileName}";
                }
                catch
                {
                    MessageBox.Show("Не удалось сохранить изображение номера. Номер будет добавлен без фотографии.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return null;
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
                else
                {
                    e.CancelCommand();
                }
            }
        }
    }
