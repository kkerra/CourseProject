using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;

namespace TelemarketingApp.Desktop
{
    /// <summary>
    /// Логика взаимодействия для CallsWindow.xaml
    /// </summary>
    public partial class CallsWindow : Window
    {
        private bool _isSupervisor = false;
        private bool _showAllCalls = false;
        private dynamic _currentEmployee;
        public HttpClient _httpClient = new();

        public CallsWindow()
        {
            InitializeComponent();
            _httpClient.BaseAddress = new Uri("http://localhost:5000/");
        }

        private async void CallsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _currentEmployee = MainWindow.CurrentEmployee;

            if (_currentEmployee != null)
            {
                string fullName = $"{_currentEmployee.Surname} {_currentEmployee.Name}";
                if (!string.IsNullOrWhiteSpace(_currentEmployee.Patronymic))
                {
                    fullName += $" {_currentEmployee.Patronymic}";
                }

                WelcomeText.Text = $"Здравствуйте, {fullName}";

                _isSupervisor = _currentEmployee.RoleId == 2;

                if (_isSupervisor)
                {
                    RoleText.Text = "Супервайзер";
                    AllCallsButton.Visibility = Visibility.Visible;
                    ExportButton.Visibility = Visibility.Visible;
                    OperatorColumn.Visibility = Visibility.Visible;
                    EmployeesButton.Visibility = Visibility.Visible;
                }
                else
                {
                    RoleText.Text = "Оператор";
                    ModeText.Text = "📞 Мои заявки";
                }
            }

            await LoadCalls();
        }

        private bool IsSupervisor(int? roleId)
        {
            return roleId.HasValue && roleId.Value == 2;
        }

        private async Task LoadCalls()
        {
            try
            {
                StatusText.Text = "Загрузка заявок...";

                if (_currentEmployee == null)
                {
                    MessageBox.Show("Сотрудник не авторизован");
                    Close();
                    return;
                }

                List<CallDisplay> displayCalls;

                if (_isSupervisor && _showAllCalls)
                {
                    displayCalls = await LoadAllCalls();
                }
                else
                {
                    int employeeId = _currentEmployee.EmployeeId;
                    displayCalls = await LoadEmployeeCalls(employeeId);
                }

                CallsDataGrid.ItemsSource = displayCalls;
                StatusText.Text = $"Загружено заявок: {displayCalls.Count} | {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка подключения";
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async Task<List<CallDisplay>> LoadEmployeeCalls(int employeeId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/calls/employee/{employeeId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    var calls = JsonSerializer.Deserialize<List<CallResponse>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (calls != null)
                    {
                        string employeeName = $"{_currentEmployee.Name} {_currentEmployee.Surname}";

                        return calls.Select(c => new CallDisplay
                        {
                            CallId = c.CallId,
                            CallDatetime = c.CallDatetime,
                            Duration = c.Duration,
                            Result = c.Result,
                            Comment = c.Comment,
                            OperatorName = employeeName,
                            Client = c.Client != null
                                ? new ClientDisplay
                                {
                                    ClientId = c.Client.ClientId,
                                    FullName = $"{c.Client.Surname} {c.Client.Name} {c.Client.Patronymic}"
                                }
                                : null,
                            ServicesList = c.Services != null && c.Services.Any()
                                ? string.Join(", ", c.Services.Select(s => s.Title))
                                : "Нет услуг"
                        }).ToList();
                    }
                }
                else
                {
                    MessageBox.Show($"Ошибка сервера: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}");
            }

            return new List<CallDisplay>();
        }

        private async Task<List<CallDisplay>> LoadAllCalls()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/calls");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ответ от api/calls/all: {responseString}"); // Для отладки

                    // Десериализуем в динамический объект
                    var calls = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (calls != null && calls.Any())
                    {
                        var result = new List<CallDisplay>();

                        foreach (var call in calls)
                        {
                            try
                            {
                                var callDisplay = new CallDisplay
                                {
                                    CallId = call.ContainsKey("callId") ? call["callId"].GetInt32() : 0,
                                    CallDatetime = call.ContainsKey("callDatetime") ? call["callDatetime"].GetDateTime() : DateTime.MinValue,
                                    Duration = call.ContainsKey("duration") ? call["duration"].GetInt32() : 0,
                                    Result = call.ContainsKey("result") ? call["result"].GetString() : "",
                                    Comment = call.ContainsKey("comment") ? call["comment"].GetString() : "",
                                    OperatorName = call.ContainsKey("employeeName") ? call["employeeName"].GetString() : "Неизвестно",
                                    Client = new ClientDisplay
                                    {
                                        ClientId = call.ContainsKey("clientId") ? call["clientId"].GetInt32() : 0,
                                        FullName = call.ContainsKey("clientName") ? call["clientName"].GetString() : "Клиент не указан"
                                    },
                                    ServicesList = call.ContainsKey("servicesList") ? call["servicesList"].GetString() : "Нет услуг"
                                };

                                result.Add(callDisplay);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка парсинга заявки: {ex.Message}");
                            }
                        }

                        return result;
                    }
                }
                else
                {
                    MessageBox.Show($"Ошибка сервера: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки всех заявок: {ex.Message}");
            }

