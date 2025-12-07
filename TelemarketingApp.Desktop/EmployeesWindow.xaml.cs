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
using TelemarketingApp.WebApi.Models;

namespace TelemarketingApp.Desktop
{
    /// <summary>
    /// Логика взаимодействия для EmployeesWindow.xaml
    /// </summary>
    public partial class EmployeesWindow : Window
    {
        private HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5171/")
        };

        private List<EmployeeData> _employees = new List<EmployeeData>();

        public EmployeesWindow()
        {
            InitializeComponent();
            Loaded += EmployeesWindow_Loaded;
        }

        private async void EmployeesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadEmployees();
        }

        private async Task LoadEmployees()
        {
            try
            {
                StatusText.Text = "Загрузка сотрудников...";

                var response = await _httpClient.GetAsync("api/employees");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    _employees = JsonSerializer.Deserialize<List<EmployeeData>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<EmployeeData>();

                    if (_employees != null)
                    {
                        // Добавляем RoleName и Status для отображения
                        foreach (var employee in _employees)
                        {
                            employee.RoleName = employee.RoleId == 2 ? "Супервайзер" : "Оператор";
                            employee.Status = employee.IsActive == 1 ? "Активен" : "Уволен";
                        }

                        EmployeesDataGrid.ItemsSource = _employees;
                        StatusText.Text = $"Загружено сотрудников: {_employees.Count} | {DateTime.Now:HH:mm:ss}";
                    }
                }
                else
                {
                    StatusText.Text = "Ошибка загрузки сотрудников";
                    MessageBox.Show($"Ошибка сервера: {response.StatusCode}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка подключения";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadEmployees();
        }

        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            AddEmployeeWindow window = new AddEmployeeWindow();
            window.Owner = this;
            window.Closed += async (s, args) =>
            {
                await LoadEmployees();
            };
            window.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is EmployeeData employee)
            {
                // Нельзя удалить самого себя
                var currentUser = MainWindow.CurrentEmployee;
                if (currentUser != null && employee.EmployeeId == currentUser.EmployeeId)
                {
                    MessageBox.Show("Вы не можете удалить самого себя!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите полностью удалить сотрудника {employee.Surname} {employee.Name}?\nЭто действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await DeleteEmployee(employee.EmployeeId);
                }
            }
        }

        private async void FireButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is EmployeeData employee)
            {
                // Нельзя уволить самого себя
                var currentUser = MainWindow.CurrentEmployee;
                if (currentUser != null && employee.EmployeeId == currentUser.EmployeeId)
                {
                    MessageBox.Show("Вы не можете уволить самого себя!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool isCurrentlyActive = employee.IsActive == 1;
                string action = isCurrentlyActive ? "уволить" : "восстановить";
                string message = isCurrentlyActive
                    ? $"Вы уверены, что хотите уволить сотрудника {employee.Surname} {employee.Name}?"
                    : $"Вы уверены, что хотите восстановить сотрудника {employee.Surname} {employee.Name}?";

                var result = MessageBox.Show(message, "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ulong newStatus = isCurrentlyActive ? 0UL : 1UL;
                    await UpdateEmployeeStatus(employee.EmployeeId, newStatus);
                }
            }
        }

        private async Task UpdateEmployeeStatus(int employeeId, ulong isActive)
        {
            try
            {
                // Создаем объект с правильным именем свойства (должно совпадать с моделью в API)
                var request = new
                {
                    IsActive = isActive  // Имя свойства должно совпадать с моделью UpdateStatusRequest в API
                };

                var json = JsonSerializer.Serialize(request);
                Console.WriteLine($"Отправляем JSON: {json}"); // Для отладки

                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/employees/{employeeId}/status", content);

                if (response.IsSuccessStatusCode)
                {
                    string action = isActive == 1 ? "восстановлен" : "уволен";
                    MessageBox.Show($"Сотрудник успешно {action}!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadEmployees();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка: {response.StatusCode}, Ответ: {errorContent}"); // Для отладки
                    MessageBox.Show($"Ошибка изменения статуса: {response.StatusCode}\n{errorContent}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is EmployeeData employee)
            {
                EditEmployeeWindow window = new EditEmployeeWindow(employee);
                window.Owner = this;
                window.Closed += async (s, args) =>
                {
                    await LoadEmployees();
                };
                window.ShowDialog();
            }
        }

        private async Task ToggleEmployeeStatus(int employeeId, bool isActive)
        {
            try
            {
                var request = new { IsActive = isActive };
                var json = JsonSerializer.Serialize(request);
                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/employees/{employeeId}/status", content);

                if (response.IsSuccessStatusCode)
                {
                    string action = isActive ? "восстановлен" : "уволен";
                    MessageBox.Show($"Сотрудник успешно {action}!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadEmployees();
                }
                else
                {
                    MessageBox.Show($"Ошибка изменения статуса: {response.StatusCode}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteEmployee(int employeeId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/employees/{employeeId}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Сотрудник успешно удален!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadEmployees();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    MessageBox.Show("Нельзя удалить сотрудника, у которого есть заявки!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка удаления: {response.StatusCode}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class EmployeeData
    {
        public int EmployeeId { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public ulong IsActive { get; set; }
        public string Status { get; set; }

        public bool IsActiveBool
        {
            get => IsActive == 1;
            set => IsActive = value ? 1UL : 0UL;
        }
    }
}
