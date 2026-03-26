using System.Windows;
using ComputerStoreApp.Models;
using ComputerStoreApp.Views.Pages;

namespace ComputerStoreApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtUserInfo.Text = $"{UserSession.FullName} ({UserSession.Role})";
            txtStoreInfo.Text = $"🏪 {UserSession.StoreName}";
            if (UserSession.IsAdmin) btnAdmin.Visibility = Visibility.Visible;
            MainFrame.Navigate(new ProductsPage());
        }


        private void BtnProducts_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProductsPage());
        private void BtnCart_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CartPage());
        private void BtnOrders_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new OrdersPage());
        private void BtnProfile_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());
        private void BtnAdmin_Click(object sender, RoutedEventArgs e) { new AdminPanel { Owner = this }.ShowDialog(); }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                UserSession.Clear();
                new LoginWindow().Show();
                Close();
            }
        }
    }
}