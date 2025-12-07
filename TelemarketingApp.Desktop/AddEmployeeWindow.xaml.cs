using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace TelemarketingApp.Desktop
{
    /// <summary>
    /// Логика взаимодействия для AddEmployeeWindow.xaml
    /// </summary>
    public partial class AddEmployeeWindow : Window
    {
        private HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5171/")
        };

        public AddEmployeeWindow()
        {
            InitializeComponent();

            RoleComboBox.SelectedIndex = 0;
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

            if (PasswordBox.Password.Length < 4)
            {
                ShowStatus("Пароль должен быть не менее 4 символов", true);
                return;
            }

            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Content = "Сохранение...";
                StatusText.Text = "Сохранение сотрудника...";

                // Получаем выбранную роль
                var roleItem = RoleComboBox.SelectedItem as ComboBoxItem;
                int roleId = roleItem != null && int.TryParse(roleItem.Tag.ToString(), out int id) ? id : 1;

                // Создаем объект сотрудника
                var employee = new
                {
                    Surname = SurnameTextBox.Text.Trim(),
                    Name = NameTextBox.Text.Trim(),
                    Patronymic = PatronymicTextBox.Text.Trim(),
                    Login = LoginTextBox.Text.Trim(),
                    Password = PasswordBox.Password,
                    Email = EmailTextBox.Text.Trim(),
                    RoleId = roleId
                };

                // Отправляем на сервер
                var json = JsonSerializer.Serialize(employee);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/employees", content);

                if (response.IsSuccessStatusCode)
                {
                    ShowStatus("Сотрудник успешно добавлен!", false);
                    MessageBox.Show("Сотрудник успешно добавлен в систему", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await System.Threading.Tasks.Task.Delay(1000);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ShowStatus($"Ошибка сервера: {response.StatusCode}", true);
                    MessageBox.Show($"Не удалось добавить сотрудника: {errorContent}", "Ошибка",
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
            this.Close();
        }

        private void ShowStatus(string message, bool isError)
        {
            StatusText.Text = message;
            StatusText.Foreground = isError ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;
        }
    }
}
