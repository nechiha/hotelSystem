using HandyControl.Controls;
using HandyControl.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            ConfigHelper.Instance.SetLang("ru");
            InitializeComponent();
        }

        public DataTable Select(string SelectSql)
        {
            DataTable dataTable = new DataTable();
            SqlConnection sqlConn = new SqlConnection(@"data source=DESKTOP-OFPSGQB\SQLEXPRESS;initial catalog=hotelSystem;integrated security=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework");
            sqlConn.Open();
            SqlCommand cmd = sqlConn.CreateCommand();
            cmd.CommandText = SelectSql;
            SqlDataAdapter adapt = new SqlDataAdapter(cmd);
            adapt.Fill(dataTable);
            return dataTable;
        }

        private void enterBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = insertLoginTB.Text?.Trim();
            string password = passwordBox.Password;

            // 1) Проверка ввода
            if (string.IsNullOrWhiteSpace(login) && string.IsNullOrWhiteSpace(password))
            {
                HandyControl.Controls.MessageBox.Show("Введите логин и пароль.", "Вход",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(login))
            {
                HandyControl.Controls.MessageBox.Show("Введите логин.", "Вход",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                HandyControl.Controls.MessageBox.Show("Введите пароль.", "Вход",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Запрос в БД (accounts -> employees -> roleId)
            string sql = $@"
SELECT TOP (1) e.roleId
FROM accounts a
JOIN employees e ON e.employeeId = a.employeeId
WHERE a.accLogin = N'{login.Replace("'", "''")}'
  AND a.accPassword COLLATE Latin1_General_CS_AS = N'{password.Replace("'", "''")}';";

            try
            {
                DataTable dt = Select(sql);

                // 3) Неверный логин/пароль
                if (dt.Rows.Count == 0)
                {
                    HandyControl.Controls.MessageBox.Show("Неверный логин или пароль.", "Вход",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int roleId = Convert.ToInt32(dt.Rows[0]["roleId"]);
                bool isAdmin = (roleId == 1); // Администратор = 1

                // 4) Открываем generalWindow и скрываем/показываем кнопки
                var gw = new generalWindow();

                gw.roomsManagementBtn.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
                gw.rateBtn.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

                gw.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show("Ошибка:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}