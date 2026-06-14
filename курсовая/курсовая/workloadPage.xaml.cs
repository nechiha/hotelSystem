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
using System.IO;
using Microsoft.Win32; 
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Wordprocessing;

namespace курсовая
{
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class workloadPage : System.Windows.Controls.Page
    {
        private int _lastRegular, _lastVip, _lastTotalBusy, _lastTotalRooms;
        private DateTime _lastStart, _lastEnd;

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

                _lastRegular = regular;
                _lastVip = vip;
                _lastTotalBusy = totalBusy;
                _lastTotalRooms = totalRooms;
                _lastStart = periodStart;
                _lastEnd = periodEnd;

                regularValue.Text = regular.ToString();
                vipValue.Text = vip.ToString();
                totalBusyValue.Text = totalBusy.ToString();
                totalRoomsValue.Text = totalRooms.ToString();

                double load = totalRooms > 0 ? (double)totalBusy / totalRooms : 0;
                if (load < 0.5)
                    totalBusyValue.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50)); // зелёный
                else if (load < 0.8)
                    totalBusyValue.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x98, 0x00)); // оранжевый
                else
                    totalBusyValue.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE5, 0x39, 0x35)); // красный

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
                    Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE4, 0x90, 0xA5)),
                    LabelPoint = p => $"{busy}"
                },
                new PieSeries
                {
                    Title = "Свободно",
                    Values = new ChartValues<int> { free },
                    DataLabels = true,
                    Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE0, 0xE0, 0xE0)),
                    LabelPoint = p => $"{free}"
                }
            };
        }

        // ===== EXCEL =====
        private void exportExcelBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Книга Excel (*.xlsx)|*.xlsx",
                FileName = $"Загрузка_{_lastStart:dd.MM.yyyy}-{_lastEnd:dd.MM.yyyy}.xlsx"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                using (var doc = SpreadsheetDocument.Create(dlg.FileName, SpreadsheetDocumentType.Workbook))
                {
                    var wbPart = doc.AddWorkbookPart();
                    wbPart.Workbook = new Workbook();
                    var wsPart = wbPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    wsPart.Worksheet = new Worksheet(sheetData);

                    var sheets = wbPart.Workbook.AppendChild(new Sheets());
                    sheets.Append(new Sheet
                    {
                        Id = wbPart.GetIdOfPart(wsPart),
                        SheetId = 1,
                        Name = "Загрузка"
                    });

                    sheetData.Append(MakeRow($"Отчёт о загрузке отеля за период " +
                        $"{_lastStart:dd.MM.yyyy} — {_lastEnd:dd.MM.yyyy}"));
                    sheetData.Append(MakeRow("")); 

                    sheetData.Append(MakeRow("Показатель", "Значение"));
                    sheetData.Append(MakeRow("Занято обычных номеров", _lastRegular.ToString()));
                    sheetData.Append(MakeRow("Занято VIP номеров", _lastVip.ToString()));
                    sheetData.Append(MakeRow("Всего занято", _lastTotalBusy.ToString()));
                    sheetData.Append(MakeRow("Всего номеров", _lastTotalRooms.ToString()));

                    wbPart.Workbook.Save();
                }

                HandyControl.Controls.MessageBox.Show("Отчёт сохранён в Excel", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show("Ошибка экспорта: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Row MakeRow(params string[] values)
        {
            var row = new Row();
            foreach (var v in values)
            {
                row.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(v)
                });
            }
            return row;
        }

        // ===== WORD =====
        private void exportWordBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Документ Word (*.docx)|*.docx",
                FileName = $"Загрузка_{_lastStart:dd.MM.yyyy}-{_lastEnd:dd.MM.yyyy}.docx"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                using (var doc = WordprocessingDocument.Create(dlg.FileName,
                    WordprocessingDocumentType.Document))
                {
                    var mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new A.Document();
                    var body = mainPart.Document.AppendChild(new A.Body());

                    body.AppendChild(MakeParagraph(
                        $"Отчёт о загрузке отеля за период {_lastStart:dd.MM.yyyy} — {_lastEnd:dd.MM.yyyy}",
                        bold: true, size: 32));
                    body.AppendChild(MakeParagraph(""));

                    var table = new A.Table();

                    table.AppendChild(new A.TableProperties(
                        new A.TableBorders(
                            new A.TopBorder { Val = A.BorderValues.Single, Size = 6 },
                            new A.BottomBorder { Val = A.BorderValues.Single, Size = 6 },
                            new A.LeftBorder { Val = A.BorderValues.Single, Size = 6 },
                            new A.RightBorder { Val = A.BorderValues.Single, Size = 6 },
                            new A.InsideHorizontalBorder { Val = A.BorderValues.Single, Size = 6 },
                            new A.InsideVerticalBorder { Val = A.BorderValues.Single, Size = 6 })));

                    table.AppendChild(MakeTableRow("Показатель", "Значение", header: true));
                    table.AppendChild(MakeTableRow("Занято обычных номеров", _lastRegular.ToString()));
                    table.AppendChild(MakeTableRow("Занято VIP номеров", _lastVip.ToString()));
                    table.AppendChild(MakeTableRow("Всего занято", _lastTotalBusy.ToString()));
                    table.AppendChild(MakeTableRow("Всего номеров", _lastTotalRooms.ToString()));

                    body.AppendChild(table);
                    mainPart.Document.Save();
                }

                HandyControl.Controls.MessageBox.Show("Отчёт сохранён в Word", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show("Ошибка экспорта: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private A.Paragraph MakeParagraph(string text, bool bold = false, int size = 24)
        {
            var run = new A.Run();
            var runProps = new A.RunProperties();
            if (bold) runProps.AppendChild(new A.Bold());
            runProps.AppendChild(new A.FontSize { Val = size.ToString() }); // half-points
            run.AppendChild(runProps);
            run.AppendChild(new A.Text(text));
            return new A.Paragraph(run);
        }

        // Строка таблицы Word
        private A.TableRow MakeTableRow(string c1, string c2, bool header = false)
        {
            return new A.TableRow(
                MakeTableCell(c1, header),
                MakeTableCell(c2, header));
        }

        private A.TableCell MakeTableCell(string text, bool bold)
        {
            var run = new A.Run();
            var runProps = new A.RunProperties();
            if (bold) runProps.AppendChild(new A.Bold());
            run.AppendChild(runProps);
            run.AppendChild(new A.Text(text));

            var cell = new A.TableCell(new A.Paragraph(run));
            cell.AppendChild(new A.TableCellProperties(
                new A.TableCellWidth { Type = A.TableWidthUnitValues.Dxa, Width = "4000" }));
            return cell;
        }
    }
}
