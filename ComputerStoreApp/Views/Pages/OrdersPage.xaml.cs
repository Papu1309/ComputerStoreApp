using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ComputerStoreApp.Connect;
using ComputerStoreApp.Models;
using ComputerStoreApp.Views.Pages;
using ComputerStoreApp.Views;
using ComputerStoreApp.Properties;
using System.Data.Entity;

namespace ComputerStoreApp.Views.Pages
{
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                // Получаем заказы текущего сотрудника
                var orders = Connect.Connection.entities.Sales
                    .Where(s => s.EmployeeId == UserSession.EmployeeId)
                    .Include("Stores")
                    .Include("Employees")
                    .OrderByDescending(s => s.SaleDate)
                    .ToList();

                if (orders != null && orders.Count > 0)
                {
                    // Есть заказы - показываем таблицу, скрываем панель "нет заказов"
                    ordersGrid.Visibility = Visibility.Visible;
                    emptyOrdersPanel.Visibility = Visibility.Collapsed;
                    dgOrders.ItemsSource = orders;

                    // Если есть заказы, показываем детали первого заказа
                    if (orders.Count > 0)
                    {
                        LoadOrderDetails(orders[0].SaleId);
                    }
                }
                else
                {
                    // Нет заказов - скрываем таблицу, показываем панель "нет заказов"
                    ordersGrid.Visibility = Visibility.Collapsed;
                    emptyOrdersPanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ordersGrid.Visibility = Visibility.Collapsed;
                emptyOrdersPanel.Visibility = Visibility.Visible;
            }
        }

        private void LoadOrderDetails(int saleId)
        {
            try
            {
                var orderItems = Connect.Connection.entities.SaleItems
                    .Where(si => si.SaleId == saleId)
                    .Include("Products")
                    .ToList();

                dgOrderItems.ItemsSource = orderItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей заказа: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrders.SelectedItem == null) return;

            var selectedOrder = dgOrders.SelectedItem as Sales;
            if (selectedOrder != null)
            {
                LoadOrderDetails(selectedOrder.SaleId);
            }
        }

        private void GoToCatalog_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу каталога товаров
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainFrame.Navigate(new ProductsPage());
            }
        }
    }
}