using ComputerStoreApp.Connect;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ComputerStoreApp.Views
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
            LoadStores();
        }

        private void LoadStores()
        {
            try
            {
                var stores = Connection.entities.Stores
                    .Where(s => s.Status == "работает")
                    .OrderBy(s => s.Name)
                    .ToList();

                cmbStore.ItemsSource = stores;
                if (stores.Any()) cmbStore.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки магазинов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;
            string role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString();
            int? storeId = cmbStore.SelectedValue as int?;
            string phone = txtPhone.Text.Trim();

            if (string.IsNullOrEmpty(fullName)) { ShowError("Введите ФИО"); txtFullName.Focus(); return; }
            if (string.IsNullOrEmpty(login)) { ShowError("Введите логин"); txtLogin.Focus(); return; }
            if (login.Length < 3) { ShowError("Логин должен содержать минимум 3 символа"); txtLogin.Focus(); return; }
            if (string.IsNullOrEmpty(password)) { ShowError("Введите пароль"); txtPassword.Focus(); return; }
            if (password.Length < 4) { ShowError("Пароль должен содержать минимум 4 символа"); txtPassword.Focus(); return; }
            if (password != confirmPassword) { ShowError("Пароли не совпадают"); txtConfirmPassword.Focus(); return; }
            if (string.IsNullOrEmpty(role)) { ShowError("Выберите роль"); return; }
            if (!storeId.HasValue) { ShowError("Выберите магазин"); return; }

            try
            {
                var existingEmployee = Connection.entities.Employees.FirstOrDefault(emp => emp.Login == login);
                if (existingEmployee != null) { ShowError("Пользователь с таким логином уже существует"); txtLogin.Focus(); return; }

                var newEmployee = new Employees
                {
                    FullName = fullName,
                    Login = login,
                    PasswordHash = password,
                    Role = role,
                    StoreId = storeId
                };

                Connection.entities.Employees.Add(newEmployee);
                Connection.entities.SaveChanges();

                MessageBox.Show($"Сотрудник {fullName} успешно зарегистрирован!\n\nЛогин: {login}\nРоль: {role}",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}