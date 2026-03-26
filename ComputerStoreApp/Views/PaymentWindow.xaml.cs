using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using ComputerStoreApp.Connect;
using ComputerStoreApp.Models;
using System.Data.Entity;

namespace ComputerStoreApp.Views
{
    public partial class PaymentWindow : Window
    {
        private decimal _totalAmount;
        private List<CartItem> _cart;
        public bool PaymentSuccessful { get; private set; }

        public PaymentWindow(decimal totalAmount, List<CartItem> cart)
        {
            InitializeComponent();
            _totalAmount = totalAmount;
            _cart = cart;
            txtTotalAmount.Text = $"{totalAmount:C}";
            itemsControl.ItemsSource = cart.Select(item => $"{item.Quantity} x {item.ProductName} = {item.Total:C}").ToList();
            rbCard.Checked += (s, e) => cardPanel.Visibility = Visibility.Visible;
            rbCash.Checked += (s, e) => cardPanel.Visibility = Visibility.Collapsed;
        }

        private void TxtCardNumber_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            string numbers = new string(textBox.Text.Where(char.IsDigit).ToArray());
            if (numbers.Length > 16) numbers = numbers.Substring(0, 16);
            string formatted = "";
            for (int i = 0; i < numbers.Length; i++) { if (i > 0 && i % 4 == 0) formatted += " "; formatted += numbers[i]; }
            textBox.Text = formatted;
            textBox.CaretIndex = textBox.Text.Length;
        }

        private void TxtExpiry_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            string numbers = new string(textBox.Text.Where(char.IsDigit).ToArray());
            if (numbers.Length > 4) numbers = numbers.Substring(0, 4);
            if (numbers.Length >= 2) { int month = int.Parse(numbers.Substring(0, 2)); if (month > 12) month = 12; numbers = month.ToString("D2") + numbers.Substring(2); }
            if (numbers.Length >= 2 && numbers.Length <= 4) { string formatted = numbers.Substring(0, 2); if (numbers.Length > 2) formatted += "/" + numbers.Substring(2); textBox.Text = formatted; textBox.CaretIndex = textBox.Text.Length; }
            else textBox.Text = numbers;
        }

        private void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            if (rbCard.IsChecked == true && !ValidateCardData()) return;
            try
            {
                var sale = new Sales { StoreId = UserSession.StoreId ?? 1, EmployeeId = UserSession.EmployeeId.Value, CustomerId = null, SaleDate = DateTime.Now, TotalAmount = _totalAmount };
                Connect.Connection.entities.Sales.Add(sale);
                Connect.Connection.entities.SaveChanges();
                foreach (var item in _cart)
                {
                    Connect.Connection.entities.SaleItems.Add(new SaleItems { SaleId = sale.SaleId, ProductId = item.ProductId, Quantity = item.Quantity, SalePrice = item.Price });
                    var stockBalance = Connect.Connection.entities.StockBalances.FirstOrDefault(sb => sb.StoreId == UserSession.StoreId && sb.ProductId == item.ProductId);
                    if (stockBalance != null) stockBalance.Quantity -= item.Quantity;
                }
                Connect.Connection.entities.SaveChanges();
                PaymentSuccessful = true;
                ShowOrderPickupInfo();
                Close();
            }
            catch (Exception ex) { ShowError($"Ошибка при оформлении заказа: {ex.Message}"); }
        }

        private bool ValidateCardData()
        {
            string cardNumber = txtCardNumber.Text.Replace(" ", "");
            string expiry = txtExpiry.Text;
            string cvv = txtCVV.Password;
            string cardHolder = txtCardHolder.Text.Trim();
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length != 16 || !Regex.IsMatch(cardNumber, @"^\d+$")) { ShowError("Введите корректный номер карты (16 цифр)"); txtCardNumber.Focus(); return false; }
            if (string.IsNullOrEmpty(expiry) || !Regex.IsMatch(expiry, @"^\d{2}/\d{2}$")) { ShowError("Введите срок действия в формате ММ/ГГ"); txtExpiry.Focus(); return false; }
            string[] expiryParts = expiry.Split('/');
            int month = int.Parse(expiryParts[0]);
            int year = int.Parse(expiryParts[1]) + 2000;
            if (month < 1 || month > 12) { ShowError("Неверный месяц (01-12)"); txtExpiry.Focus(); return false; }
            if (new DateTime(year, month, 1).AddMonths(1).AddDays(-1) < DateTime.Now) { ShowError("Срок действия карты истек"); txtExpiry.Focus(); return false; }
            if (string.IsNullOrEmpty(cvv) || cvv.Length != 3 || !Regex.IsMatch(cvv, @"^\d+$")) { ShowError("Введите корректный CVV код (3 цифры)"); txtCVV.Focus(); return false; }
            if (string.IsNullOrEmpty(cardHolder) || cardHolder.Length < 3) { ShowError("Введите имя владельца карты"); txtCardHolder.Focus(); return false; }
            return true;
        }

        private void ShowOrderPickupInfo()
        {
            var store = Connect.Connection.entities.Stores.FirstOrDefault(s => s.StoreId == UserSession.StoreId && s.Status == "работает") ?? Connect.Connection.entities.Stores.FirstOrDefault(s => s.Status == "работает");
            if (store == null) { MessageBox.Show("Заказ успешно оформлен! Ожидайте звонка оператора.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            DateTime pickupDate = DateTime.Now.AddHours(2);
            if (pickupDate.Hour > 20) pickupDate = pickupDate.AddDays(1).Date.AddHours(10);
            else if (pickupDate.Hour < 9) pickupDate = pickupDate.Date.AddHours(10);
            MessageBox.Show($"✅ ЗАКАЗ УСПЕШНО ОФОРМЛЕН!\n\nНомер заказа: #{new Random().Next(10000, 99999)}\nСумма: {_totalAmount:C}\n\n🏪 МЕСТО ПОЛУЧЕНИЯ:\n{store.Name}\n{store.Address}\nТелефон: {store.Phone}\n\n⏰ ВРЕМЯ ПОЛУЧЕНИЯ:\n{pickupDate:dd.MM.yyyy HH:mm}\n\n📋 Документы для получения:\n- Паспорт или другой документ, удостоверяющий личность\n- Номер заказа (будет отправлен на ваш телефон)\n\nСпасибо за покупку! Ждем вас в нашем магазине!", "Информация о заказе", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowError(string message) { txtError.Text = message; txtError.Visibility = Visibility.Visible; }
    }
}