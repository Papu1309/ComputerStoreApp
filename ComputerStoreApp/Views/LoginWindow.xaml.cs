using System;
using System.Linq;
using System.Windows;
using ComputerStoreApp.Connect;
using ComputerStoreApp.Models;

namespace ComputerStoreApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            txtLogin.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            try
            {
                var employee = Connection.entities.Employees
                    .FirstOrDefault(emp => emp.Login == login);

                if (employee == null)
                {
                    ShowError("Неверный логин или пароль");
                    return;
                }

                if (employee.PasswordHash != password)
                {
                    ShowError("Неверный логин или пароль");
                    return;
                }

                string storeName = "";
                if (employee.StoreId.HasValue)
                {
                    var store = Connection.entities.Stores
                        .FirstOrDefault(s => s.StoreId == employee.StoreId);
                    storeName = store?.Name ?? "";
                }

                UserSession.EmployeeId = employee.EmployeeId;
                UserSession.FullName = employee.FullName;
                UserSession.Login = employee.Login;
                UserSession.Role = employee.Role;
                UserSession.StoreId = employee.StoreId;
                UserSession.StoreName = storeName;

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подключения к базе данных: {ex.Message}");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow regWindow = new RegistrationWindow();
            regWindow.Owner = this;
            regWindow.ShowDialog();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}