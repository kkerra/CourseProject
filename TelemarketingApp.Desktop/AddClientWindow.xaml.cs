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
    /// Логика взаимодействия для AddClientWindow.xaml
    /// </summary>
    public partial class AddClientWindow : Window
    {
        private HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5000/")
        };

        public AddClientWindow()
        {
            InitializeComponent();

            StatusComboBox.SelectedIndex = 0;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SurnameTextBox.Text))
            {
                ShowStatus("Фамилия обязательна для заполнения", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowStatus("Имя обязательно для заполнения", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                ShowStatus("Номер телефона обязателен для заполнения", true);
                return;
            }

            try
            {
                // Блокируем кнопку
                SaveButton.IsEnabled = false;
                SaveButton.Content = "Сохранение...";
                StatusText.Text = "Сохранение клиента...";

                // Создаем объект клиента
                var client = new
                {
                    Surname = SurnameTextBox.Text.Trim(),
                    Name = NameTextBox.Text.Trim(),
                    Patronymic = PatronymicTextBox.Text.Trim(),
                    Address = AddressTextBox.Text.Trim(),
                    PhoneNumber = PhoneTextBox.Text.Trim(),
                    InteractionStatus = (StatusComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString()
                };

                // Отправляем на сервер
                var json = JsonSerializer.Serialize(client);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/clients", content);

                if (response.IsSuccessStatusCode)
                {
                    ShowStatus("Клиент успешно добавлен!", false);
                    MessageBox.Show("Клиент успешно добавлен в базу данных", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Закрываем окно через 1 секунду
                    await System.Threading.Tasks.Task.Delay(1000);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ShowStatus($"Ошибка сервера: {response.StatusCode}", true);
                    MessageBox.Show($"Не удалось добавить клиента: {errorContent}", "Ошибка",
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
