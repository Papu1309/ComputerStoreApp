using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ComputerStoreApp.Models;
using ComputerStoreApp.Views;

namespace ComputerStoreApp.Views.Pages
{
    public partial class CartPage : Page
    {
        private static ObservableCollection<CartItem> _cart = new ObservableCollection<CartItem>();

        public CartPage()
        {
            InitializeComponent();
            UpdateCartDisplay();
        }

        private static Window GetCurrentWindow()
        {
            return Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        }

        public static void AddToCart(int productId, string productName, decimal price, string sku)
        {
            var existingItem = _cart.FirstOrDefault(item => item.ProductId == productId);
            if (existingItem != null)
                existingItem.Quantity++;
            else
                _cart.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    SKU = sku,
                    Quantity = 1,
                    Price = price
                });
        }

        private void UpdateCartDisplay()
        {
            dgCart.ItemsSource = _cart;
            if (_cart.Count == 0)
            {
                dgCart.Visibility = Visibility.Collapsed;
                emptyCartPanel.Visibility = Visibility.Visible;
                btnClearCart.IsEnabled = false;
                btnCheckout.IsEnabled = false;
            }
            else
            {
                dgCart.Visibility = Visibility.Visible;
                emptyCartPanel.Visibility = Visibility.Collapsed;
                btnClearCart.IsEnabled = true;
                btnCheckout.IsEnabled = true;
            }
            txtTotalAmount.Text = $"{_cart.Sum(item => item.Total):C}";
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int productId = (int)button.Tag;
            var itemToRemove = _cart.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null)
            {
                _cart.Remove(itemToRemove);
                UpdateCartDisplay();
            }
        }

        private void BtnClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите очистить корзину?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _cart.Clear();
                UpdateCartDisplay();
            }
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var paymentWindow = new PaymentWindow(_cart.Sum(i => i.Total), _cart.ToList())
            {
                Owner = GetCurrentWindow()
            };

            paymentWindow.ShowDialog();
            if (paymentWindow.PaymentSuccessful)
            {
                _cart.Clear();
                UpdateCartDisplay();
                MessageBox.Show("Спасибо за покупку! Заказ успешно оформлен.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}