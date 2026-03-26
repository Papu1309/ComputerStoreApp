using ComputerStoreApp.Connect;
using ComputerStoreApp.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace ComputerStoreApp.Views
{
    public partial class AdminPanel : Window
    {
        public AdminPanel()
        {
            InitializeComponent();
            LoadProducts();
            LoadEmployees();
            LoadStores();
            LoadReports();
        }

        private void LoadProducts()
        {
            dgProducts.ItemsSource = Connect.Connection.entities.Products.OrderBy(p => p.Name).ToList();
        }

        private void LoadEmployees()
        {
            dgEmployees.ItemsSource = Connect.Connection.entities.Employees.Include("Stores").OrderBy(e => e.FullName).ToList();
        }

        private void LoadStores()
        {
            dgStores.ItemsSource = Connect.Connection.entities.Stores.OrderBy(s => s.Name).ToList();
        }

        private void LoadReports()
        {
            try
            {
                var allSales = Connect.Connection.entities.Sales.ToList();

                var salesByDay = allSales
                    .GroupBy(s => s.SaleDate.Date)
                    .Select(g => new
                    {
                        Дата = g.Key.ToString("dd.MM.yyyy"),
                        Количество_продаж = g.Count(),
                        Сумма = g.Sum(s => s.TotalAmount)
                    })
                    .OrderByDescending(s => s.Дата)
                    .Take(30)
                    .ToList();
                dgSalesByDay.ItemsSource = salesByDay;

                var stockBalances = Connect.Connection.entities.StockBalances
                    .Include("Stores")
                    .Include("Products")
                    .ToList()
                    .Select(sb => new
                    {
                        Магазин = sb.Stores?.Name ?? "Не указан",
                        Товар = sb.Products?.Name ?? "Не указан",
                        Количество = sb.Quantity
                    })
                    .OrderBy(sb => sb.Магазин)
                    .ThenBy(sb => sb.Товар)
                    .ToList();
                dgStockBalances.ItemsSource = stockBalances;

                var saleItems = Connect.Connection.entities.SaleItems
                    .Include("Products")
                    .ToList();

                var topProducts = saleItems
                    .GroupBy(si => si.Products?.Name ?? "Неизвестный товар")
                    .Select(g => new
                    {
                        Товар = g.Key,
                        Продано_штук = g.Sum(si => si.Quantity),
                        Сумма = g.Sum(si => si.Quantity * si.SalePrice)
                    })
                    .OrderByDescending(t => t.Продано_штук)
                    .Take(10)
                    .ToList();
                dgTopProducts.ItemsSource = topProducts;

                var allSalesWithEmployees = Connect.Connection.entities.Sales
                    .Include("Employees")
                    .ToList();

                var employeeStats = allSalesWithEmployees
                    .GroupBy(s => s.Employees?.FullName ?? "Неизвестный сотрудник")
                    .Select(g => new
                    {
                        Сотрудник = g.Key,
                        Количество_продаж = g.Count(),
                        Общая_сумма = g.Sum(s => s.TotalAmount),
                        Средний_чек = g.Average(s => s.TotalAmount)
                    })
                    .OrderByDescending(s => s.Общая_сумма)
                    .ToList();
                dgEmployeeStats.ItemsSource = employeeStats;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчетов: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs evt)
        {
            CreateProductWindow(false).ShowDialog();
        }

        private void EditProduct_Click(object sender, RoutedEventArgs evt)
        {
            if (dgProducts.SelectedItem != null)
                CreateProductWindow(true, dgProducts.SelectedItem as Products).ShowDialog();
            else
                MessageBox.Show("Выберите товар", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private Window CreateProductWindow(bool isEdit, Products product = null)
        {
            var window = new Window
            {
                Title = isEdit ? "Редактирование товара" : "Добавление товара",
                Width = 500,
                Height = 750,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };
            stackPanel.Children.Add(new TextBlock
            {
                Text = isEdit ? "Редактирование товара" : "Новый товар",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var txtSKU = new TextBox { Height = 40, Margin = new Thickness(0, 5, 0, 10) };
            var txtName = new TextBox { Height = 40, Margin = new Thickness(0, 5, 0, 10) };
            var txtCategory = new TextBox { Height = 40, Margin = new Thickness(0, 5, 0, 10) };
            var txtManufacturer = new TextBox { Height = 40, Margin = new Thickness(0, 5, 0, 10) };
            var txtWarranty = new TextBox { Height = 40, Margin = new Thickness(0, 5, 0, 10) };

            if (isEdit && product != null)
            {
                txtSKU.Text = product.SKU;
                txtName.Text = product.Name;
                txtCategory.Text = product.Category;
                txtManufacturer.Text = product.Manufacturer;
                txtWarranty.Text = product.WarrantyMonths.ToString();
            }

            stackPanel.Children.Add(new Label { Content = "Артикул *" });
            stackPanel.Children.Add(txtSKU);
            stackPanel.Children.Add(new Label { Content = "Название *" });
            stackPanel.Children.Add(txtName);
            stackPanel.Children.Add(new Label { Content = "Категория" });
            stackPanel.Children.Add(txtCategory);
            stackPanel.Children.Add(new Label { Content = "Производитель" });
            stackPanel.Children.Add(txtManufacturer);
            stackPanel.Children.Add(new Label { Content = "Гарантия (мес)" });
            stackPanel.Children.Add(txtWarranty);

            var btnSave = new Button
            {
                Content = "Сохранить",
                Height = 40,
                Margin = new Thickness(0, 20, 0, 0),
                Background = (System.Windows.Media.Brush)Application.Current.Resources["SuccessColor"]
            };

            btnSave.Click += (s, args) =>
            {
                if (string.IsNullOrEmpty(txtSKU.Text) || string.IsNullOrEmpty(txtName.Text))
                {
                    MessageBox.Show("Заполните обязательные поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    if (isEdit && product != null)
                    {
                        product.SKU = txtSKU.Text;
                        product.Name = txtName.Text;
                        product.Category = txtCategory.Text;
                        product.Manufacturer = txtManufacturer.Text;
                        product.WarrantyMonths = int.TryParse(txtWarranty.Text, out int w) ? w : 0;
                    }
                    else
                    {
                        Connect.Connection.entities.Products.Add(new Products
                        {
                            SKU = txtSKU.Text,
                            Name = txtName.Text,
                            Category = txtCategory.Text,
                            Manufacturer = txtManufacturer.Text,
                            WarrantyMonths = int.TryParse(txtWarranty.Text, out int w) ? w : 0
                        });
                    }

                    Connect.Connection.entities.SaveChanges();
                    LoadProducts();
                    MessageBox.Show("Товар сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    window.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            stackPanel.Children.Add(btnSave);
            window.Content = stackPanel;
            return window;
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs evt)
        {
            if (dgProducts.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Удалить товар?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var product = dgProducts.SelectedItem as Products;
                var hasSales = Connect.Connection.entities.SaleItems.Any(si => si.ProductId == product.ProductId);

                if (hasSales)
                {
                    MessageBox.Show("Товар участвовал в продажах", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Connect.Connection.entities.Products.Remove(product);
                Connect.Connection.entities.SaveChanges();
                LoadProducts();
            }
        }

        private void RefreshProducts_Click(object sender, RoutedEventArgs evt) => LoadProducts();
        private void AddEmployee_Click(object sender, RoutedEventArgs evt) => new RegistrationWindow { Owner = this }.ShowDialog();

        private void DeleteEmployee_Click(object sender, RoutedEventArgs evt)
        {
            if (dgEmployees.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var employee = dgEmployees.SelectedItem as Employees;
            if (employee.EmployeeId == UserSession.EmployeeId)
            {
                MessageBox.Show("Нельзя удалить себя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить {employee.FullName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var hasSales = Connect.Connection.entities.Sales.Any(s => s.EmployeeId == employee.EmployeeId);

                if (hasSales)
                {
                    MessageBox.Show("Сотрудник проводил продажи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Connect.Connection.entities.Employees.Remove(employee);
                Connect.Connection.entities.SaveChanges();
                LoadEmployees();
            }
        }

        private void RefreshEmployees_Click(object sender, RoutedEventArgs evt) => LoadEmployees();
        private void AddStore_Click(object sender, RoutedEventArgs evt) => CreateStoreWindow(false).ShowDialog();

        private void EditStore_Click(object sender, RoutedEventArgs evt)
        {
            if (dgStores.SelectedItem != null)
                CreateStoreWindow(true, dgStores.SelectedItem as Stores).ShowDialog();
            else
                MessageBox.Show("Выберите магазин", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private Window CreateStoreWindow(bool isEdit, Stores store = null)
        {
            var window = new Window
            {
                Title = isEdit ? "Редактирование магазина" : "Добавление магазина",
                Width = 500,
                Height = 650,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };
            stackPanel.Children.Add(new TextBlock
            {
                Text = isEdit ? "Редактирование магазина" : "Новый магазин",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var txtName = new TextBox { Height = 40, Margin = new Thickness(0, 5, 0, 10) };
            var txtAddress = new TextBox { Height = 40, Margin = new Thickness(0, 5, 0, 10) };
            var txtPhone = new TextBox { Height = 40, Margin = new Thickness(0, 5, 0, 10) };
            var cmbStatus = new ComboBox { Height = 40, Margin = new Thickness(0, 5, 0, 10) };
            cmbStatus.Items.Add("работает");
            cmbStatus.Items.Add("закрыт");
            cmbStatus.Items.Add("на ремонте");
            cmbStatus.SelectedIndex = 0;

            if (isEdit && store != null)
            {
                txtName.Text = store.Name;
                txtAddress.Text = store.Address;
                txtPhone.Text = store.Phone;
                cmbStatus.SelectedItem = store.Status;
            }

            stackPanel.Children.Add(new Label { Content = "Название *" });
            stackPanel.Children.Add(txtName);
            stackPanel.Children.Add(new Label { Content = "Адрес *" });
            stackPanel.Children.Add(txtAddress);
            stackPanel.Children.Add(new Label { Content = "Телефон" });
            stackPanel.Children.Add(txtPhone);
            stackPanel.Children.Add(new Label { Content = "Статус *" });
            stackPanel.Children.Add(cmbStatus);

            var btnSave = new Button
            {
                Content = "Сохранить",
                Height = 40,
                Margin = new Thickness(0, 20, 0, 0),
                Background = (System.Windows.Media.Brush)Application.Current.Resources["SuccessColor"]
            };

            btnSave.Click += (s, args) =>
            {
                if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtAddress.Text))
                {
                    MessageBox.Show("Заполните обязательные поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    if (isEdit && store != null)
                    {
                        store.Name = txtName.Text;
                        store.Address = txtAddress.Text;
                        store.Phone = txtPhone.Text;
                        store.Status = cmbStatus.SelectedItem?.ToString();
                    }
                    else
                    {
                        Connect.Connection.entities.Stores.Add(new Stores
                        {
                            Name = txtName.Text,
                            Address = txtAddress.Text,
                            Phone = txtPhone.Text,
                            Status = cmbStatus.SelectedItem?.ToString()
                        });
                    }

                    Connect.Connection.entities.SaveChanges();
                    LoadStores();
                    MessageBox.Show("Магазин сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    window.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            stackPanel.Children.Add(btnSave);
            window.Content = stackPanel;
            return window;
        }

        private void DeleteStore_Click(object sender, RoutedEventArgs evt)
        {
            if (dgStores.SelectedItem == null)
            {
                MessageBox.Show("Выберите магазин", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var store = dgStores.SelectedItem as Stores;
            if (MessageBox.Show($"Удалить магазин {store.Name}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var hasEmployees = Connect.Connection.entities.Employees.Any(e => e.StoreId == store.StoreId);
                var hasSales = Connect.Connection.entities.Sales.Any(s => s.StoreId == store.StoreId);

                if (hasEmployees || hasSales)
                {
                    MessageBox.Show("У магазина есть сотрудники или продажи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Connect.Connection.entities.Stores.Remove(store);
                Connect.Connection.entities.SaveChanges();
                LoadStores();
            }
        }

        private void RefreshStores_Click(object sender, RoutedEventArgs evt) => LoadStores();
        private void BtnClose_Click(object sender, RoutedEventArgs evt) => Close();
    }
}