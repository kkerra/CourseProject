using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TelemarketingApp.Desktop
{
    /// <summary>
    /// Логика взаимодействия для EditEmployeeWindow.xaml
    /// </summary>
    public partial class EditEmployeeWindow : Window
    {
        private HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5171/")
        };

        private readonly EmployeeData _employee;


        public EditEmployeeWindow(EmployeeData employee)
        {
            InitializeComponent();
            _employee = employee;
            Loaded += EditEmployeeWindow_Loaded;
        }

        private void EditEmployeeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SurnameTextBox.Text = _employee.Surname;
            NameTextBox.Text = _employee.Name;
            PatronymicTextBox.Text = _employee.Patronymic;
            LoginTextBox.Text = _employee.Login;
            EmailTextBox.Text = _employee.Email;

            // Устанавливаем роль
            foreach (ComboBoxItem item in RoleComboBox.Items)
            {
                if (int.TryParse(item.Tag.ToString(), out int roleId) && roleId == _employee.RoleId)
                {
                    RoleComboBox.SelectedItem = item;
                    break;
                }
            }

            // Устанавливаем статус
            bool employeeIsActive = _employee.IsActive == 1;
            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (bool.TryParse(item.Tag.ToString(), out bool isActiveBool) && isActiveBool == employeeIsActive)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SurnameTextBox.Text))
            {
                ShowStatus("Фамилия обязательна", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowStatus("Имя обязательно", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                ShowStatus("Логин обязателен", true);
                return;
            }

            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Content = "Сохранение...";
                StatusText.Text = "Сохранение изменений...";

                // Получаем выбранные значения
                var roleItem = RoleComboBox.SelectedItem as ComboBoxItem;
                int roleId = roleItem != null && int.TryParse(roleItem.Tag.ToString(), out int id) ? id : 1;

                var statusItem = StatusComboBox.SelectedItem as ComboBoxItem;
                bool isActiveBool = statusItem != null && bool.TryParse(statusItem.Tag.ToString(), out bool active) ? active : true;

                // Конвертируем bool в ulong
                ulong isActive = isActiveBool ? 1UL : 0UL;

                // Создаем объект для обновления
                var updateRequest = new
                {
                    Surname = SurnameTextBox.Text.Trim(),
                    Name = NameTextBox.Text.Trim(),
                    Patronymic = PatronymicTextBox.Text.Trim(),
                    Login = LoginTextBox.Text.Trim(),
                    Password = string.IsNullOrWhiteSpace(PasswordBox.Password) ? null : PasswordBox.Password,
                    Email = EmailTextBox.Text.Trim(),
                    RoleId = roleId,
                    IsActive = isActive
                };

                // Отправляем на сервер
                var json = JsonSerializer.Serialize(updateRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/employees/{_employee.EmployeeId}", content);

                if (response.IsSuccessStatusCode)
                {
                    ShowStatus("Изменения успешно сохранены!", false);
                    MessageBox.Show("Данные сотрудника обновлены", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ShowStatus($"Ошибка сохранения: {response.StatusCode}", true);
                    MessageBox.Show($"Не удалось обновить данные: {errorContent}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Ошибка: {ex.Message}", true);
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveButton.IsEnabled = true;
                SaveButton.Content = "Сохранить";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowStatus(string message, bool isError)
        {
            StatusText.Text = message;
            StatusText.Foreground = isError ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;
        }
    }
}
