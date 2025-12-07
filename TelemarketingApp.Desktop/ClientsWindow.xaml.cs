using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TelemarketingApp.WebApi.DTOs;

namespace TelemarketingApp.Desktop
{
    /// <summary>
    /// Логика взаимодействия для ClientsWindow.xaml
    /// </summary>
    public partial class ClientsWindow : Window
    {

        private HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5171/")
        };

        private List<ClientData> _allClients = new List<ClientData>();
        private ICollectionView _clientsView;
        private string _currentSortProperty = "Surname";
        private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;

        public ClientsWindow()
        {
            InitializeComponent();
            Loaded += ClientsWindow_Loaded;

            StatusFilterComboBox.SelectedIndex = 0;
            SortComboBox.SelectedIndex = 0;
        }

        private async void ClientsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClients();
        }

        private async Task LoadClients()
        {
            try
            {
                StatusText.Text = "Загрузка клиентов...";
                FilterStatusText.Text = "";

                var response = await _httpClient.GetAsync("api/clients");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    _allClients = JsonSerializer.Deserialize<List<ClientData>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ClientData>();

                    ApplyFiltersAndSorting();
                    StatusText.Text = $"Всего клиентов: {_allClients.Count} | {DateTime.Now:HH:mm:ss}";
                }
                else
                {
                    StatusText.Text = "Ошибка загрузки клиентов";
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

        private void ApplyFiltersAndSorting()
        {
            // Создаем представление для фильтрации и сортировки
            _clientsView = CollectionViewSource.GetDefaultView(_allClients);

            // Применяем фильтры
            _clientsView.Filter = ClientFilter;

            // Применяем сортировку
            ApplySorting();

            // Обновляем DataGrid
            ClientsDataGrid.ItemsSource = _clientsView;

            // Обновляем статус фильтров
            UpdateFilterStatus();
        }

        private bool ClientFilter(object item)
        {
            if (item is not ClientData client) return false;

            // Фильтр по поиску
            string searchText = SearchTextBox.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                bool matchesSearch =
                    client.Surname.ToLower().Contains(searchText) ||
                    client.Name.ToLower().Contains(searchText) ||
                    (client.Patronymic?.ToLower().Contains(searchText) ?? false) ||
                    client.PhoneNumber.ToLower().Contains(searchText) ||
                    (client.Address?.ToLower().Contains(searchText) ?? false);

                if (!matchesSearch) return false;
            }

            // Фильтр по статусу
            var selectedStatusItem = StatusFilterComboBox.SelectedItem as ComboBoxItem;
            if (selectedStatusItem?.Tag is string selectedStatus && !string.IsNullOrEmpty(selectedStatus))
            {
                if (client.InteractionStatus != selectedStatus) return false;
            }

            return true;
        }

        private void ApplySorting()
        {
            if (_clientsView == null) return;

            _clientsView.SortDescriptions.Clear();

            // Применяем сортировку в зависимости от выбранного варианта
            var sortItem = SortComboBox.SelectedItem as ComboBoxItem;
            string sortTag = sortItem?.Tag as string ?? "SurnameAsc";

            switch (sortTag)
            {
                case "SurnameAsc":
                    _currentSortProperty = "Surname";
                    _currentSortDirection = ListSortDirection.Ascending;
                    break;
                case "SurnameDesc":
                    _currentSortProperty = "Surname";
                    _currentSortDirection = ListSortDirection.Descending;
                    break;
                case "NameAsc":
                    _currentSortProperty = "Name";
                    _currentSortDirection = ListSortDirection.Ascending;
                    break;
                case "NameDesc":
                    _currentSortProperty = "Name";
                    _currentSortDirection = ListSortDirection.Descending;
                    break;
                case "StatusAsc":
                    _currentSortProperty = "InteractionStatus";
                    _currentSortDirection = ListSortDirection.Ascending;
                    break;
                case "IdAsc":
                    _currentSortProperty = "ClientId";
                    _currentSortDirection = ListSortDirection.Ascending;
                    break;
                case "IdDesc":
                    _currentSortProperty = "ClientId";
                    _currentSortDirection = ListSortDirection.Descending;
                    break;
            }

            _clientsView.SortDescriptions.Add(
                new SortDescription(_currentSortProperty, _currentSortDirection));
        }

        private void UpdateFilterStatus()
        {
            if (_clientsView == null) return;

            int filteredCount = 0;
            foreach (var item in _allClients)
            {
                if (ClientFilter(item)) filteredCount++;
            }

            string statusText = "";

            // Текст поиска
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                statusText += $"Поиск: '{SearchTextBox.Text}'";
            }

            // Текст фильтра статуса
            var selectedStatusItem = StatusFilterComboBox.SelectedItem as ComboBoxItem;
            if (selectedStatusItem?.Tag is string selectedStatus && !string.IsNullOrEmpty(selectedStatus))
            {
                if (!string.IsNullOrEmpty(statusText)) statusText += " | ";
                statusText += $"Статус: {selectedStatus}";
            }

            // Текст сортировки
            if (SortComboBox.SelectedItem is ComboBoxItem sortItem)
            {
                if (!string.IsNullOrEmpty(statusText)) statusText += " | ";
                statusText += $"Сортировка: {sortItem.Content}";
            }

            FilterStatusText.Text = statusText;
            StatusText.Text = $"Показано: {filteredCount} из {_allClients.Count} клиентов | {DateTime.Now:HH:mm:ss}";
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadClients();
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            AddClientWindow window = new AddClientWindow();
            window.Owner = this;
            window.Closed += async (s, args) =>
            {
                await LoadClients();
            };
            window.ShowDialog();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySorting();
            UpdateFilterStatus();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _clientsView?.Refresh();
            UpdateFilterStatus();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            StatusFilterComboBox.SelectedIndex = 0;
            SortComboBox.SelectedIndex = 0;

            _clientsView?.Refresh();
            UpdateFilterStatus();
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _clientsView?.Refresh();
            UpdateFilterStatus();
        }

        private void ClientsDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            // Определяем свойство для сортировки
            string propertyName = e.Column.SortMemberPath;
            if (string.IsNullOrEmpty(propertyName)) return;

            // Меняем направление сортировки
            _currentSortProperty = propertyName;
            _currentSortDirection = e.Column.SortDirection != ListSortDirection.Ascending ?
                ListSortDirection.Ascending : ListSortDirection.Descending;

            // Обновляем выпадающий список
            string newTag = propertyName + (_currentSortDirection == ListSortDirection.Ascending ? "Asc" : "Desc");

            foreach (ComboBoxItem item in SortComboBox.Items)
            {
                if (item.Tag as string == newTag)
                {
                    SortComboBox.SelectedItem = item;
                    break;
                }
            }

            ApplySorting();
            UpdateFilterStatus();
        }

        private void SortDirectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string direction = button.Tag as string;

                // Меняем направление сортировки для текущего свойства
                if (direction == "Desc")
                {
                    _currentSortDirection = ListSortDirection.Descending;
                }
                else
                {
                    _currentSortDirection = ListSortDirection.Ascending;
                }

                // Обновляем выпадающий список
                string newTag = _currentSortProperty + (_currentSortDirection == ListSortDirection.Ascending ? "Asc" : "Desc");

                foreach (ComboBoxItem item in SortComboBox.Items)
                {
                    if (item.Tag as string == newTag)
                    {
                        SortComboBox.SelectedItem = item;
                        break;
                    }
                }

                ApplySorting();
                UpdateFilterStatus();
            }
        }
    }

    public class ClientData
    {
        public int ClientId { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string InteractionStatus { get; set; }
    }
}
