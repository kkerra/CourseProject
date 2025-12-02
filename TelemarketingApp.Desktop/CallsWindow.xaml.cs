using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace TelemarketingApp.Desktop
{
    /// <summary>
    /// Логика взаимодействия для CallsWindow.xaml
    /// </summary>
    public partial class CallsWindow : Window
    {
        public CallsWindow()
        {
            InitializeComponent();
        }

        private async void CallsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCalls();
        }

        private async Task LoadCalls()
        {
            try
            {
                StatusText.Text = "Загрузка заявок...";

                var employee = LoginWindow.CurrentEmployee;
                if (employee == null)
                {
                    MessageBox.Show("Сотрудник не авторизован");
                    Close();
                    return;
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5171/");

                    var response = await client.GetAsync($"api/calls/employee/{employee.EmployeeId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Ответ от сервера: {responseString.Substring(0, Math.Min(500, responseString.Length))}...");

                        var calls = JsonSerializer.Deserialize<List<CallResponse>>(
                            responseString,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true,
                                // Добавьте это для WPF
                                ReferenceHandler = ReferenceHandler.Preserve
                            });

                        if (calls != null)
                        {
                            // Преобразуем для отображения
                            var displayCalls = calls.Select(c => new
                            {
                                c.CallId,
                                c.CallDatetime,
                                c.Duration,
                                c.Result,
                                c.Comment,
                                ClientName = c.Client != null
                                    ? $"{c.Client.Surname} {c.Client.Name} {c.Client.Patronymic}"
                                    : "Клиент не указан",
                                ServicesList = c.Services != null && c.Services.Any()
                                    ? string.Join(", ", c.Services.Select(s => s.Title))
                                    : "Нет услуг"
                            }).ToList();

                            CallsDataGrid.ItemsSource = displayCalls;
                            StatusText.Text = $"Загружено заявок: {displayCalls.Count}";
                        }
                    }
                    else
                    {
                        StatusText.Text = "Ошибка загрузки";
                        MessageBox.Show($"Ошибка: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка подключения";
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadCalls();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateCallButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class CallResponse
    {
        public int CallId { get; set; }
        public DateTime CallDatetime { get; set; }
        public int Duration { get; set; }
        public string Result { get; set; }
        public string Comment { get; set; }
        public ClientResponse Client { get; set; }
        public List<ServiceResponse> Services { get; set; }
    }

    public class ClientResponse
    {
        public int ClientId { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
    }

    public class ServiceResponse
    {
        public int ServiceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }

    // Классы для отображения
    public class CallDisplay
    {
        public int CallId { get; set; }
        public DateTime CallDatetime { get; set; }
        public int Duration { get; set; }
        public string Result { get; set; }
        public string Comment { get; set; }
        public ClientDisplay Client { get; set; }
        public string ServicesList { get; set; }
    }

    public class ClientDisplay
    {
        public int ClientId { get; set; }
        public string FullName { get; set; }
    }
}