            return new List<CallDisplay>();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadCalls();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CurrentEmployee = null;
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileWindow window = new();
            window.Show();
        }

        private void CreateCallButton_Click(object sender, RoutedEventArgs e)
        {
            CreateCallWindow window = new CreateCallWindow(MainWindow.CurrentEmployee.EmployeeId);
            window.ShowDialog();
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Показываем диалог сохранения файла
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel файлы (*.xlsx)|*.xlsx|CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                    FileName = $"Заявки_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await ExportToCsv(saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportToCsv(string filePath)
        {
            try
            {
                StatusText.Text = "Экспорт данных...";

                // Получаем все заявки для экспорта
                var allCalls = await GetAllCallsForExport();

                if (allCalls == null || !allCalls.Any())
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText.Text = "Нет данных для экспорта";
                    return;
                }

                // Создаем CSV файл
                using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    // Заголовки
                    writer.WriteLine("Дата и время;Длительность (мин);Результат;Комментарий;Оператор;Клиент;Услуги");

                    // Данные
                    foreach (var call in allCalls)
                    {
                        // Экранируем кавычки в комментариях
                        string comment = call.Comment?.Replace("\"", "\"\"") ?? "";

                        var line = $"{call.CallDatetime:dd.MM.yyyy HH:mm};" +
                                   $"{call.Duration};" +
                                   $"{call.Result};" +
                                   $"\"{comment}\";" +
                                   $"{call.OperatorName};" +
                                   $"{call.ClientName};" +
                                   $"{call.ServicesList}";

                        writer.WriteLine(line);
                    }
                }

                StatusText.Text = $"Экспорт завершен: {allCalls.Count} записей";
                MessageBox.Show($"Данные успешно экспортированы в файл:\n{filePath}\n\nВсего записей: {allCalls.Count}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка экспорта";
            }
        }

        private async Task<List<CallForExport>> GetAllCallsForExport()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/calls");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var calls = JsonSerializer.Deserialize<List<CallForExport>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return calls ?? new List<CallForExport>();
                }
                else
                {
                    MessageBox.Show($"Ошибка сервера: {response.StatusCode}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new List<CallForExport>();
        }

        private async void AllCallsButton_Click(object sender, RoutedEventArgs e)
        {
            _showAllCalls = !_showAllCalls;

            if (_showAllCalls)
            {
                AllCallsButton.Content = "📋 Мои заявки";
                ModeText.Text = "📊 Все заявки операторов";
            }
            else
            {
                AllCallsButton.Content = "📋 Все заявки";
                ModeText.Text = "📞 Мои заявки";
            }

            await LoadCalls();
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            ClientsWindow window = new();
            window.ShowDialog();
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeesWindow window = new EmployeesWindow();
            window.Owner = this;
            window.ShowDialog();
        }
    }

    public class CallForExport
    {
        public int CallId { get; set; }
        public DateTime CallDatetime { get; set; }
        public int Duration { get; set; }
        public string Result { get; set; }
        public string Comment { get; set; }
        public string OperatorName { get; set; }
        public string ClientName { get; set; }
        public string ServicesList { get; set; }
    }

    public class SupervisorCallResponse
    {
        public int CallId { get; set; }
        public DateTime CallDatetime { get; set; }
        public int Duration { get; set; }
        public string Result { get; set; }
        public string Comment { get; set; }
        public string EmployeeName { get; set; }
        public int? ClientId { get; set; }
        public string ClientName { get; set; }
        public string ServicesList { get; set; }
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
        public string OperatorName { get; set; }
        public ClientDisplay Client { get; set; }
        public string ServicesList { get; set; }
    }

    public class ClientDisplay
    {
        public int ClientId { get; set; }
        public string FullName { get; set; }
    }

    public class AllCallsResponse
    {
        public int CallId { get; set; }
        public DateTime CallDatetime { get; set; }
        public int Duration { get; set; }
        public string Result { get; set; }
        public string Comment { get; set; }
        public string EmployeeName { get; set; }
        public int? ClientId { get; set; }
        public string ClientName { get; set; }
        public string ServicesList { get; set; }
    }
}
