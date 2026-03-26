using ComputerStoreApp.Connect;
using ComputerStoreApp.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ComputerStoreApp.Views.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadProfileInfo();
            LoadStatistics();
            LoadAdditionalInfo();
        }

        private void LoadProfileInfo()
        {
            try
            {
                txtFullName.Text = UserSession.FullName;
                txtLogin.Text = UserSession.Login;
                txtRole.Text = UserSession.Role;
                txtStore.Text = UserSession.StoreName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки информации профиля: {ex.Message}");
            }
        }

        private void LoadStatistics()
        {
            try
            {
                var employeeSales = Connect.Connection.entities.Sales
                    .Where(s => s.EmployeeId == UserSession.EmployeeId)
                    .ToList();

                int totalSales = employeeSales.Count;
                decimal totalAmount = employeeSales.Sum(s => s.TotalAmount);
                decimal avgCheck = totalSales > 0 ? totalAmount / totalSales : 0;

                txtTotalSales.Text = $"{totalSales} шт.";
                txtTotalAmount.Text = $"{totalAmount:C}";
                txtAvgCheck.Text = $"{avgCheck:C}";

                if (totalSales > 0)
                {
                    var bestDay = employeeSales
                        .GroupBy(s => s.SaleDate.Date)
                        .Select(g => new { Date = g.Key, Total = g.Sum(s => s.TotalAmount) })
                        .OrderByDescending(g => g.Total)
                        .FirstOrDefault();

                    if (bestDay != null)
                    {
                        txtBestDay.Text = $"{bestDay.Date:dd.MM.yyyy} (на сумму {bestDay.Total:C})";
                    }
                    else
                    {
                        txtBestDay.Text = "Нет данных";
                    }
                }
                else
                {
                    txtBestDay.Text = "Нет продаж";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}");
                txtTotalSales.Text = "0";
                txtTotalAmount.Text = "0 ₽";
                txtAvgCheck.Text = "0 ₽";
                txtBestDay.Text = "Нет данных";
            }
        }

        private void LoadAdditionalInfo()
        {
            try
            {
                var employee = Connect.Connection.entities.Employees
                    .FirstOrDefault(e => e.EmployeeId == UserSession.EmployeeId);

                if (employee != null)
                {
                    var firstSale = Connect.Connection.entities.Sales
                        .Where(s => s.EmployeeId == UserSession.EmployeeId)
                        .OrderBy(s => s.SaleDate)
                        .FirstOrDefault();

                    if (firstSale != null)
                    {
                        txtRegistrationDate.Text = firstSale.SaleDate.ToString("dd.MM.yyyy");
                    }
                    else
                    {
                        txtRegistrationDate.Text = "Нет данных";
                    }

                    txtStatus.Text = "Активен";

                    var lastSale = Connect.Connection.entities.Sales
                        .Where(s => s.EmployeeId == UserSession.EmployeeId)
                        .OrderByDescending(s => s.SaleDate)
                        .FirstOrDefault();

                    if (lastSale != null)
                    {
                        txtLastActivity.Text = lastSale.SaleDate.ToString("dd.MM.yyyy HH:mm");
                    }
                    else
                    {
                        txtLastActivity.Text = "Нет активности";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дополнительной информации: {ex.Message}");
                txtRegistrationDate.Text = "Нет данных";
                txtStatus.Text = "Активен";
                txtLastActivity.Text = "Нет активности";
            }
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs eventArgs)
        {
            var changePasswordWindow = new Window
            {
                Title = "Смена пароля",
                Width = 450,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(25) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "🔒 Смена пароля",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = (System.Windows.Media.Brush)Application.Current.Resources["PrimaryColor"]
            });

            stackPanel.Children.Add(new Label { Content = "Текущий пароль:", FontSize = 14 });
            var txtCurrentPassword = new PasswordBox { Height = 40, Margin = new Thickness(0, 5, 0, 15) };
            stackPanel.Children.Add(txtCurrentPassword);

            stackPanel.Children.Add(new Label { Content = "Новый пароль:", FontSize = 14 });
            var txtNewPassword = new PasswordBox { Height = 40, Margin = new Thickness(0, 5, 0, 15) };
            stackPanel.Children.Add(txtNewPassword);

            stackPanel.Children.Add(new Label { Content = "Подтверждение пароля:", FontSize = 14 });
            var txtConfirmPassword = new PasswordBox { Height = 40, Margin = new Thickness(0, 5, 0, 20) };
            stackPanel.Children.Add(txtConfirmPassword);

            var btnSave = new Button
            {
                Content = "Сохранить",
                Height = 40,
                FontSize = 14,
                Background = (System.Windows.Media.Brush)Application.Current.Resources["SuccessColor"]
            };

            btnSave.Click += (s, args) =>
            {
                string currentPassword = txtCurrentPassword.Password;
                string newPassword = txtNewPassword.Password;
                string confirmPassword = txtConfirmPassword.Password;

                if (string.IsNullOrEmpty(currentPassword))
                {
                    MessageBox.Show("Введите текущий пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtCurrentPassword.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(newPassword))
                {
                    MessageBox.Show("Введите новый пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewPassword.Focus();
                    return;
                }

                if (newPassword.Length < 4)
                {
                    MessageBox.Show("Новый пароль должен содержать минимум 4 символа", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewPassword.Focus();
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtConfirmPassword.Focus();
                    return;
                }

                try
                {
                    var employee = Connect.Connection.entities.Employees
                        .FirstOrDefault(emp => emp.EmployeeId == UserSession.EmployeeId);

                    if (employee == null)
                    {
                        MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (employee.PasswordHash != currentPassword)
                    {
                        MessageBox.Show("Неверный текущий пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        txtCurrentPassword.Focus();
                        return;
                    }

                    employee.PasswordHash = newPassword;
                    Connect.Connection.entities.SaveChanges();

                    MessageBox.Show("Пароль успешно изменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    changePasswordWindow.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка изменения пароля: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            stackPanel.Children.Add(btnSave);
            changePasswordWindow.Content = stackPanel;
            changePasswordWindow.ShowDialog();
        }
    }
}