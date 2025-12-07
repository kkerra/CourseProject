using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
using TelemarketingApp.WebApi.DTOs;
using TelemarketingApp.WebApi.Models;

namespace TelemarketingApp.Desktop
{
    /// <summary>
    /// Логика взаимодействия для CreateCallWindow.xaml
    /// </summary>
    public partial class CreateCallWindow : Window
    {
        private readonly HttpClient _httpClient;
        private int? _employeeId;

        public CreateCallWindow(int? employeeId = null)
        {
            InitializeComponent();

            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5171/")};
            _employeeId = employeeId;
            ConnectionDatePicker.SelectedDate = DateTime.Now;
            ResultComboBox.SelectedIndex = 0;
            Loaded += CreateCallWindow_Loaded;
        }

        private async void CreateCallWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDropdownData();
        }

        private async Task LoadDropdownData()
        {
            try
            {
                // Загружаем все сервисы (и услуги, и оборудование)
                var allServices = await _httpClient.GetFromJsonAsync<List<Service>>("api/services");

                if (allServices != null)
                {
                    // Фильтруем услуги (например, по категории "Услуга" или другим признакам)
                    var services = allServices
                        .Where(s => s.Category == "Интернет" || s.Category == "Телевидение" || string.IsNullOrEmpty(s.Category))
                        .ToList();

                    ServiceComboBox.ItemsSource = services;

                    // Фильтруем оборудование (например, по категории "Оборудование" или "Equipment")
                    var equipment = allServices
                        .Where(s => s.Category == "Оборудование" || s.Category == "Equipment")
                        .ToList();

                    EquipmentComboBox.ItemsSource = equipment;

                    // Если оборудование не найдено, делаем поле необязательным
                    if (equipment.Count == 0)
                    {
                        // Можно скрыть или отключить поле оборудования
                        // EquipmentComboBox.IsEnabled = false;
                        // EquipmentComboBox.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (!ValidateData())
                    return;

                var callDto = new CreateCallDto
                {
                    Address = AddressTextBox.Text,
                    ServiceTitle = (ServiceComboBox.SelectedItem as Service)?.Title,
                    EquipmentName = (EquipmentComboBox.SelectedItem as Service)?.Title, 
                    ClientFullName = ClientNameTextBox.Text,
                    ClientPhone = ClientPhoneTextBox.Text,
                    ClientEmail = string.IsNullOrWhiteSpace(ClientEmailTextBox.Text) ?
                        null : ClientEmailTextBox.Text,
                    ConnectionDate = ConnectionDatePicker.SelectedDate ?? DateTime.Now,
                    Comment = CommentTextBox.Text,
                    Duration = int.TryParse(DurationTextBox.Text, out int duration) ? duration : 30,
                    Result = (ResultComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Успешно",
                    EmployeeId = _employeeId
                };

                // Проверяем, что услуга выбрана
                if (string.IsNullOrEmpty(callDto.ServiceTitle))
                {
                    MessageBox.Show("Выберите услугу", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Отправка на сервер
                var response = await _httpClient.PostAsJsonAsync("api/calls/create", callDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CreateCallResponse>();
                    MessageBox.Show($"Заявка успешно создана! Номер заявки: {result?.CallId}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка создания заявки: {error}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateData()
        {
            // Простая валидация
            if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Введите адрес подключения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ClientNameTextBox.Text))
            {
                MessageBox.Show("Введите ФИО клиента", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ClientPhoneTextBox.Text))
            {
                MessageBox.Show("Введите телефон клиента", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (ServiceComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите услугу", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class CreateCallResponse
    {
        public string Message { get; set; } = null!;
        public int CallId { get; set; }
    }
}
