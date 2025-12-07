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
    /// Логика взаимодействия для ProfileWindow.xaml
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5171/")
        };

        public ProfileWindow()
        {
            InitializeComponent();

            Loaded += ProfileWindow_Loaded;
        }

        private async void ProfileWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var employee = MainWindow.CurrentEmployee;
            if (employee != null)
            {
                // Добавляем информацию о сотруднике
                DisplayEmployeeInfo(employee);

                // Загружаем и отображаем статистику
                await LoadAndDisplayStatistics(employee.EmployeeId);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DisplayEmployeeInfo(Employee employee)
        {
            EmployeeInfoPanel.Children.Add(CreateInfoRow("Фамилия:", employee.Surname));
            EmployeeInfoPanel.Children.Add(CreateInfoRow("Имя:", employee.Name));
            EmployeeInfoPanel.Children.Add(CreateInfoRow("Отчество:", employee.Patronymic ?? "Не указано"));
            EmployeeInfoPanel.Children.Add(CreateInfoRow("Логин:", employee.Login));
            EmployeeInfoPanel.Children.Add(CreateInfoRow("Email:", employee.Email ?? "Не указан"));
            EmployeeInfoPanel.Children.Add(CreateInfoRow("ID сотрудника:", employee.EmployeeId.ToString()));

            // Добавляем информацию о роли
            string roleText = employee.RoleId == 2 ? "Супервайзер" : "Оператор";
            EmployeeInfoPanel.Children.Add(CreateInfoRow("Роль:", roleText));
        }

        private async Task LoadAndDisplayStatistics(int employeeId)
        {
            try
            {
                // Загружаем заявки сотрудника
                var response = await _httpClient.GetAsync($"api/calls/employee/{employeeId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var calls = JsonSerializer.Deserialize<List<CallResponse>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (calls != null && calls.Any())
                    {
                        CalculateAndDisplayStatistics(calls);
                    }
                    else
                    {
                        StatisticsPanel.Children.Add(CreateStatRow("Нет данных о заявках", "0"));
                    }
                }
                else
                {
                    StatisticsPanel.Children.Add(CreateStatRow("Ошибка загрузки статистики", ""));
                }
            }
            catch (Exception ex)
            {
                StatisticsPanel.Children.Add(CreateStatRow("Ошибка:", ex.Message));
            }
        }

        private void CalculateAndDisplayStatistics(List<CallResponse> calls)
        {
            // Общая статистика
            int totalCalls = calls.Count;
            int successfulCalls = calls.Count(c => c.Result?.ToLower().Contains("успешно") == true ||
                                                  c.Result?.ToLower().Contains("завершено") == true ||
                                                  c.Result?.ToLower().Contains("продажа") == true);
            int failedCalls = calls.Count(c => c.Result?.ToLower().Contains("неудачно") == true ||
                                              c.Result?.ToLower().Contains("отказ") == true ||
                                              c.Result?.ToLower().Contains("неуспешно") == true);
            int otherCalls = totalCalls - successfulCalls - failedCalls;

            // Средняя длительность
            double avgDuration = calls.Any() ? calls.Average(c => c.Duration) : 0;

            // Заявки по месяцам (последние 3 месяца)
            var threeMonthsAgo = DateTime.Now.AddMonths(-3);
            var recentCalls = calls.Where(c => c.CallDatetime >= threeMonthsAgo).Count();

            // Отображаем статистику
            StatisticsPanel.Children.Add(CreateStatRow("Всего заявок:", totalCalls.ToString()));
            StatisticsPanel.Children.Add(CreateStatRow("Успешных:", $"{successfulCalls} ({CalculatePercentage(successfulCalls, totalCalls)}%)"));
            StatisticsPanel.Children.Add(CreateStatRow("Неуспешных:", $"{failedCalls} ({CalculatePercentage(failedCalls, totalCalls)}%)"));
            StatisticsPanel.Children.Add(CreateStatRow("Прочих:", $"{otherCalls} ({CalculatePercentage(otherCalls, totalCalls)}%)"));
            StatisticsPanel.Children.Add(CreateStatRow("Ср. длительность:", $"{avgDuration:F1} мин"));
            StatisticsPanel.Children.Add(CreateStatRow("Заявок за 3 мес:", recentCalls.ToString()));

            // Создаем график успешности
            CreateSuccessChart(successfulCalls, failedCalls, otherCalls, totalCalls);
        }

        private void CreateSuccessChart(int successful, int failed, int other, int total)
        {
            if (total == 0) return;

            // Успешные заявки
            AddChartBar("Успешные", successful, total, "#4CAF50");

            // Неуспешные заявки
            AddChartBar("Неуспешные", failed, total, "#F44336");

            // Прочие заявки
            AddChartBar("Прочие", other, total, "#FFC107");
        }

        private void AddChartBar(string label, int value, int total, string color)
        {
            if (total == 0) return;

            double percentage = (value * 100.0) / total;

            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

            // Заголовок и процент
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 14,
                Width = 80,
                Foreground = Brushes.Black
            });

            headerPanel.Children.Add(new TextBlock
            {
                Text = $"{value} ({percentage:F1}%)",
                FontSize = 14,
                Margin = new Thickness(10, 0, 0, 0),
                Foreground = Brushes.Black
            });

            stackPanel.Children.Add(headerPanel);

            // Прогресс-бар
            var progressBar = new Border
            {
                Height = 20,
                Width = 250,
                Background = Brushes.LightGray,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0, 5, 0, 0)
            };

            var fillBar = new Border
            {
                Height = 20,
                Width = percentage * 2.5, // 250px * percentage/100
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString(color),
                CornerRadius = new CornerRadius(10),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            };

            progressBar.Child = fillBar;
            stackPanel.Children.Add(progressBar);

            SuccessChartPanel.Children.Add(stackPanel);
        }

        private string CalculatePercentage(int part, int total)
        {
            if (total == 0) return "0";
            return ((part * 100.0) / total).ToString("F1");
        }

        private StackPanel CreateInfoRow(string label, string value)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 8, 0, 8)
            };

            stackPanel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.Bold,
                Width = 150,
                FontSize = 14,
                Foreground = Brushes.Black
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 14,
                Foreground = Brushes.DarkSlateGray
            });

            return stackPanel;
        }

        private StackPanel CreateStatRow(string label, string value)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            stackPanel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                Width = 180,
                FontSize = 14,
                Foreground = Brushes.Black
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkSlateGray
            });

            return stackPanel;
        }
    }
}
