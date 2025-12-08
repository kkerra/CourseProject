using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TelemarketingApp.WebApi.Models;

namespace TelemarketingApp.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static dynamic CurrentEmployee { get; set; }
        private HttpClient _httpClient;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };
        }

        private async void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            await TryLogin();
        }

        private async Task TryLogin()
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Введите логин и пароль", "Ошибка", MessageBoxImage.Warning);
                return;
            }

            ToggleLoginButton(false, "Вход...");

            try
            {
                var loginRequest = new { Login = login, Password = password };
                var jsonContent = new StringContent(JsonSerializer.Serialize(loginRequest), System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Auth/login", jsonContent);
                await HandleResponse(response);
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
            finally
            {
                ToggleLoginButton(true, "Войти");
            }
        }


        private async Task HandleResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var employee = JsonSerializer.Deserialize<Employee>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (employee != null)
                {
                    CurrentEmployee = employee;
                    new CallsWindow().Show();
                    this.Close();
                }
                else
                {
                    ShowMessage("Ошибка при получении данных", "Ошибка", MessageBoxImage.Error);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ShowMessage("Неверный логин или пароль", "Ошибка авторизации", MessageBoxImage.Error);
            }
            else
            {
                ShowMessage($"Ошибка сервера: {response.StatusCode}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private void ShowMessage(string message, string caption, MessageBoxImage icon)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, icon);
        }

        private void ToggleLoginButton(bool isEnabled, string content)
        {
            AuthButton.IsEnabled = isEnabled;
            AuthButton.Content = content;
        }
    }
}